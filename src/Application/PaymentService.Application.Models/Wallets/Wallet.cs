namespace PaymentService.Application.Models.Wallets;

public record Wallet(
    long Id,
    long UserId,
    long Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);