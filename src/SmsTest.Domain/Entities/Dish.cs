namespace SmsTest.Domain.Entities
{
    public class Dish : AggregateRoot<string>
    {
        public Dish(string id, string article, string name, decimal price, bool isWeighted, string fullPath)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Article = article ?? throw new ArgumentNullException(nameof(article));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
            IsWeighted = isWeighted;
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
            _barcodes = new List<string>();
        }

        private readonly List<string> _barcodes;
        
        public string Article { get; private set; }
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public bool IsWeighted { get; private set; }
        public string FullPath { get; private set; }
        public IReadOnlyCollection<string> Barcodes => _barcodes.AsReadOnly();

        public void AddBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Barcode cannot be null or empty", nameof(barcode));
            
            if (!_barcodes.Contains(barcode))
                _barcodes.Add(barcode);
        }

        public void RemoveBarcode(string barcode)
        {
            _barcodes.Remove(barcode);
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative", nameof(newPrice));
            
            Price = newPrice;
        }

        public void UpdateInfo(string name, string article, string fullPath)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Article = article ?? throw new ArgumentNullException(nameof(article));
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        }
    }
}
