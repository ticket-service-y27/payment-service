using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Payments.Kafka.Contracts;
using PaymentService.Application.Contracts.Events;

namespace PaymentService.Presentation.Kafka.ProducerHandlers;

public class PaymentSucceededHandler : IEventHandler<PaymentSucceededEvent>
{
    private readonly IKafkaMessageProducer<PaymentStatusKey, PaymentStatusValue> _messageProducer;

    public PaymentSucceededHandler(IKafkaMessageProducer<PaymentStatusKey, PaymentStatusValue> messageProducer)
    {
        _messageProducer = messageProducer;
    }

    public async ValueTask HandleAsync(PaymentSucceededEvent evt, CancellationToken cancellationToken)
    {
        var key = new PaymentStatusKey
        {
            PaymentId = evt.PaymentId,
        };

        var value = new PaymentStatusValue
        {
            PaymentSucceeded = new PaymentStatusValue.Types.PaymentSucceeded
            {
                PaymentId = evt.PaymentId,
                WalletId = evt.WalletId,
                Amount = evt.Amount,
            },
        };

        var message = new KafkaProducerMessage<PaymentStatusKey, PaymentStatusValue>(key, value);
        await _messageProducer.ProduceAsync(message, cancellationToken);
    }
}