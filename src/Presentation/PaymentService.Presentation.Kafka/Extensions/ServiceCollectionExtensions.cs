using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        collection.AddPlatformKafka(kafka => kafka
            .ConfigureOptions(configuration.GetSection("Presentation:Kafka"))
            .AddConsumer(consumer => consumer
                .WithKey<UserEventKey>()
                .WithValue<UserEventValue>()
                .WithConfiguration(configuration.GetSection($"{consumerKey}:UserCreation"))
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .HandleWith<UserConsumerHandler>()));

        return collection;
    }
}