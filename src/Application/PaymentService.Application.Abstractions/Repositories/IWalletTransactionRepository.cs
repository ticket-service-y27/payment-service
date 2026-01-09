using PaymentService.Application.Models.Transactions;

namespace PaymentService.Application.Abstractions.Repositories;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(long transactionId, CancellationToken ct);

    Task<WalletTransaction> CreateAsync(long walletId, TransactionType type, long amount, CancellationToken ct, long? paymentId);

    IAsyncEnumerable<WalletTransaction> GetByWalletIdAsync(
        long walletId,
        CancellationToken ct,
        long? cursor,
        DateTimeOffset? from,
        DateTimeOffset? last);
}