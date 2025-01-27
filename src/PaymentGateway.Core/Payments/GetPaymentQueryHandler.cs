using MediatR;

namespace PaymentGateway.Core.Payments;

using Core.Entities;
using Core.Interfaces;

public class GetPaymentQueryHandler(IPaymentsRepository paymentsRepository)
    : IRequestHandler<GetPaymentQuery, Payment>
{
    public Task<Payment?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = paymentsRepository.Get(request.PaymentId);
        
        if (payment != null && payment.MerchantId == request.MerchantId)
            return Task.FromResult((Payment?)payment);

        return Task.FromResult((Payment?)null);
    }
}