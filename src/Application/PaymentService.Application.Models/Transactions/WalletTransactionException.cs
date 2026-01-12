namespace PaymentService.Application.Models.Transactions;

public class WalletTransactionException : Exception
{
    public WalletTransactionException(string message) : base(message) { }
}