namespace PaymentGateway.Api.V1.Models.Responses;

using Core.Payments;

public class PostPaymentResponse
{
    public PostPaymentResponse()
    {
    }
    public PostPaymentResponse(CreatePaymentCommand command, CreatePaymentResult result)
    {
        Id = result.Id;
        Status = result.Status.ToString();
        CardNumberLastFour = command.CardNumber[^4.. command.CardNumber.Length];
        ExpiryMonth = command.ExpiryMonth;
        ExpiryYear = command.ExpiryYear;
        Currency = command.Currency.ToString();
        Amount = command.Amount;
    }

    public Guid Id { get; set; }
    public string? Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}