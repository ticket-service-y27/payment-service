namespace PaymentService.Application.Models.Payments;

public record Payment(
    long Id,
    long WalletId,
    long Amount,
    PaymentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);