using MediatR;
using Microsoft.Extensions.Logging;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Application.Common.Models;

namespace SmsTest.Application.Queries
{
    public class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, Result<List<DishDto>>>
    {
        private readonly IExternalService _externalService;
        private readonly ILogger<GetMenuQueryHandler> _logger;

        public GetMenuQueryHandler(
            IExternalService externalService,
            ILogger<GetMenuQueryHandler> logger)
        {
            _externalService = externalService;
            _logger = logger;
        }

        public async Task<Result<List<DishDto>>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var dishes = await _externalService.GetMenuAsync(request.WithPrice, cancellationToken);
                
                _logger.LogInformation("Retrieved {Count} dishes from external service", dishes.Count);
                
                var dishDtos = dishes.Select(d => new DishDto
                {
                    Id = d.Id,
                    Article = d.Article,
                    Name = d.Name,
                    Price = d.Price,
                    IsWeighted = d.IsWeighted,
                    FullPath = d.FullPath,
                    Barcodes = d.Barcodes.ToList()
                }).ToList();

                return Result<List<DishDto>>.GetSuccess(dishDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu from external service");
                return Result<List<DishDto>>.GetFailure($"Error retrieving menu: {ex.Message}");
            }
        }
    }
}
