using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsTest.Domain.Entities;

namespace SmsTest.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new DishConfiguration());
			modelBuilder.ApplyConfiguration(new OrderConfiguration());
			base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }

	public class DishConfiguration : IEntityTypeConfiguration<Dish>
	{
		public void Configure(EntityTypeBuilder<Dish> builder)
		{
			builder.ToTable("Dishes");

			builder.HasKey(d => d.Id);

			builder.Property(d => d.Id)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(d => d.Article)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(d => d.Name)
				.HasMaxLength(255)
				.IsRequired();

			builder.Property(d => d.Price)
				.HasPrecision(18, 2)
				.IsRequired();

			builder.Property(d => d.FullPath)
				.HasMaxLength(500);

			builder.Property(d => d.IsWeighted)
				.IsRequired();

			builder.Property(d => d.Barcodes)
				.HasColumnType("jsonb")
				.HasConversion(
					v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
					v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

			builder.HasIndex(d => d.Article)
				.IsUnique();

			builder.HasIndex(d => d.Name);
		}
	}

	public class OrderConfiguration : IEntityTypeConfiguration<Order>
	{
		public void Configure(EntityTypeBuilder<Order> builder)
		{
			builder.ToTable("Orders");

			builder.HasKey(o => o.Id);

			builder.Property(o => o.CreatedDate)
				.IsRequired();

			builder.Property(o => o.Status)
				.IsRequired()
				.HasConversion<string>();

			builder.OwnsMany(o => o.Items, itemBuilder =>
			{
				itemBuilder.ToTable("OrderItems");

				itemBuilder.WithOwner().HasForeignKey("OrderId");
				itemBuilder.Property<Guid>("Id").ValueGeneratedOnAdd();
				itemBuilder.HasKey("Id");

				itemBuilder.Property(oi => oi.DishId)
					.HasMaxLength(50)
					.IsRequired();

				itemBuilder.Property(oi => oi.DishName)
					.HasMaxLength(255)
					.IsRequired();

				itemBuilder.Property(oi => oi.Quantity)
					.HasPrecision(18, 3)
					.IsRequired();

				itemBuilder.Property(oi => oi.UnitPrice)
					.HasPrecision(18, 2)
					.IsRequired();
			});

			builder.Ignore(o => o.DomainEvents);

			builder.HasIndex(o => o.CreatedDate);
			builder.HasIndex(o => o.Status);
		}
	}
}
