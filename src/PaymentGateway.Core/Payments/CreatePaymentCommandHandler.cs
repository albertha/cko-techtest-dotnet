using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PaymentGateway.Core.Payments;

using Core.ApiClients;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;

public class CreatePaymentCommandHandler(
    IMediator mediator,
    IAcquiringBankClient acquiringBankClient,
    IPaymentsRepository paymentsRepository,
    ILogger<CreatePaymentCommandHandler> logger) : IRequestHandler<CreatePaymentCommand, CreatePaymentResult>
{
    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        AcquiringBankPaymentResponse result = new();

        try
        {
            AcquiringBankPaymentRequest request = new AcquiringBankPaymentRequest
            {
                Amount = command.Amount,
                CardNumber = command.CardNumber,
                Currency = command.Currency.ToString(),
                ExpiryMonth = command.ExpiryMonth,
                ExpiryYear = command.ExpiryYear,
                Cvv = command.Cvv
            };
            var apiResponse = await acquiringBankClient.PostAsync(request);
            if (apiResponse.IsSuccessStatusCode)
                result = apiResponse.Content!;

            if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
                return new CreatePaymentResult(command.Id, false);

            await apiResponse.EnsureSuccessStatusCodeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }

        var payment = new Payment(
                    command.Id,
                    command.MerchantId,
                    command.CardNumber,
                    command.ExpiryMonth,
                    command.ExpiryYear,
                    command.Currency,
                    command.Amount,
                    command.Cvv,
                    result.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                    result.AuthorizationCode);

        paymentsRepository.Add(payment);

        return new CreatePaymentResult(command.Id, true, payment.Status);
    }
}
