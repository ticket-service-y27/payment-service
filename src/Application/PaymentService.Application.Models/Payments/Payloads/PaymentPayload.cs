using System.Text.Json.Serialization;

namespace PaymentService.Application.Models.Payments.Payloads;

[JsonDerivedType(typeof(PaymentSucceededPayload), "succeeded")]
[JsonDerivedType(typeof(PaymentPendingPayload), "pending")]
[JsonDerivedType(typeof(PaymentFailedPayload), "failed")]
[JsonDerivedType(typeof(PaymentRefundedPayload), "refunded")]
public record PaymentPayload(string Message);