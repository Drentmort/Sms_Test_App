using Microsoft.EntityFrameworkCore;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Domain.Entities;
using SmsTest.Infrastructure.Persistence;

namespace SmsTest.Infrastructure.Repositories
{
    public class DishRepository : IDishRepository
    {
        private readonly ApplicationDbContext _context;

        public DishRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dish?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.Dishes
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<List<Dish>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Dishes
                .OrderBy(d => d.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Dish dish, CancellationToken cancellationToken)
        {
            await _context.Dishes.AddAsync(dish, cancellationToken);
        }

        public async Task UpdateAsync(Dish dish, CancellationToken cancellationToken)
        {
            _context.Dishes.Update(dish);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var dish = await GetByIdAsync(id, cancellationToken);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
            }
        }
    }
}
