namespace PaymentService.Application.Models.Payments;

public record Payment(
    long Id,
    long WalletId,
    PaymentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);