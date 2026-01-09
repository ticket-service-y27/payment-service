namespace PaymentService.Application.Models.Payments.Payloads;

public record PaymentPendingPayload(long PaymentId, string Message) : PaymentPayload(Message);