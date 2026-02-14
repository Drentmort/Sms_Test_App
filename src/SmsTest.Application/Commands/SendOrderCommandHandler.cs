using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Application.Common.Models;
using SmsTest.Domain.Entities;

namespace SmsTest.Application.Commands
{
	public class SendOrderCommandHandler : IRequestHandler<SendOrderCommand, Result<Guid>>
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IExternalService _externalService;
		private readonly ILogger<SendOrderCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public SendOrderCommandHandler(
			IOrderRepository orderRepository,
			IExternalService externalService,
			ILogger<SendOrderCommandHandler> logger,
			IUnitOfWork unitOfWork)
		{
			_orderRepository = orderRepository;
			_externalService = externalService;
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<Result<Guid>> Handle(SendOrderCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var orderId = Guid.NewGuid();
				var order = new Order(orderId, DateTime.UtcNow);

				foreach (var item in request.Items)
				{
					order.AddItem(item.DishId, item.Quantity, item.UnitPrice, item.DishName);
				}

				await _orderRepository.AddAsync(order, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				_logger.LogInformation("Order {OrderId} saved to database", orderId);

				var (success, errorMessage) = await _externalService.SendOrderAsync(order, cancellationToken);

				if (success)
				{
					order.MarkAsSent();
					_logger.LogInformation("Order {OrderId} sent successfully", orderId);
				}
				else
				{
					order.MarkAsFailed(errorMessage);
					_logger.LogError("Failed to send order {OrderId}: {ErrorMessage}", orderId, errorMessage);
				}

				await _orderRepository.UpdateAsync(order, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				return success
					? Result<Guid>.GetSuccess(orderId)
					: Result<Guid>.GetFailure(errorMessage);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing SendOrderCommand");
				return Result<Guid>.GetFailure($"Internal error: {ex.Message}");
			}
		}
	}
}