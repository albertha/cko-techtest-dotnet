using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;

namespace PaymentGateway.Api.V1.Controllers;

using Api.V1.Models.Requests;
using Api.V1.Models.Responses;
using Core.Enums;
using Core.Payments;

[Authorize]
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PaymentsController(
    IValidator<PostPaymentRequest> validator,
    IMediator mediator) : Controller
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<BadRequest, NotFound, Ok<GetPaymentResponse>>> GetPayment(Guid id)
    {
        var query = new GetPaymentQuery(MerchantId, id);

        // Route the query for handling via mediator
        var payment = await mediator.Send(query);

        return payment == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new GetPaymentResponse(payment));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<Results<BadRequest<string>, Created<PostPaymentResponse>, UnprocessableEntity<string>>> PostPayment(
        [FromBody] PostPaymentRequest request,
        [FromHeader(Name = "x-idempotency-key")] string? idempotencyKey)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return TypedResults.BadRequest(PaymentStatus.Rejected.ToString());
        }

        var command = new CreatePaymentCommand(
            MerchantId,
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            Enum.Parse<Currency>(request.Currency, ignoreCase: true),
            request.Amount,
            request.Cvv,
            idempotencyKey);

        // Route the command for handling via mediator
        var result = await mediator.Send(command, HttpContext.RequestAborted);

        var response = new PostPaymentResponse(command, result);

        return result.Success
            ? TypedResults.Created<PostPaymentResponse>($"/api/v1/payments/{result.Id}", response)
            : TypedResults.UnprocessableEntity("Payment could not be processed."); // TODO: use HTTP 402 Payment Required instead ?
    }

    private string MerchantId => HttpContext.User?.Identity?.Name!; // The signed-in merchant user
}