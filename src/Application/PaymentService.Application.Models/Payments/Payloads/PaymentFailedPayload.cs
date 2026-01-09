namespace PaymentService.Application.Models.Payments.Payloads;

public record PaymentFailedPayload(long PaymentId, string Message) : PaymentPayload(Message);