using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Domain.Entities;

namespace SmsTest.Infrastructure.ExternalServices
{
    public class HttpExternalService : IExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServiceSettings _settings;
        private readonly ILogger<HttpExternalService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpExternalService(
            HttpClient httpClient,
            IOptions<ExternalServiceSettings> settings,
            ILogger<HttpExternalService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                var authValue = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}"));
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Basic", authValue);
            }
            
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public async Task<List<Dish>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
        {
            try
            {
                var request = new
                {
                    Command = "GetMenu",
                    CommandParameters = new { WithPrice = withPrice }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<MenuData>>(responseContent, _jsonOptions);

                if (apiResponse == null || !apiResponse.Success)
                {
                    throw new Exception($"Server error: {apiResponse?.ErrorMessage ?? "Unknown error"}");
                }

                return apiResponse.Data?.MenuItems.Select(item => new Dish(
                    item.Id,
                    item.Article,
                    item.Name,
                    item.Price,
                    item.IsWeighted,
                    item.FullPath
                )).ToList() ?? new List<Dish>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu from HTTP service");
                throw;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SendOrderAsync(Order order, CancellationToken cancellationToken)
        {
            try
            {
                var request = new
                {
                    Command = "SendOrder",
                    CommandParameters = new
                    {
                        OrderId = order.Id.ToString(),
                        MenuItems = order.Items.Select(item => new
                        {
                            Id = item.DishId,
                            Quantity = item.Quantity.ToString("0.###")
                        }).ToList()
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);

                if (apiResponse == null)
                {
                    return (false, "Failed to deserialize response");
                }

                return (apiResponse.Success, apiResponse.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order via HTTP service");
                return (false, $"HTTP error: {ex.Message}");
            }
        }

        #region DTO Classes
        private class ApiResponse<T>
        {
            public string Command { get; set; } = string.Empty;
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
            public T? Data { get; set; }
        }

        private class MenuData
        {
            public List<MenuItemDto> MenuItems { get; set; } = new();
        }

        private class MenuItemDto
        {
            public string Id { get; set; } = string.Empty;
            public string Article { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsWeighted { get; set; }
            public string FullPath { get; set; } = string.Empty;
            public List<string> Barcodes { get; set; } = new();
        }
        #endregion
    }

    public class ExternalServiceSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public string Type { get; set; } = "Http";
    }
}
