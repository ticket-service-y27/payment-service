using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Extensions;
using PaymentService.Infrastructure.DataAccess.Extensions;
using PaymentService.Presentation.Grpc.Interceptors;
using PaymentService.Presentation.Grpc.Mapper;
using PaymentService.Presentation.Grpc.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ErrorHandling>();
});

builder.Services
    .AddDatabaseOptions(builder.Configuration)
    .AddMigrations()
    .AddNpgsqlDataSource()
    .AddMigrationHostedService()
    .AddRepositories()
    .AddApplication()
    .AddSingleton<ModelMapper>();

WebApplication app = builder.Build();

app.MapGrpcService<PaymentGrpcService>();
app.MapGrpcService<WalletGrpcService>();
app.MapGrpcService<WalletTransactionGrpcService>();

app.MapGet("/", () => "Hello World!");

app.Run();