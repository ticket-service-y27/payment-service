namespace PaymentService.Application.Models.Payments;

public class PaymentException : Exception
{
    public PaymentException(string message) : base(message) { }
}