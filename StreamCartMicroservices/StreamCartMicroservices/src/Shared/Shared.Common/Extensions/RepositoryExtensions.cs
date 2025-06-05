using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Data.Interfaces;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;

namespace Shared.Common.Extensions
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Registers generic repositories for all entity types that inherit from BaseEntity
        /// </summary>
        /// <typeparam name="TContext">The DbContext type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddGenericRepositories<TContext>(this IServiceCollection services) 
            where TContext : DbContext
        {
            // Register the open generic repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(EfCoreGenericRepository<>));
            
            return services;
        }
    }
}