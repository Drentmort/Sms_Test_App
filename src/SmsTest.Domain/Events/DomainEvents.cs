namespace SmsTest.Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }

    public class OrderSentEvent : IDomainEvent
    {
        public OrderSentEvent(Guid orderId, DateTime occurredOn)
        {
            OrderId = orderId;
            OccurredOn = occurredOn;
        }

        public Guid OrderId { get; }
        public DateTime OccurredOn { get; }
    }

    public class OrderFailedEvent : IDomainEvent
    {
        public OrderFailedEvent(Guid orderId, DateTime occurredOn, string errorMessage)
        {
            OrderId = orderId;
            OccurredOn = occurredOn;
            ErrorMessage = errorMessage;
        }

        public Guid OrderId { get; }
        public DateTime OccurredOn { get; }
        public string ErrorMessage { get; }
    }
}
