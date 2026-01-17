using Itmo.Dev.Platform.Events;
using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Contracts.Events;
using PaymentService.Application.Contracts.Loyalty;
using PaymentService.Application.Contracts.Payments;
using PaymentService.Application.Models.Payments;
using PaymentService.Application.Models.Transactions;
using PaymentService.Application.Models.Wallets;
using System.Transactions;

namespace PaymentService.Application.Payments;

public class PaymentsService : IPaymentService
{
    private const int ZeroValue = 0;

    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserLoyaltyClient _userLoyaltyClient;

    public PaymentsService(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IEventPublisher eventPublisher,
        IUserLoyaltyClient userLoyaltyClient)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _eventPublisher = eventPublisher;
        _userLoyaltyClient = userLoyaltyClient;
    }

    public async Task<Payment?> GetByIdAsync(long paymentId, CancellationToken cancellationToken)
    {
        Payment? payment = await _paymentRepository.GetAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            throw new PaymentException("payment not found");
        }

        return payment;
    }

    public async Task<IAsyncEnumerable<Payment>> GetPaymentsAsync(
        long walletId,
        CancellationToken cancellationToken,
        long? cursor)
    {
        IAsyncEnumerable<Payment> payments = _paymentRepository.GetByWalletIdAsync(walletId, cancellationToken, cursor);

        if (await payments.CountAsync(cancellationToken: cancellationToken) == ZeroValue)
        {
            throw new PaymentException("there are no payments");
        }

        return payments;
    }

    public async Task<long> CreatePaymentAsync(long userId, long amount, CancellationToken cancellationToken)
    {
        Wallet? wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
        {
            throw new PaymentException("wallet not found");
        }

        if (wallet.IsBlocked)
        {
            throw new PaymentException("wallet is blocked");
        }

        UserDiscount discount = await _userLoyaltyClient.GetUserLoyalty(wallet.UserId, cancellationToken);

        long finalAmount = amount - (amount * discount.DiscountPercent / 100);

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        long? paymentId = await _paymentRepository.CreateAsync(wallet.Id, finalAmount, cancellationToken);

        if (paymentId == null)
        {
            throw new PaymentException("payment can't be created");
        }

        var evt = new PaymentPendingEvent((long)paymentId, wallet.Id, finalAmount, wallet.UserId);
        await _eventPublisher.PublishAsync(evt, cancellationToken);

        scope.Complete();
        return (long)paymentId;
    }

    public async Task<PayResult> TryPayAsync(long paymentId, CancellationToken cancellationToken)
    {
        Payment? payment = await _paymentRepository.GetAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return new(false, PaymentFailReason.PaymentNotFound);
        }

        if (payment.Status == PaymentStatus.Succeeded)
        {
            return new(true, null);
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            await TransferPaymentStatusToFailedAsync(paymentId, cancellationToken);
            return new(false, PaymentFailReason.InternalError);
        }

        Wallet? wallet = await _walletRepository.GetByIdAsync(payment.WalletId, cancellationToken);
        if (wallet is null)
        {
            await TransferPaymentStatusToFailedAsync(paymentId, cancellationToken);
            return new(false, PaymentFailReason.WalletNotFound);
        }

        if (wallet.IsBlocked)
        {
            await TransferPaymentStatusToFailedAsync(paymentId, cancellationToken);
            return new(false, PaymentFailReason.UserIsBlocked);
        }

        if (wallet.Balance < payment.Amount)
        {
            await TransferPaymentStatusToFailedAsync(paymentId, cancellationToken);
            return new(false, PaymentFailReason.NotEnoughMoney);
        }

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        long newBalance = wallet.Balance - payment.Amount;

        await _walletRepository.UpdateAsync(wallet.Id, newBalance, cancellationToken);

        long? txId = await _walletTransactionRepository.CreateAsync(
            wallet.Id,
            TransactionType.Payment,
            payment.Amount,
            cancellationToken,
            paymentId);

        if (txId is null)
        {
            throw new PaymentException("transaction can't be created");
        }

        await TransferPaymentStatusToSucceededAsync(
            paymentId,
            wallet.Id,
            payment.Amount,
            wallet.UserId,
            cancellationToken);

        scope.Complete();
        return new(true, null);
    }

    public async Task TransferPaymentStatusToRefundedAsync(long paymentId, CancellationToken cancellationToken)
    {
        Payment? payment = await _paymentRepository.GetAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            throw new PaymentException("payment not found");
        }

        if (payment.Status != PaymentStatus.Succeeded)
        {
            throw new PaymentException("payment status must be succeeded");
        }

        Wallet? wallet = await _walletRepository.GetByIdAsync(payment.WalletId, cancellationToken);

        if (wallet == null)
        {
            throw new PaymentException("wallet not found");
        }

        if (wallet.IsBlocked)
        {
            throw new PaymentException("wallet is blocked");
        }

        long newBalance = wallet.Balance + payment.Amount;

        var evt = new PaymentRefundedEvent(paymentId, wallet.Id, payment.Amount, wallet.UserId);

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _walletRepository.UpdateAsync(wallet.Id, newBalance, cancellationToken);

        long? txId = await _walletTransactionRepository.CreateAsync(
            wallet.Id,
            TransactionType.Refund,
            payment.Amount,
            cancellationToken,
            paymentId);

        if (txId == null)
        {
            throw new PaymentException("transaction can't be created");
        }

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        await _paymentRepository.UpdatePaymentAsync(paymentId, PaymentStatus.Refunded, cancellationToken);
        scope.Complete();
    }

    private async Task TransferPaymentStatusToSucceededAsync(
        long paymentId,
        long walletId,
        long amount,
        long userId,
        CancellationToken cancellationToken)
    {
        var evt = new PaymentSucceededEvent(paymentId, walletId, amount, userId);

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        await _paymentRepository.UpdatePaymentAsync(paymentId, PaymentStatus.Succeeded, cancellationToken);
    }

    private async Task TransferPaymentStatusToFailedAsync(
        long paymentId,
        CancellationToken cancellationToken)
    {
        var evt = new PaymentFailedEvent(paymentId);

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _paymentRepository.UpdatePaymentAsync(paymentId, PaymentStatus.Failed, cancellationToken);
        await _eventPublisher.PublishAsync(evt, cancellationToken);
        scope.Complete();
    }
}