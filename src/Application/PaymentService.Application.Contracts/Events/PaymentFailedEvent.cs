using Itmo.Dev.Platform.Events;

namespace PaymentService.Application.Contracts.Events;

public record PaymentFailedEvent(long PaymentId, long WalletId, long Amount, long UserId) : IEvent;