using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Kafka.Contracts;
using PaymentService.Presentation.Kafka.ConsumerHandlers;
using Users.Kafka.Contracts;

namespace PaymentService.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationKafka(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        const string consumerKey = "Presentation:Kafka:Consumers";
        const string producerKey = "Presentation:Kafka:Producers";

        collection.AddPlatformKafka(kafka => kafka
            .ConfigureOptions(configuration.GetSection("Presentation:Kafka"))
            .AddConsumer(consumer => consumer
                .WithKey<UserEventKey>()
                .WithValue<UserEventValue>()
                .WithConfiguration(configuration.GetSection($"{consumerKey}:UserCreation"))
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .HandleWith<UserConsumerHandler>())
            .AddProducer(producer => producer
                .WithKey<PaymentStatusKey>()
                .WithValue<PaymentStatusValue>()
                .WithConfiguration(configuration.GetSection($"{producerKey}:PaymentStatus"))
                .SerializeKeyWithProto()
                .SerializeValueWithProto()));

        return collection;
    }

    public static IEventsConfigurationBuilder AddPresentationKafkaEventHandlers(
        this IEventsConfigurationBuilder builder)
    {
        return builder.AddHandlersFromAssemblyContaining<IKafkaAssemblyMarker>();
    }
}