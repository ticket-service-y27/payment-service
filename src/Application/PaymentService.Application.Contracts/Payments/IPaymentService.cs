using PaymentService.Application.Models.Payments;

namespace PaymentService.Application.Contracts.Payments;

public interface IPaymentService
{
    Task<Payment?> GetPaymentByIdAsync(long paymentId, CancellationToken cancellationToken);

    Task<IAsyncEnumerable<Payment>> GetPaymentsAsync(long walletId, CancellationToken cancellationToken, long? cursor);

    Task<long> CreatePaymentAsync(long walletId, long amount, CancellationToken cancellationToken);

    Task TransferPaymentStatusToSucceededAsync(long paymentId, CancellationToken cancellationToken);

    Task TransferPaymentStatusToFailedAsync(long paymentId, CancellationToken cancellationToken);

    Task TransferPaymentStatusToRefundedAsync(long paymentId, CancellationToken cancellationToken);
}
