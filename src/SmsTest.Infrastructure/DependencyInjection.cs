using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Infrastructure.ExternalServices;
using SmsTest.Infrastructure.Persistence;
using SmsTest.Infrastructure.Repositories;
using SmsTest.Infrastructure.Services;

namespace SmsTest.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString,
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IDishRepository, DishRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<ExternalServiceSettings>(
                configuration.GetSection("ExternalService"));
            
            services.AddHttpClient<HttpExternalService>();
            services.AddScoped<GrpcExternalService>();
            services.AddScoped<TestExternalService>();
            services.AddScoped<IExternalServiceFactory, ExternalServiceFactory>();
            services.AddScoped<IExternalService>(sp =>
                sp.GetRequiredService<IExternalServiceFactory>().CreateService());

            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

            return services;
        }
    }
}
