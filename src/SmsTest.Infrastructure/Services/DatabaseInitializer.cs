using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmsTest.Infrastructure.Persistence;

namespace SmsTest.Infrastructure.Services
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            ApplicationDbContext context,
            ILogger<DatabaseInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await EnsureTablesCreatedAsync();

				await _context.Database.MigrateAsync(cancellationToken);
                
                _logger.LogInformation("Database migrations applied successfully");
                
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                if (canConnect)
                {
                    _logger.LogInformation("Database connection successful");
                }
                else
                {
                    _logger.LogError("Cannot connect to database");
                }

				_logger.LogInformation("Ensuring database is created...");
                _context.Database.EnsureCreated();

				_logger.LogInformation("Database initialization completed successfully");
			}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                throw;
            }
        }

        private async Task EnsureTablesCreatedAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ""Dishes"" (
            ""Id"" varchar(50) PRIMARY KEY,
            ""Article"" varchar(50) NOT NULL,
            ""Name"" varchar(255) NOT NULL,
            ""Price"" numeric(18,2) NOT NULL,
            ""IsWeighted"" boolean NOT NULL,
            ""FullPath"" varchar(500),
            ""Barcodes"" jsonb
        );
    ");
			await _context.Database.ExecuteSqlRawAsync(@"
     
CREATE TABLE IF NOT EXISTS ""Orders"" (
    ""Id"" UUID NOT NULL,
    ""CreatedDate"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    ""Status"" TEXT NOT NULL,
    CONSTRAINT ""PK_Orders"" PRIMARY KEY (""Id"")
);

CREATE TABLE IF NOT EXISTS ""OrderItems"" (
    ""Id"" UUID NOT NULL DEFAULT gen_random_uuid(),
    ""OrderId"" UUID NOT NULL,
    ""DishId"" VARCHAR(50) NOT NULL,
    ""DishName"" VARCHAR(255) NOT NULL,
    ""Quantity"" NUMERIC(18, 3) NOT NULL,
    ""UnitPrice"" NUMERIC(18, 2) NOT NULL,
    CONSTRAINT ""PK_OrderItems"" PRIMARY KEY (""Id"")
);

CREATE INDEX IF NOT EXISTS ""IX_Orders_CreatedDate"" ON ""Orders"" (""CreatedDate"");
CREATE INDEX IF NOT EXISTS ""IX_Orders_Status"" ON ""Orders"" (""Status"");
CREATE INDEX IF NOT EXISTS ""IX_OrderItems_OrderId"" ON ""OrderItems"" (""OrderId"");
CREATE INDEX IF NOT EXISTS ""IX_OrderItems_DishId"" ON ""OrderItems"" (""DishId"");");
		}
    }
}
