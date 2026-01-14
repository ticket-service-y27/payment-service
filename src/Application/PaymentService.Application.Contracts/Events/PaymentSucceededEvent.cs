using Itmo.Dev.Platform.Events;

namespace PaymentService.Application.Contracts.Events;

public record PaymentSucceededEvent(long PaymentId, long WalletId, long Amount) : IEvent;