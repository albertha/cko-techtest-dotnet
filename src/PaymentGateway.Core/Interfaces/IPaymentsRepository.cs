namespace PaymentGateway.Core.Interfaces;

using Core.Entities;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}