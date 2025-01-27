using System.Text.Json.Serialization;

namespace PaymentGateway.Core.ApiClients;

public class AcquiringBankPaymentRequest
{
    [JsonPropertyName("amount")]
    public required int Amount { get; set; }

    [JsonPropertyName("card_number")]
    public required string CardNumber { get; set; }

    [JsonPropertyName("expiry_date")]
    public string ExpiryDate => $"{ExpiryMonth:00}/{ExpiryYear}";

    [JsonIgnore]
    public int ExpiryYear { get; set; }

    [JsonIgnore]
    public int ExpiryMonth { get; set; }

    [JsonPropertyName("cvv")]
    public required string Cvv { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
}
