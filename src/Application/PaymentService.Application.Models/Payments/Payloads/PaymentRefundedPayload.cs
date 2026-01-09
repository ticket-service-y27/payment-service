namespace PaymentService.Application.Models.Payments.Payloads;

public record PaymentRefundedPayload(long PaymentId, string Message) : PaymentPayload(Message);