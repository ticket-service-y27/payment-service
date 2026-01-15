using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Payments.Kafka.Contracts;
using PaymentService.Application.Contracts.Events;

namespace PaymentService.Presentation.Kafka.ProducerHandlers;

public class PaymentFailedHandler : IEventHandler<PaymentFailedEvent>
{
    private readonly IKafkaMessageProducer<PaymentStatusKey, PaymentStatusValue> _messageProducer;

    public PaymentFailedHandler(IKafkaMessageProducer<PaymentStatusKey, PaymentStatusValue> messageProducer)
    {
        _messageProducer = messageProducer;
    }

    public async ValueTask HandleAsync(PaymentFailedEvent evt, CancellationToken cancellationToken)
    {
        var key = new PaymentStatusKey
        {
            PaymentId = evt.PaymentId,
        };

        var value = new PaymentStatusValue
        {
            PaymentFailed = new PaymentStatusValue.Types.PaymentFailed
            {
                WalletId = evt.WalletId,
                Amount = evt.Amount,
                UserId = evt.UserId,
            },
        };

        var message = new KafkaProducerMessage<PaymentStatusKey, PaymentStatusValue>(key, value);
        await _messageProducer.ProduceAsync(message, cancellationToken);
    }
}