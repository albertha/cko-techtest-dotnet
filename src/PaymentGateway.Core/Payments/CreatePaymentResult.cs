namespace PaymentGateway.Core.Payments;

using Enums;

public record CreatePaymentResult(Guid Id, bool Success, PaymentStatus? Status = null);
