using System.Collections.Concurrent;

namespace PaymentGateway.Infrastructure.Repositories;

using Core.Entities;
using Core.Interfaces;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public void Add(Payment payment)
    {
        _payments.TryAdd(payment.Id, payment);
    }

    public Payment? Get(Guid id)
    {
        _payments.TryGetValue(id, out var payment);
        return payment;
    }
}