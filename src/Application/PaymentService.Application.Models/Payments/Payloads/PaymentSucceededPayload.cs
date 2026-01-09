namespace PaymentService.Application.Models.Payments.Payloads;

public record PaymentSucceededPayload(long PaymentId, string Message) : PaymentPayload(Message);