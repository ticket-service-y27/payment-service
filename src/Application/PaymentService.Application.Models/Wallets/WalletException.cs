namespace PaymentService.Application.Models.Wallets;

public class WalletException : Exception
{
    public WalletException(string message) : base(message) { }
}