namespace SmsTest.Domain.ValueObjects
{
    public class OrderItem : ValueObject
    {
        public OrderItem(string dishId, string dishName, decimal quantity, decimal unitPrice)
        {
            DishId = dishId ?? throw new ArgumentNullException(nameof(dishId));
            DishName = dishName ?? throw new ArgumentNullException(nameof(dishName));
            Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
        }

        public string DishId { get; private set; }
        public string DishName { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        public void UpdateQuantity(decimal newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
            
            Quantity = newQuantity;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DishId;
            yield return Quantity;
        }
    }
}
