using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Repositories;

namespace TransactionService.Infrastructure.Extensions
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<ITransactionRepository, TransactionRepository>();

            return services;
        }
    }
}
