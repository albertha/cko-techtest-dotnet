namespace PaymentGateway.Core.Interfaces;

using Entities;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}