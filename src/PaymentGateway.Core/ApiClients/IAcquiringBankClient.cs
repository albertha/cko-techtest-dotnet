using Refit;

namespace PaymentGateway.Core.ApiClients;

public interface IAcquiringBankClient
{
    [Post("/payments")]
    Task<ApiResponse<AcquiringBankPaymentResponse>> PostAsync(AcquiringBankPaymentRequest request);
}
