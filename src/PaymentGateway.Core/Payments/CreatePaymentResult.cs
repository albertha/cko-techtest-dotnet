namespace PaymentGateway.Core.Payments;

using Core.Enums;

public record CreatePaymentResult(Guid Id, bool Success, PaymentStatus? Status = null);
