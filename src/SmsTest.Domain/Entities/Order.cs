using SmsTest.Domain.Events;
using SmsTest.Domain.ValueObjects;

namespace SmsTest.Domain.Entities
{
    public class Order : AggregateRoot<Guid>
    {
        private readonly List<OrderItem> _items = new();

        public Order(Guid id, DateTime createdDate)
        {
            Id = id;
            CreatedDate = createdDate;
            Status = OrderStatus.Pending;
        }

        public DateTime CreatedDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public decimal TotalAmount => _items.Sum(item => item.Quantity * item.UnitPrice);

        public void AddItem(string dishId, decimal quantity, decimal unitPrice, string dishName)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            
            var existingItem = _items.FirstOrDefault(item => item.DishId == dishId);
            
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                _items.Add(new OrderItem(dishId, dishName, quantity, unitPrice));
            }
        }

        public void RemoveItem(string dishId)
        {
            var item = _items.FirstOrDefault(i => i.DishId == dishId);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public void MarkAsSent()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot mark order as sent. Current status: {Status}");
            
            Status = OrderStatus.Sent;
            AddDomainEvent(new OrderSentEvent(Id, DateTime.UtcNow));
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = OrderStatus.Failed;
            AddDomainEvent(new OrderFailedEvent(Id, DateTime.UtcNow, errorMessage));
        }
    }

    public enum OrderStatus
    {
        Pending,
        Sent,
        Failed
    }
}
