using FluentValidation;

namespace PaymentGateway.Api.V1.Validators;

using Models.Requests;

using PaymentGateway.Core.Enums;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private const string NumbersOnlyRegex = "^[0-9]*$";
    private const int MaxAmount = int.MaxValue; // max transaction amount can be configured per merchant
    
    private readonly TimeProvider _timeProvider;

    public PostPaymentRequestValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .MinimumLength(14)
            .MaximumLength(19)
            .Matches(NumbersOnlyRegex);

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12);
        
        RuleFor(x => x.ExpiryYear)
            .Must(BeValidExpiry);

        RuleFor(x => x.Currency)
            .Length(3)
            .IsEnumName(typeof(Currency), caseSensitive: false);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .LessThan(MaxAmount);

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(4)
            .Matches(NumbersOnlyRegex);
    }

    private bool BeValidExpiry(PostPaymentRequest req, int expiryYear)
    { 
        var utcNow = _timeProvider.GetUtcNow();

        if (utcNow.Year > expiryYear)
            return false;

        if (utcNow.Year == expiryYear && utcNow.Month > req.ExpiryMonth)
            return false;

        return true;
    }
}
