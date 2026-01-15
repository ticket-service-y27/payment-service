using Itmo.Dev.Platform.Events;
using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Contracts.Events;
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

    public PaymentsService(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IEventPublisher eventPublisher)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _eventPublisher = eventPublisher;
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

    public async Task<long> CreatePaymentAsync(long walletId, long amount, CancellationToken cancellationToken)
    {
        Wallet? wallet = await _walletRepository.GetByIdAsync(walletId, cancellationToken);

        if (wallet == null)
        {
            throw new PaymentException("wallet not found");
        }

        if (wallet.IsBlocked)
        {
            throw new PaymentException("wallet is blocked");
        }

        if (amount <= ZeroValue)
        {
            throw new PaymentException("amount must be more than zero");
        }

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        long? paymentId = await _paymentRepository.CreateAsync(walletId, amount, cancellationToken);

        if (paymentId == null)
        {
            throw new PaymentException("payment can't be created");
        }

        var evt = new PaymentPendingEvent((long)paymentId, walletId, amount, wallet.UserId);
        await _eventPublisher.PublishAsync(evt, cancellationToken);

        scope.Complete();
        return (long)paymentId;
    }

    public async Task TransferPaymentStatusToSucceededAsync(long paymentId, CancellationToken cancellationToken)
    {
        Payment? payment = await _paymentRepository.GetAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            throw new PaymentException("payment not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new PaymentException("payment status must be pending");
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

        if (wallet.Balance <= payment.Amount)
        {
            throw new PaymentException("not enough money");
        }

        long newBalance = wallet.Balance - payment.Amount;

        var evt = new PaymentSucceededEvent(paymentId, wallet.Id, payment.Amount, wallet.UserId);

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _walletRepository.UpdateAsync(wallet.Id, newBalance, cancellationToken);

        long? transactionId = await _walletTransactionRepository.CreateAsync(
            wallet.Id,
            TransactionType.Payment,
            payment.Amount,
            cancellationToken,
            paymentId);

        if (transactionId == null)
        {
            throw new PaymentException("transaction can't be created");
        }

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        await _paymentRepository.UpdatePaymentAsync(paymentId, PaymentStatus.Succeeded, cancellationToken);
        scope.Complete();
    }

    public async Task TransferPaymentStatusToFailedAsync(long paymentId, CancellationToken cancellationToken)
    {
        Payment? payment = await _paymentRepository.GetAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            throw new PaymentException("payment not found");
        }

        Wallet? wallet = await _walletRepository.GetByIdAsync(payment.WalletId, cancellationToken);
        if (wallet == null)
        {
            throw new PaymentException("wallet not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new PaymentException("payment status must be pending");
        }

        var evt = new PaymentFailedEvent(paymentId, payment.WalletId, payment.Amount, wallet.UserId);

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _paymentRepository.UpdatePaymentAsync(paymentId, PaymentStatus.Failed, cancellationToken);
        await _eventPublisher.PublishAsync(evt, cancellationToken);
        scope.Complete();
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

        long? transactionId = await _walletTransactionRepository.CreateAsync(
            wallet.Id,
            TransactionType.Refund,
            payment.Amount,
            cancellationToken,
            paymentId);

        if (transactionId == null)
        {
            throw new PaymentException("transaction can't be created");
        }

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        await _paymentRepository.UpdatePaymentAsync(paymentId, PaymentStatus.Refunded, cancellationToken);
        scope.Complete();
    }
}