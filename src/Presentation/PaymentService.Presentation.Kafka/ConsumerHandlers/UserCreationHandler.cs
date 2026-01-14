using Itmo.Dev.Platform.Kafka.Consumer;
using PaymentService.Application.Contracts.Wallets;
using Users.Kafka.Contracts;

namespace PaymentService.Presentation.Kafka.ConsumerHandlers;

public class UserCreationHandler : IKafkaConsumerHandler<UserCreationKey, UserCreationValue>
{
    private readonly IWalletService _walletService;

    public UserCreationHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<UserCreationKey, UserCreationValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (IKafkaConsumerMessage<UserCreationKey, UserCreationValue> message in messages)
        {
            await _walletService.CreateWalletAsync(message.Value.UserCreated.UserId, cancellationToken);
        }
    }
}