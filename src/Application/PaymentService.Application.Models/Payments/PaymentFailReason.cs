namespace PaymentService.Application.Models.Payments;

public enum PaymentFailReason
{
    NotEnoughMoney,
    UserIsBlocked,
    InternalError,
    PaymentNotFound,
    WalletNotFound,
}