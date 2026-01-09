using PaymentService.Application.Models.Transactions;

namespace PaymentService.Application.Contracts.WalletTransactions;

public interface IWalletTransactionsService
{
    Task<WalletTransaction?> GetByIdAsync(long transactionId, CancellationToken ct);

    Task<IAsyncEnumerable<WalletTransaction>> GetByWalletIdAsync(long walletId, CancellationToken ct);

    Task<WalletTransaction> CreateAsync(long walletId, TransactionType type, long amount, CancellationToken ct, long? paymentId);
}