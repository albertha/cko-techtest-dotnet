namespace PaymentGateway.Core.Interfaces;

using Payments;

public interface IIdempotencyRepository
{
    public bool Add(string key, CreatePaymentResult item);

    public bool Update(string key, CreatePaymentResult item);

    public bool Remove(string key);

    public bool Get(string key, out CreatePaymentResult value);
}