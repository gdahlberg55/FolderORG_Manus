using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FolderORG.Manus.Domain.Extensions
{
    /// <summary>
    /// Extension methods for IPathValidator.
    /// </summary>
    public static class PathValidatorExtensions
    {
        /// <summary>
        /// Validates that the path exists and is a file.
        /// </summary>
        /// <param name="validator">The path validator.</param>
        /// <param name="filePath">The file path to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A validation result indicating if the path is a valid file.</returns>
        public static async Task<ValidationResult> ValidateFileExistsAsync(
            this IPathValidator validator, 
            string filePath, 
            CancellationToken cancellationToken = default)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            
            var context = new PathValidationContext
            {
                ExpectFile = true,
                RequireExistingPath = true,
                CheckReadPermissions = true
            };
            
            return await validator.ValidatePathAsync(filePath, context, cancellationToken);
        }
        
        /// <summary>
        /// Validates that the path exists and is a directory.
        /// </summary>
        /// <param name="validator">The path validator.</param>
        /// <param name="directoryPath">The directory path to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A validation result indicating if the path is a valid directory.</returns>
        public static async Task<ValidationResult> ValidateDirectoryExistsAsync(
            this IPathValidator validator, 
            string directoryPath, 
            CancellationToken cancellationToken = default)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            
            var context = new PathValidationContext
            {
                ExpectDirectory = true,
                RequireExistingPath = true,
                CheckReadPermissions = true
            };
            
            return await validator.ValidatePathAsync(directoryPath, context, cancellationToken);
        }
        
        /// <summary>
        /// Validates that a path can be written to.
        /// </summary>
        /// <param name="validator">The path validator.</param>
        /// <param name="path">The path to validate.</param>
        /// <param name="isFile">Whether the path is a file path. If false, it is considered a directory path.</param>
        /// <param name="createIfMissing">Whether to create the directory if it doesn't exist.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A validation result indicating if the path is writable.</returns>
        public static async Task<ValidationResult> ValidateWritablePathAsync(
            this IPathValidator validator, 
            string path, 
            bool isFile, 
            bool createIfMissing = false,
            CancellationToken cancellationToken = default)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            
            var context = new PathValidationContext
            {
                ExpectFile = isFile,
                ExpectDirectory = !isFile,
                CheckWritePermissions = true,
                CreateDirectories = createIfMissing
            };
            
            return await validator.ValidatePathAsync(path, context, cancellationToken);
        }
        
        /// <summary>
        /// Ensures a directory exists, creating it if necessary, and validates it is writable.
        /// </summary>
        /// <param name="validator">The path validator.</param>
        /// <param name="directoryPath">The directory path to create and validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A validation result indicating if the directory exists and is writable.</returns>
        public static async Task<ValidationResult> EnsureDirectoryExistsAsync(
            this IPathValidator validator, 
            string directoryPath, 
            CancellationToken cancellationToken = default)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            
            var context = new PathValidationContext
            {
                ExpectDirectory = true,
                CheckWritePermissions = true,
                CreateDirectories = true
            };
            
            var result = await validator.ValidatePathAsync(directoryPath, context, cancellationToken);
            
            // Additional verification after creation attempt
            if (result.IsValid && !Directory.Exists(result.NormalizedPath))
            {
                result.AddIssue(
                    $"Failed to create directory at: {result.NormalizedPath}", 
                    ValidationSeverity.Error, 
                    "DIR_CREATE_VERIFICATION_FAILED");
            }
            
            return result;
        }
        
        /// <summary>
        /// Normalizes a path and resolves variables.
        /// </summary>
        /// <param name="validator">The path validator.</param>
        /// <param name="path">The path to normalize and resolve.</param>
        /// <param name="variables">Optional variables to use for resolution.</param>
        /// <returns>The normalized and resolved path.</returns>
        public static string NormalizeAndResolvePath(
            this IPathValidator validator, 
            string path, 
            params (string name, string value)[] variables)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            
            var context = new PathValidationContext
            {
                NormalizePath = true,
                ResolveEnvironmentVariables = true
            };
            
            // Add any variables provided
            if (variables != null)
            {
                foreach (var (name, value) in variables)
                {
                    context.SetVariable(name, value);
                }
            }
            
            // First normalize, then resolve variables
            string normalizedPath = validator.NormalizePath(path, context);
            return validator.ResolveVariables(normalizedPath, context);
        }
        
        /// <summary>
        /// Simple method to check if a path is valid without checking existence or permissions.
        /// </summary>
        /// <param name="validator">The path validator.</param>
        /// <param name="path">The path to validate.</param>
        /// <returns>True if the path format is valid, false otherwise.</returns>
        public static bool IsValidPathFormat(this IPathValidator validator, string path)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            
            var context = new PathValidationContext
            {
                CheckExistence = false,
                CheckReadPermissions = false,
                CheckWritePermissions = false,
                NormalizePath = true
            };
            
            var result = validator.ValidatePath(path, context);
            return result.IsValid;
        }
    }
} 