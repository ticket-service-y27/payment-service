using PaymentService.Application.Models.Payments;
using PaymentService.Application.Models.Payments.Payloads;

namespace PaymentService.Application.Abstractions.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetAsync(long paymentId, CancellationToken cancellationToken);

    Task<long?> CreateAsync(long walletId, long amount, PaymentPayload payload, CancellationToken cancellationToken);

    Task UpdatePaymentAsync(long paymentId, PaymentStatus status, CancellationToken cancellationToken);

    IAsyncEnumerable<Payment> GetByWalletIdAsync(
        long walletId,
        CancellationToken cancellationToken,
        long? cursor);
}