using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Contracts.Wallets;
using PaymentService.Application.Models.Transactions;
using PaymentService.Application.Models.Wallets;
using System.Transactions;

namespace PaymentService.Application.Wallets;

public class WalletService : IWalletService
{
    private const int ZeroValue = 0;

    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public WalletService(IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository)
    {
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<long> CreateWalletAsync(long userId, CancellationToken cancellationToken)
    {
        Wallet? wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet != null)
        {
            throw new WalletException("Wallet already exists");
        }

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        long? walletId = await _walletRepository.CreateAsync(userId, cancellationToken);

        if (walletId == null)
        {
            throw new WalletException("could not create wallet");
        }

        scope.Complete();
        return (long)walletId;
    }

    public async Task TopUpWalletAsync(long walletId, long amount, CancellationToken cancellationToken)
    {
        Wallet? wallet = await _walletRepository.GetByIdAsync(walletId, cancellationToken);

        if (wallet == null)
        {
            throw new WalletException("wallet not found");
        }

        if (wallet.IsBlocked)
        {
            throw new WalletException("wallet is blocked");
        }

        if (amount <= ZeroValue)
        {
            throw new WalletException("amount must be greater than zero");
        }

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        long newBalance = wallet.Balance + amount;

        await _walletRepository.UpdateAsync(walletId, newBalance, cancellationToken);

        long? transactionId = await _walletTransactionRepository.CreateAsync(
            wallet.Id,
            TransactionType.Topup,
            amount,
            cancellationToken,
            paymentId: null);

        if (transactionId == null)
        {
            throw new WalletException("could not create transaction");
        }

        scope.Complete();
    }

    public async Task SetBlockStatusAsync(long userId, bool isBlocked, CancellationToken cancellationToken)
    {
        Wallet? wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
        {
            throw new WalletException("wallet not found");
        }

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _walletRepository.SetBlockedAsync(wallet.Id, isBlocked, cancellationToken);
        scope.Complete();
    }
}