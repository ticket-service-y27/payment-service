using PaymentService.Application.Contracts.Loyalty;
using Users.UserService.Contracts;

namespace PaymentService.Presentation.Grpc.Clients;

public class UserLoyaltyClient : IUserLoyaltyClient
{
    private readonly UserLoyaltyService.UserLoyaltyServiceClient _client;

    public UserLoyaltyClient(UserLoyaltyService.UserLoyaltyServiceClient client)
    {
        _client = client;
    }

    public async Task<UserDiscount> GetUserLoyalty(long userId, CancellationToken cancellationToken)
    {
        var request = new GetUserDiscountRequest
        {
            UserId = userId,
        };

        GetUserDiscountResponse response =
            await _client.GetUserDiscountAsync(request, cancellationToken: cancellationToken);

        return new UserDiscount(response.DiscountPercent, response.IsBlocked);
    }
}