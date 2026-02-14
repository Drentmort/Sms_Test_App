using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsTest.Application.Common.Interfaces;

namespace SmsTest.Infrastructure.ExternalServices
{
    public interface IExternalServiceFactory
    {
        IExternalService CreateService();
    }

    public class ExternalServiceFactory : IExternalServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ExternalServiceSettings _settings;

        public ExternalServiceFactory(
            IServiceProvider serviceProvider,
            IOptions<ExternalServiceSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
        }

        public IExternalService CreateService()
        {
            return _settings.Type.ToLowerInvariant() switch
            {
                "test" => _serviceProvider.GetRequiredService<TestExternalService>(),
                "grpc" => _serviceProvider.GetRequiredService<GrpcExternalService>(),
                _ => _serviceProvider.GetRequiredService<HttpExternalService>()
            };
        }
    }
}
