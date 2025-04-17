using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FolderORG.Manus.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds core services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Register Path Validation components
            services.AddSingleton<IPathValidator, PathValidator>();

            // Add more service registrations here

            return services;
        }
    }
} 