using Itmo.Dev.Platform.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Extensions;
using PaymentService.Infrastructure.DataAccess.Extensions;
using PaymentService.Presentation.Grpc.Interceptors;
using PaymentService.Presentation.Grpc.Mapper;
using PaymentService.Presentation.Grpc.Services;
using PaymentService.Presentation.Kafka.Extensions;

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
    .AddSingleton<ModelMapper>()
    .AddPresentationKafka(builder.Configuration);

builder.Services.AddPlatformEvents(events => events
    .AddPresentationKafkaEventHandlers());

WebApplication app = builder.Build();

app.MapGrpcService<PaymentGrpcService>();
app.MapGrpcService<WalletGrpcService>();
app.MapGrpcService<WalletTransactionGrpcService>();

app.Run();