namespace PaymentService.Application.Models.Wallets;

public record Wallet(
    long Id,
    long UserId,
    long Balance,
    bool IsBlocked,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);