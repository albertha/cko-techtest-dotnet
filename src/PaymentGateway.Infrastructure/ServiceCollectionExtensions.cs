using Microsoft.Extensions.DependencyInjection;

namespace PaymentGateway.Infrastructure;

using Core.Interfaces;
using Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IPaymentsRepository, PaymentsRepository>()
            .AddSingleton<IIdempotencyRepository, IdempotencyRepository>();
    }
}
