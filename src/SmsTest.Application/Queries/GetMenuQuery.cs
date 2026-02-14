using MediatR;
using SmsTest.Application.Common.Models;

namespace SmsTest.Application.Queries
{
    public class GetMenuQuery : IRequest<Result<List<DishDto>>>
    {
        public bool WithPrice { get; set; } = true;
    }

    public class DishDto
    {
        public string Id { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsWeighted { get; set; }
        public string FullPath { get; set; } = string.Empty;
        public List<string> Barcodes { get; set; } = new();
    }
}
