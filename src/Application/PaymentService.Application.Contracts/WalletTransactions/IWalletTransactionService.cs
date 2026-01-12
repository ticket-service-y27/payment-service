using PaymentService.Application.Models.Transactions;

namespace PaymentService.Application.Contracts.WalletTransactions;

public interface IWalletTransactionService
{
    Task<WalletTransaction?> GetByIdAsync(long transactionId, CancellationToken ct);

    Task<IAsyncEnumerable<WalletTransaction>> GetByWalletIdAsync(
        long walletId,
        CancellationToken ct,
        long? cursor,
        DateTimeOffset? from,
        DateTimeOffset? last);
}