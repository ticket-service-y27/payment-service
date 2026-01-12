using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Contracts.Payments;
using PaymentService.Application.Contracts.Wallets;
using PaymentService.Application.Contracts.WalletTransactions;
using PaymentService.Application.Payments;
using PaymentService.Application.Wallets;
using PaymentService.Application.WalletTransactions;

namespace PaymentService.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IPaymentService, PaymentsService>();
        services.AddScoped<IWalletTransactionService, WalletTransactionService>();

        return services;
    }
}