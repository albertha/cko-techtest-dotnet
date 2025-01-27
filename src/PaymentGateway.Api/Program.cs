using FluentValidation;
using MediatR;
using Refit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

using PaymentGateway.Api.Authentication;
using PaymentGateway.Api.ExceptionHandlers;
using PaymentGateway.Api.V1.Models.Requests;
using PaymentGateway.Api.V1.Validators;
using PaymentGateway.Core.ApiClients;
using PaymentGateway.Core.Payments;
using PaymentGateway.Api;
using PaymentGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddControllers();
        
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddSingleton(TimeProvider.System);
services.AddScoped<IValidator<PostPaymentRequest>, PostPaymentRequestValidator>();
services
    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePaymentCommand).Assembly))
    .AddTransient(typeof(IPipelineBehavior<CreatePaymentCommand, CreatePaymentResult>), typeof(IdempotencyBehavior));
        
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

services.AddRefitClient<IAcquiringBankClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["AcquiringBankUrl"]));

services.AddCoreServices();
        
// Register exception handler
services.AddExceptionHandler<DuplicateRequestExceptionHandler>();
services.AddExceptionHandler<ArgumentExceptionHandler>();
services.AddProblemDetails();

// Add dummy authentication
services.AddAuthentication("DefaultAuthentication").AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("DefaultAuthentication", null);

services.ConfigureSwaggerSecurity();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();