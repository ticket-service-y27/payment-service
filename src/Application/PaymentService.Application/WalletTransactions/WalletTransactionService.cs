using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Contracts.WalletTransactions;
using PaymentService.Application.Models.Transactions;

namespace PaymentService.Application.WalletTransactions;

public class WalletTransactionService : IWalletTransactionService
{
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public WalletTransactionService(IWalletTransactionRepository walletTransactionRepository)
    {
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<WalletTransaction?> GetByIdAsync(long transactionId, CancellationToken ct)
    {
        WalletTransaction? transaction = await _walletTransactionRepository.GetByIdAsync(transactionId, ct);

        if (transaction == null)
        {
            throw new WalletTransactionException("transaction not found");
        }

        return transaction;
    }

    public async Task<IAsyncEnumerable<WalletTransaction>> GetByWalletIdAsync(
        long walletId,
        CancellationToken ct,
        long? cursor,
        DateTimeOffset? from,
        DateTimeOffset? last)
    {
        IAsyncEnumerable<WalletTransaction> transactions =
            _walletTransactionRepository.GetByWalletIdAsync(walletId, ct, from, last, cursor);

        if (await transactions.CountAsync(ct) == 0)
        {
            throw new WalletTransactionException("transactions not found");
        }

        return transactions;
    }
}