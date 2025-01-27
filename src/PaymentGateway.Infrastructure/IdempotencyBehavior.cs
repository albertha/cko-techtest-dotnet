using MediatR;

namespace PaymentGateway.Infrastructure;

using Core.Interfaces;
using Core.Exceptions;
using Core.Payments;

public class IdempotencyBehavior(IIdempotencyRepository repository) : IPipelineBehavior<CreatePaymentCommand, CreatePaymentResult>
{
    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand request, RequestHandlerDelegate<CreatePaymentResult> next, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return await next(); // Proceed to the command handler without checking for duplicate request

        var idempotencyKey = $"{request.MerchantId}-{request.IdempotencyKey}";

        var idempotencyKeyAdded = repository.Add(idempotencyKey, default);
        if (!idempotencyKeyAdded)
        {
            // If the idempotency key already exist, we can try return the original payment result
            if (repository.Get(idempotencyKey, out var result) && result is not null)
                return result;

            // If there is no existing result, then there may be multiple requests in-flight
            throw new DuplicateRequestException();
        }

        try
        {
            // Proceed to the command handler
            var response = await next();

            repository.Update(idempotencyKey, response);

            return response;
        }
        catch (Exception)
        {
            // TODO: add tests for this scenario / code path
            repository.Remove(idempotencyKey);
            throw;
        }
    }
}
