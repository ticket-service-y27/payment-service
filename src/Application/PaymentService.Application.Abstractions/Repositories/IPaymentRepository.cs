using PaymentService.Application.Models.Payments;

namespace PaymentService.Application.Abstractions.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetAsync(long paymentId, CancellationToken cancellationToken);

    Task CreateAsync(long walletId, long amount, CancellationToken cancellationToken);

    Task UpdatePaymentAsync(long paymentId, PaymentStatus status, CancellationToken cancellationToken);

    IAsyncEnumerable<Payment> GetByWalletIdAsync(
        long walletId,
        CancellationToken cancellationToken,
        long? cursor);
}