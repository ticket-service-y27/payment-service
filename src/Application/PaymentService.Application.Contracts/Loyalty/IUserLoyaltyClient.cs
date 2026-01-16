namespace PaymentService.Application.Contracts.Loyalty;

public interface IUserLoyaltyClient
{
    Task<UserDiscount> GetUserLoyalty(long userId, CancellationToken cancellationToken);
}