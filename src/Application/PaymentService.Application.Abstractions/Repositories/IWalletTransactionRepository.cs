using PaymentService.Application.Models.Transactions;

namespace PaymentService.Application.Abstractions.Repositories;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(long transactionId, CancellationToken ct);

    Task<long?> CreateAsync(long walletId, TransactionType type, long amount, CancellationToken ct, long? paymentId);

    IAsyncEnumerable<WalletTransaction> GetByWalletIdAsync(
        long walletId,
        CancellationToken ct,
        DateTimeOffset? from,
        DateTimeOffset? last,
        long? cursor);
}