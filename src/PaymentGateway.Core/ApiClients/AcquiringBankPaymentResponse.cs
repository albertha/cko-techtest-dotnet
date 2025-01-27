using System.Text.Json.Serialization;

namespace PaymentGateway.Core.ApiClients;

public class AcquiringBankPaymentResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; set; }

    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; set; }
}
