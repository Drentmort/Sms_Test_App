using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Domain.Entities;
using static Sms.Test.Grpc.SmsTestService;

namespace SmsTest.Infrastructure.ExternalServices
{
    public class GrpcExternalService : IExternalService
    {
        private readonly GrpcChannel _channel;
        private readonly SmsTestServiceClient _client;
        private readonly ILogger<GrpcExternalService> _logger;

        public GrpcExternalService(
            IOptions<ExternalServiceSettings> settings,
            ILogger<GrpcExternalService> logger)
        {
            _logger = logger;
            var grpcUrl = settings.Value.BaseUrl;
            
            _channel = GrpcChannel.ForAddress(grpcUrl);
            _client = new SmsTestServiceClient(_channel);
        }

        public async Task<List<Dish>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
        {
            try
            {
                var request = new Google.Protobuf.WellKnownTypes.BoolValue { Value = withPrice };
                var response = await _client.GetMenuAsync(request, cancellationToken: cancellationToken);
                
                if (!response.Success)
                {
                    throw new Exception($"Server error: {response.ErrorMessage}");
                }

                return response.MenuItems.Select(mi => new Dish(
                    mi.Id,
                    mi.Article,
                    mi.Name,
                    (decimal)mi.Price,
                    mi.IsWeighted,
                    mi.FullPath
                )).ToList();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error getting menu");
                throw new Exception($"gRPC error: {ex.Status.Detail}", ex);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SendOrderAsync(Order order, CancellationToken cancellationToken)
        {
            try
            {
                var request = new Sms.Test.Grpc.Order
				{
                    Id = order.Id.ToString()
                };
                
                request.OrderItems.AddRange(order.Items.Select(item => new Sms.Test.Grpc.OrderItem
                {
                    Id = item.DishId,
                    Quantity = (double)item.Quantity
                }));

                var response = await _client.SendOrderAsync(request, cancellationToken: cancellationToken);
                return (response.Success, response.ErrorMessage);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error sending order");
                return (false, $"gRPC error: {ex.Status.Detail}");
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
