using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Domain.Services;
using FolderORG.Manus.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

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
        
        /// <summary>
        /// Adds transaction-based file operation services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="baseDirectory">Base directory for transaction storage.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddFileTransactionServices(this IServiceCollection services, string baseDirectory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            if (string.IsNullOrEmpty(baseDirectory))
                throw new ArgumentException("Base directory cannot be null or empty.", nameof(baseDirectory));
            
            // Ensure the base directory exists
            Directory.CreateDirectory(baseDirectory);
            
            // Register File Transaction Service
            services.AddSingleton<IFileTransactionService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<JsonFileTransactionService>>();
                return new JsonFileTransactionService(baseDirectory, logger);
            });
            
            // Extend the FileOperationService with transaction support
            services.AddScoped(provider =>
            {
                var transactionService = provider.GetRequiredService<IFileTransactionService>();
                return new FolderORG.Manus.Application.Services.FileOperationService(transactionService);
            });
            
            return services;
        }
        
        /// <summary>
        /// Adds restore point services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="baseDirectory">Base directory for restore point storage.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddRestorePointServices(this IServiceCollection services, string baseDirectory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            if (string.IsNullOrEmpty(baseDirectory))
                throw new ArgumentException("Base directory cannot be null or empty.", nameof(baseDirectory));
            
            // Ensure the base directory exists
            Directory.CreateDirectory(baseDirectory);
            
            // Register Restore Point Service
            services.AddSingleton<IRestorePointService>(provider =>
            {
                var fileTransactionService = provider.GetRequiredService<IFileTransactionService>();
                var logger = provider.GetRequiredService<ILogger<RestorePointService>>();
                return new RestorePointService(baseDirectory, fileTransactionService, logger);
            });
            
            return services;
        }
    }
} 