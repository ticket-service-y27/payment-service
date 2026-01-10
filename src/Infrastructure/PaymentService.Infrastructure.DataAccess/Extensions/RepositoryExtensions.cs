using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Models.Payments;
using PaymentService.Application.Models.Transactions;
using PaymentService.Infrastructure.DataAccess.Migrations;
using PaymentService.Infrastructure.DataAccess.Options;
using PaymentService.Infrastructure.DataAccess.Repositories;

namespace PaymentService.Infrastructure.DataAccess.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddDatabaseOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection("DatabaseSettings"));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

        return services;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(sp =>
                    sp.GetRequiredService<IOptionsMonitor<DatabaseSettings>>().CurrentValue.ConnectionString)
                .ScanIn(typeof(CreateWalletsTable).Assembly).For.Migrations());

        return services;
    }

    public static IServiceCollection AddNpgsqlDataSource(this IServiceCollection services)
    {
        services
            .AddSingleton(sp =>
            {
                string dbSettings = sp.GetRequiredService<IOptionsMonitor<DatabaseSettings>>().CurrentValue.ConnectionString;
                var sourceBuilder = new NpgsqlDataSourceBuilder(dbSettings);

                sourceBuilder.MapEnum<PaymentStatus>();
                sourceBuilder.MapEnum<TransactionType>();

                return sourceBuilder.Build();
            });

        return services;
    }
}