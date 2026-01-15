using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Payments.Kafka.Contracts;
using PaymentService.Application.Contracts.Events;

namespace PaymentService.Presentation.Kafka.ProducerHandlers;

public class PaymentRefundedHandler : IEventHandler<PaymentRefundedEvent>
{
    private readonly IKafkaMessageProducer<PaymentStatusKey, PaymentStatusValue> _messageProducer;

    public PaymentRefundedHandler(IKafkaMessageProducer<PaymentStatusKey, PaymentStatusValue> messageProducer)
    {
        _messageProducer = messageProducer;
    }

    public async ValueTask HandleAsync(PaymentRefundedEvent evt, CancellationToken cancellationToken)
    {
        var key = new PaymentStatusKey
        {
            PaymentId = evt.PaymentId,
        };

        var value = new PaymentStatusValue
        {
            PaymentRefunded = new PaymentStatusValue.Types.PaymentRefunded
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