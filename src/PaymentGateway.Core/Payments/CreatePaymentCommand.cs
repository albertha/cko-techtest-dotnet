using MediatR;

namespace PaymentGateway.Core.Payments;

using Enums;

public class CreatePaymentCommand(string merchantId, string cardNumber, int expiryMonth, int expiryYear, Currency currency, int amount, string cvv, string? idempotencyKey)
    : IRequest<CreatePaymentResult>
{
    public Guid Id { get; } = Guid.NewGuid();
    public string MerchantId { get; } = merchantId;
    public string CardNumber { get; } = cardNumber;
    public int ExpiryMonth { get; } = expiryMonth;
    public int ExpiryYear { get; } = expiryYear;
    public Currency Currency { get; } = currency;
    public int Amount { get; } = amount;
    public string Cvv { get; } = cvv;
    public string? IdempotencyKey => idempotencyKey;
}