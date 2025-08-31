
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using Npgsql.NameTranslation; 
using OrderService.Domain.Enums; 

namespace OrderService.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderContext>
    {
        public OrderContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            var connectionString = "Host=160.187.241.81;Port=5432;Database=StreamCartDb;Username=admin;Password=12345;";
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.MapEnum<OrderStatus>("order_status", nameTranslator: new NpgsqlNullNameTranslator());
            dataSourceBuilder.MapEnum<PaymentStatus>("payment_status", nameTranslator: new NpgsqlNullNameTranslator());
            var dataSource = dataSourceBuilder.Build();
            optionsBuilder.UseNpgsql(dataSource,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(OrderContext).Assembly.FullName);
                });

            return new OrderContext(optionsBuilder.Options);
        }
    }
}