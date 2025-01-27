namespace PaymentGateway.Api.V1.Models.Responses;

using Core;

using PaymentGateway.Core.Entities;

public class GetPaymentResponse
{
    public GetPaymentResponse()
    {
    }
    public GetPaymentResponse(Payment payment)
    {
        Id = payment.Id;
        Status = payment.Status.ToString();
        MaskedCardNumber = payment.MaskedCardNumber;
        ExpiryMonth = payment.ExpiryMonth;
        ExpiryYear = payment.ExpiryYear;
        Currency = payment.Currency.ToString();
        Amount = payment.Amount;
    }

    public Guid Id { get; set; }
    public string Status { get; set; }
    public string MaskedCardNumber { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}