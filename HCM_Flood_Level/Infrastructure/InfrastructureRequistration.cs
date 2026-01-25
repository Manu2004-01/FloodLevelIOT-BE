using Core.Interfaces;
using Infrastructure.DBContext;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure
{
    public static class InfrastructureRequistration
    {
        public static IServiceCollection InfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<ManageDBContext>(option =>
            {
                option.UseNpgsql(configuration.GetConnectionString("CoreDb"), npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(120);
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
                });
                option.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
            );

            services.AddDbContext<EventsDBContext>(option =>
            {
                option.UseNpgsql(configuration.GetConnectionString("EventsDb"), npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(120);
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
                });
                option.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
            );

            services.AddMemoryCache();

            return services;
        }

        public static async Task InfrastructureConfigMiddleware(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ManageDBContext>();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect && env.IsDevelopment())
                {
                    await context.Database.EnsureCreatedAsync();
                }
            }
            catch
            {
                if (env.IsDevelopment())
                {
                    await context.Database.EnsureCreatedAsync();
                }
            }

            var eventsContext = scope.ServiceProvider.GetRequiredService<EventsDBContext>();
            try
            {
                var canConnectEvents = await eventsContext.Database.CanConnectAsync();
                if (!canConnectEvents && env.IsDevelopment())
                {
                    await eventsContext.Database.EnsureCreatedAsync();
                }
            }
            catch
            {
                if (env.IsDevelopment())
                {
                    await eventsContext.Database.EnsureCreatedAsync();
                }
            }
        }
    }
}
