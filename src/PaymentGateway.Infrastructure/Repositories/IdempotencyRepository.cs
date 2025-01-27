using System.Collections.Concurrent;

namespace PaymentGateway.Infrastructure.Repositories;

using Core.Interfaces;
using Core.Payments;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly ConcurrentDictionary<string, CreatePaymentResult> _store = new ConcurrentDictionary<string, CreatePaymentResult>();

    public bool Add(string key, CreatePaymentResult value) => _store.TryAdd(key, value);

    public bool Update(string key, CreatePaymentResult value) => _store.TryUpdate(key, value, default);

    public bool Remove(string key) => _store.Remove(key, out _);

    public bool Get(string key, out CreatePaymentResult value) => _store.TryGetValue(key, out value);
}