using FluentValidation;
using MediatR;
using SmsTest.Application.Common.Models;

namespace SmsTest.Application.Commands
{
    public class SendOrderCommand : IRequest<Result<Guid>>
    {
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string DishId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string DishName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class SendOrderCommandValidator : AbstractValidator<SendOrderCommand>
    {
        public SendOrderCommandValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item");
            
            RuleForEach(x => x.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.DishId)
                        .NotEmpty().WithMessage("Dish ID is required");
                    
                    item.RuleFor(i => i.Quantity)
                        .GreaterThan(0).WithMessage("Quantity must be greater than zero");
                    
                    item.RuleFor(i => i.UnitPrice)
                        .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
                });
        }
    }
}
