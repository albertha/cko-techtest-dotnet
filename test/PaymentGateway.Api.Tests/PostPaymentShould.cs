namespace PaymentGateway.Api.Tests;

using Api.V1.Controllers;
using Api.V1.Models.Requests;
using Api.V1.Models.Responses;
using Core.ApiClients;
using Core.Enums;
using Core.Entities;
using Core.Interfaces;

public class PostPaymentShould
{
    private readonly HttpClient _client;
    private readonly Mock<IPaymentsRepository> _paymentsRepository = new();
    private readonly Mock<IAcquiringBankClient> _acquiringBankClient = new();

    private readonly Payment _payment;
    private readonly FakeTimeProvider _fakeTimeProvider = new();
    private const string MerchantId = "TestMerchantId";

    public PostPaymentShould()
    {
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(provider => _paymentsRepository.Object);
                services.AddSingleton(_fakeTimeProvider as TimeProvider);
                services.AddTransient(provider => _acquiringBankClient.Object);
            });
        });

        _client = webApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(MerchantId);
    }

    [Theory]
    [MemberData(nameof(InvalidCardNumbers))]
    public async Task Return400_WhenCardNumberInvalid(string cardNumber)
    {
        var request = ValidRequest;
        request.CardNumber = cardNumber;

        var response = await PostAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static IEnumerable<object[]> InvalidCardNumbers => new List<object[]>
    {
        new[] { "-1" },
        new[] { "1" },
        new[] { "1111111111111" }, // 13 digits
        new[] { "11111111111111111111" }, // 20 digits
        new[] { "Hello World 1111" },
    };

    [Theory]
    [MemberData(nameof(InvalidCVVs))]
    public async Task Return400_WhenCardCvvInvalid(string cvv)
    {
        var request = ValidRequest;
        request.Cvv = cvv;

        var response = await PostAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static IEnumerable<object[]> InvalidCVVs => new List<object[]>
    {
        new[] { "12" },
        new[] { "12345" },
        new[] { "ab3" }
    };

    [Theory]
    [MemberData(nameof(InvalidCurrencies))]
    public async Task Return400_WhenCardCurrencyInvalid(string currency)
    {
        var request = ValidRequest;
        request.Currency = currency;

        var response = await PostAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static IEnumerable<object[]> InvalidCurrencies => new List<object[]>
    {
        new[] { "AB" },
        new[] { "ABCD" },
        new[] { "XXX" },
        new[] { "123" }
    };

    [Theory]
    [InlineData(2024, 12)]
    [InlineData(2025, 1)]
    public async Task Return400_WhenCardExpired(int expiryYear, int expiryMonth)
    {
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero));

        var request = ValidRequest;
        request.ExpiryYear = expiryYear;
        request.ExpiryMonth = expiryMonth;

        var response = await PostAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task Return400_WhenCardExpiryMonthInvalid(int expiryMonth)
    {
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero));

        var request = ValidRequest;
        request.ExpiryMonth = expiryMonth;

        var response = await PostAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Return401_WhenMerchantNotAuthenticated()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await PostAsync(ValidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(AcquiringBankPaymentStatus.Authorized)]
    [InlineData(AcquiringBankPaymentStatus.Declined)]
    public async Task Return201_WhenPaymentSucessfullySentToAcquiringBank(AcquiringBankPaymentStatus status)
    {
        SetupAcquiringBankResponse(status);

        var response = await PostAsync(ValidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(AcquiringBankPaymentStatus.Authorized)]
    [InlineData(AcquiringBankPaymentStatus.Declined)]
    public async Task ReturnCorrectPaymentResponse_WhenPaymentSucessfullySentToAcquiringBank(AcquiringBankPaymentStatus status)
    {
        SetupAcquiringBankResponse(status);

        var response = await PostAsync(ValidRequest);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        using (new AssertionScope())
        {
            paymentResponse.Should().NotBeNull();
            paymentResponse.Id.Should().NotBeEmpty();
            paymentResponse.Status.Should().Be(status.ToString());
            paymentResponse.CardNumberLastFour.Should().Be(ValidRequest.CardNumber[(ValidRequest.CardNumber.Length - 4).. ValidRequest.CardNumber.Length]);
            paymentResponse.ExpiryMonth.Should().Be(ValidRequest.ExpiryMonth);
            paymentResponse.ExpiryYear.Should().Be(ValidRequest.ExpiryYear);
            paymentResponse.Currency.Should().Be(ValidRequest.Currency.ToString());
            paymentResponse.Amount.Should().Be(ValidRequest.Amount);
        }
    }

    [Fact]
    public async Task ReturnResourceLocation_WhenPaymentAuthorized()
    {
        SetupAcquiringBankResponse(AcquiringBankPaymentStatus.Authorized);

        var response = await PostAsync(ValidRequest);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        response.Should().NotBeNull();
        paymentResponse.Should().NotBeNull();
        response.Headers.Location?.OriginalString.Should().Be($"/api/v1/payments/{paymentResponse.Id}");
    }

    [Fact]
    public async Task ReturnOriginalResponse_WhenRequestDuplicated_And_IdempotencyKeyIsSet()
    {
        SetupAcquiringBankResponse(AcquiringBankPaymentStatus.Authorized);

        _client.DefaultRequestHeaders.Add("x-idempotency-key", "idempotencyKey");

        var response1 = await PostAsync(ValidRequest);
        var response2 = await PostAsync(ValidRequest);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var paymentResponse1 = await response1.Content.ReadFromJsonAsync<PostPaymentResponse>();
        var paymentResponse2 = await response2.Content.ReadFromJsonAsync<PostPaymentResponse>();

        _paymentsRepository.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
        paymentResponse2!.Id.Should().Be(paymentResponse1!.Id);
    }
    
    [Fact]
    public async Task ReturnDifferentPaymentIds_WhenRequestDuplicated_And_IdempotencyKeyIsNotSet()
    {
        SetupAcquiringBankResponse(AcquiringBankPaymentStatus.Authorized);

        _client.DefaultRequestHeaders.Add("x-idempotency-key", "");

        var response1 = await PostAsync(ValidRequest);
        var response2 = await PostAsync(ValidRequest);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var paymentResponse1 = await response1.Content.ReadFromJsonAsync<PostPaymentResponse>();
        var paymentResponse2 = await response2.Content.ReadFromJsonAsync<PostPaymentResponse>();

        _paymentsRepository.Verify(x => x.Add(It.IsAny<Payment>()), Times.Exactly(2));
        paymentResponse2!.Id.Should().NotBe(paymentResponse1!.Id);
    }

    [Fact]
    public async Task ReturnDifferentPaymentIds_WhenSameIdempotencyKeyIsUsed_ForDifferentMerchants()
    {
        SetupAcquiringBankResponse(AcquiringBankPaymentStatus.Authorized);

        _client.DefaultRequestHeaders.Add("x-idempotency-key", "idempotencyKey");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Merchant_1");
        var response1 = await PostAsync(ValidRequest);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Merchant_2");
        var response2 = await PostAsync(ValidRequest);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var paymentResponse1 = await response1.Content.ReadFromJsonAsync<PostPaymentResponse>();
        var paymentResponse2 = await response2.Content.ReadFromJsonAsync<PostPaymentResponse>();

        _paymentsRepository.Verify(x => x.Add(It.IsAny<Payment>()), Times.Exactly(2));
        paymentResponse2!.Id.Should().NotBe(paymentResponse1!.Id);
    }

    [Fact]
    public async Task Return422_WhenPaymentCouldNotBeProcessedByAcquiringBank()
    {
        var apiResponse = new ApiResponse<AcquiringBankPaymentResponse>(
            new HttpResponseMessage(HttpStatusCode.BadRequest),
            null,
            new RefitSettings());

        _acquiringBankClient
            .Setup(x => x.PostAsync(It.IsAny<AcquiringBankPaymentRequest>()))
            .ReturnsAsync(apiResponse);

        var response = await PostAsync(ValidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private static PostPaymentRequest ValidRequest => new()
    {
        CardNumber = "1111111111111111",
        ExpiryMonth = 12,
        ExpiryYear = 2028,
        Amount = 100,
        Currency = Currency.GBP.ToString(),
        Cvv = "789",
    };

    private Task<HttpResponseMessage> PostAsync(PostPaymentRequest request)
    {
        return _client.PostAsync("/api/v1/payments/", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));
    }

    private void SetupAcquiringBankResponse(AcquiringBankPaymentStatus status)
    {
        AcquiringBankPaymentResponse bankResponse = new()
        {
            Authorized = status == AcquiringBankPaymentStatus.Authorized ? true : false,
            AuthorizationCode = status == AcquiringBankPaymentStatus.Authorized ? Guid.NewGuid().ToString() : null
        };
        var apiResponse = new ApiResponse<AcquiringBankPaymentResponse>(
            new HttpResponseMessage(HttpStatusCode.Created),
            bankResponse,
            new RefitSettings());

        _acquiringBankClient
            .Setup(x => x.PostAsync(It.IsAny<AcquiringBankPaymentRequest>()))
            .ReturnsAsync(apiResponse);
    }
}