namespace PaymentGateway.Core.Entities;

using Enums;

public class Payment(Guid id, string merchantId, string cardNumber, int expiryMonth, int expiryYear, Currency currency, int amount, string cvv, PaymentStatus status, string? authorizationCode)
{
    public Guid Id { get; } = id;
    public string MerchantId { get; } = merchantId;
    public string CardNumber { get; } = cardNumber;
    public int ExpiryYear { get; } = expiryYear;
    public int ExpiryMonth { get; } = expiryMonth;
    public Currency Currency { get; } = currency;
    public int Amount { get; } = amount;
    public string Cvv { get; } = cvv;
    public PaymentStatus Status { get; } = status;
    public string? AuthorizationCode { get; } = authorizationCode;
    public string CardNumberLastFour => CardNumber[^4..CardNumber.Length];
    public string MaskedCardNumber => CardNumber[^4..CardNumber.Length].PadLeft(CardNumber.Length, 'X');

    public DateTime DateTimeCreated { get; } = DateTime.UtcNow;
}
