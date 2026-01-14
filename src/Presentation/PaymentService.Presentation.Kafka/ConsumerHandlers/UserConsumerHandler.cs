using Itmo.Dev.Platform.Kafka.Consumer;
using PaymentService.Application.Contracts.Wallets;
using Users.Kafka.Contracts;

namespace PaymentService.Presentation.Kafka.ConsumerHandlers;

public class UserConsumerHandler : IKafkaConsumerHandler<UserEventKey, UserEventValue>
{
    private const bool BlockedStatus = true;
    private const bool UnBlockedStatus = false;

    private readonly IWalletService _walletService;

    public UserConsumerHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<UserEventKey, UserEventValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (IKafkaConsumerMessage<UserEventKey, UserEventValue> message in messages)
        {
            if (message.Value.EventCase is UserEventValue.EventOneofCase.UserCreated)
            {
                await _walletService.CreateWalletAsync(message.Value.UserCreated.UserId, cancellationToken);
            }
            else if (message.Value.EventCase is UserEventValue.EventOneofCase.UserBlocked)
            {
                await _walletService.SetBlockStatusAsync(message.Value.UserBlocked.UserId, BlockedStatus, cancellationToken);
            }
            else if (message.Value.EventCase is UserEventValue.EventOneofCase.UserUnblocked)
            {
                await _walletService.SetBlockStatusAsync(message.Value.UserUnblocked.UserId, UnBlockedStatus, cancellationToken);
            }
        }
    }
}