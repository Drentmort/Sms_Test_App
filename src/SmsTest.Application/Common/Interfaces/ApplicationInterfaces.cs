using SmsTest.Application.Common.Models;
using SmsTest.Domain.Entities;

namespace SmsTest.Application.Common.Interfaces
{
    public interface IExternalService
    {
        Task<List<Dish>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken);
        Task<(bool Success, string ErrorMessage)> SendOrderAsync(Order order, CancellationToken cancellationToken);
    }

    public interface IDishRepository
    {
        Task<Dish?> GetByIdAsync(string id, CancellationToken cancellationToken);
        Task<List<Dish>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(Dish dish, CancellationToken cancellationToken);
        Task UpdateAsync(Dish dish, CancellationToken cancellationToken);
        Task DeleteAsync(string id, CancellationToken cancellationToken);
    }

    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Order>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(Order order, CancellationToken cancellationToken);
        Task UpdateAsync(Order order, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
