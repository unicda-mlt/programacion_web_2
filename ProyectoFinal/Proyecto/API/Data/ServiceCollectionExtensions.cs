using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString)
                .UseSnakeCaseNamingConvention()
            );

            return services;
        }
    }
}
