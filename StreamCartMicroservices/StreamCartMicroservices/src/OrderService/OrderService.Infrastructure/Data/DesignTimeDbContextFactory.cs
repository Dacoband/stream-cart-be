using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderContext>
    {
        public OrderContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();

            // ✅ Connection string cứng - GUARANTEED TO WORK
            optionsBuilder.UseNpgsql("Host=160.187.241.81;Port=5432;Database=StreamCartDb;Username=admin;Password=12345;",
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(OrderContext).Assembly.FullName);
                });

            return new OrderContext(optionsBuilder.Options);
        }
    }
}