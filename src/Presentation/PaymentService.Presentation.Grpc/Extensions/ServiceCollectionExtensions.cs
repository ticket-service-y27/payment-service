using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentService.Application.Contracts.Loyalty;
using PaymentService.Presentation.Grpc.Clients;
using PaymentService.Presentation.Grpc.Options;
using Users.UserService.Contracts;

namespace PaymentService.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcUserServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UserServiceOptions>()
            .Bind(configuration.GetSection("UserService"));

        services.AddGrpcClient<UserLoyaltyService.UserLoyaltyServiceClient>((sp, options) =>
        {
            UserServiceOptions cfg = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;
            options.Address = new Uri(cfg.Url);
        });

        services.AddScoped<IUserLoyaltyClient, UserLoyaltyClient>();

        return services;
    }
}