namespace PaymentService.Application.Models.Transactions;

public record WalletTransaction(
    long Id,
    long WalletId,
    TransactionType Type,
    long Amount,
    long? PaymentId,
    DateTimeOffset CreatedAt);