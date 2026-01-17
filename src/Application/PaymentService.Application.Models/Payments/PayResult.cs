namespace PaymentService.Application.Models.Payments;

public record PayResult(bool Success, PaymentFailReason? FailReason);