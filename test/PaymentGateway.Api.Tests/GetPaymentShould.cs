namespace PaymentGateway.Api.Tests;

using V1.Controllers;
using V1.Models.Responses;
using Core.Enums;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Repositories;

public class GetPaymentShould
{
    private readonly HttpClient _client;

    private readonly Payment _payment;
    private readonly Random _random = new();
    private const string MerchantId = "TestMerchantId";

    public GetPaymentShould()
    {
        _payment = Payment;
        PaymentsRepository paymentsRepository = new();
        paymentsRepository.Add(_payment);
        
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPaymentsRepository>(provider => paymentsRepository);
            });
        });

        _client = webApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(MerchantId);
    }

    [Fact]
    public async Task Return401_WhenMerchantNotAuthenticated()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await GetAsync(_payment.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Return400_WhenPaymentIdEmpty()
    {
        var response = await GetAsync(Guid.Empty);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Return404_WhenPaymentNotFound()
    {
        var response = await GetAsync(Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Return404_WhenPaymentDoesNotBelongToMerchant()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SomeOtherMerchantId");
        
        var response = await GetAsync(_payment.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Return200_WhenPaymentFound()
    {
        var response = await GetAsync(_payment.Id);
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ReturnCorrectFields_WhenPaymentFound()
    {
        var response = await GetAsync(_payment.Id);
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        using (new AssertionScope())
        {
            paymentResponse.Should().NotBeNull();
            paymentResponse.Id.Should().Be(_payment.Id);
            paymentResponse.Status.Should().Be(_payment.Status.ToString());
            paymentResponse.MaskedCardNumber.Should().NotBeNullOrEmpty();
            paymentResponse.ExpiryMonth.Should().Be(_payment.ExpiryMonth);
            paymentResponse.ExpiryYear.Should().Be(_payment.ExpiryYear);
            paymentResponse.Currency.Should().Be(_payment.Currency.ToString());
            paymentResponse.Amount.Should().Be(_payment.Amount);
        }
    }

    [Fact]
    public async Task ReturnMaskedCardNumber_WhenPaymentFound()
    {
        var response = await GetAsync(_payment.Id);
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        using (new AssertionScope())
        {
            paymentResponse.Should().NotBeNull();
            paymentResponse.MaskedCardNumber.Should().NotBe(_payment.CardNumber);
            paymentResponse.MaskedCardNumber.Should().Be(_payment.MaskedCardNumber);
        }
    }

    private Payment Payment => new(
                    Guid.NewGuid(),
                    MerchantId,
                    "1234567890123456",
                    _random.Next(1, 12),
                    _random.Next(2000, 2030),
                    Currency.GBP,
                    _random.Next(1, 1000),
                    "123",
                    PaymentStatus.Authorized,
                    Guid.NewGuid().ToString());

    private Task<HttpResponseMessage> GetAsync(Guid id)
    {
        return _client.GetAsync($"/api/v1/payments/{id}");
    }
}