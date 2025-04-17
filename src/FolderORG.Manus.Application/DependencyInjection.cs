using System;
using FolderORG.Manus.Domain.Rules.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FolderORG.Manus.Application
{
    /// <summary>
    /// Extension methods for registering application services in the dependency injection container.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds application services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Register existing services
            // ...

            // Register Path Validation service
            services.AddSingleton<IPathValidationService, PathValidationService>();

            return services;
        }
    }
} 