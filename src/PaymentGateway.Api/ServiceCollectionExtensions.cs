using Microsoft.OpenApi.Models;

namespace PaymentGateway.Api;

public static class ServiceCollectionExtensions
{
    public static void ConfigureSwaggerSecurity(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment Gateway API", Version = "v1" });
            options.AddSecurityDefinition("Default", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Default",
                In = ParameterLocation.Header,
                Description = "The value provided will be used as the merchant identifier",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Default"
                    }
                },
                new string[] {}
             }});
        });
    }
}
