using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Domain.Entities;
using SmsTest.Infrastructure.Persistence;

namespace SmsTest.Infrastructure.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly ApplicationDbContext _context;

		public OrderRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
		}

		public async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken)
		{
			return await _context.Orders
				.Include(o => o.Items) 
				.OrderByDescending(o => o.CreatedDate)
				.ToListAsync(cancellationToken);
		}

		public async Task AddAsync(Order order, CancellationToken cancellationToken)
		{
			await _context.Orders.AddAsync(order, cancellationToken);
		}

		public Task UpdateAsync(Order order, CancellationToken cancellationToken)
		{
			_context.Orders.Update(order);
			return Task.CompletedTask;
		}

		public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
		{
			var order = await GetByIdAsync(id, cancellationToken);
			if (order != null)
			{
				_context.Orders.Remove(order);
			}
		}
	}
}