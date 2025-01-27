using MediatR;

namespace PaymentGateway.Core.Payments;

using Core.Entities;

public class GetPaymentQuery : IRequest<Payment>
{
    public string MerchantId { get; }
    public Guid PaymentId { get; }

    public GetPaymentQuery(string merchantId, Guid paymentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(merchantId, nameof(merchantId));

        if (paymentId == Guid.Empty)
            throw new ArgumentException($"{nameof(paymentId)} can not be empty.");

        MerchantId = merchantId;
        PaymentId = paymentId;

    }
}
