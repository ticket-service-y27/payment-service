using Google.Protobuf.WellKnownTypes;
using PaymentService.Grpc.Payments;
using PaymentService.Grpc.WalletTransactions;

namespace PaymentService.Presentation.Grpc.Mapper;

public class ModelMapper
{
    public WalletTransaction Map(Application.Models.Transactions.WalletTransaction tx)
    {
        return new WalletTransaction
        {
            TransactionId = tx.Id,
            WalletId = tx.WalletId,
            Type = Map(tx.Type),
            Amount = tx.Amount,
            PaymentId = tx.PaymentId ?? 0,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(tx.CreatedAt),
        };
    }

    public Payment Map(Application.Models.Payments.Payment payment)
    {
        return new Payment
        {
            PaymentId = payment.Id,
            WalletId = payment.WalletId,
            Amount = payment.Amount,
            Status = Map(payment.Status),
            CreatedAt = Timestamp.FromDateTimeOffset(payment.CreatedAt),
            UpdatedAt = Timestamp.FromDateTimeOffset(payment.UpdatedAt),
        };
    }

    private TransactionType Map(Application.Models.Transactions.TransactionType type)
    {
        return type switch
        {
            Application.Models.Transactions.TransactionType.Topup => TransactionType.Topup,
            Application.Models.Transactions.TransactionType.Payment => TransactionType.Payment,
            Application.Models.Transactions.TransactionType.Refund => TransactionType.Refund,
            _ => TransactionType.Unspecified,
        };
    }

    private PaymentStatus Map(Application.Models.Payments.PaymentStatus status)
    {
        return status switch
        {
            Application.Models.Payments.PaymentStatus.Pending => PaymentStatus.Pending,
            Application.Models.Payments.PaymentStatus.Succeeded => PaymentStatus.Succeeded,
            Application.Models.Payments.PaymentStatus.Failed => PaymentStatus.Failed,
            Application.Models.Payments.PaymentStatus.Refunded => PaymentStatus.Refunded,
            _ => PaymentStatus.Unspecified,
        };
    }
}