using PaymentService.Application.Models.Payments;

namespace PaymentService.Application.Contracts.Payments;

public interface IPaymentService
{
    Task<Payment?> GetByIdAsync(long paymentId, CancellationToken cancellationToken);

    Task<IAsyncEnumerable<Payment>> GetPaymentsAsync(long walletId, CancellationToken cancellationToken, long? cursor);

    Task<long> CreatePaymentAsync(long userId, long amount, CancellationToken cancellationToken);

    Task<PayResult> TryPayAsync(long paymentId, CancellationToken cancellationToken);

    Task TransferPaymentStatusToRefundedAsync(long paymentId, CancellationToken cancellationToken);
}
