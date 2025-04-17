using FolderORG.Manus.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace FolderORG.Manus.Core.Interfaces
{
    /// <summary>
    /// Interface for validating file and directory paths.
    /// </summary>
    public interface IPathValidator
    {
        /// <summary>
        /// Validates a path according to the specified context.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="context">The validation context containing validation settings.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A validation result containing the normalized path and any validation issues.</returns>
        Task<ValidationResult> ValidatePathAsync(string path, PathValidationContext context, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validates a path according to the specified context (synchronous version).
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="context">The validation context containing validation settings.</param>
        /// <returns>A validation result containing the normalized path and any validation issues.</returns>
        ValidationResult ValidatePath(string path, PathValidationContext context);
        
        /// <summary>
        /// Normalizes a path according to the specified context.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <param name="context">The validation context containing normalization settings.</param>
        /// <returns>The normalized path.</returns>
        string NormalizePath(string path, PathValidationContext context);
        
        /// <summary>
        /// Resolves variables in a path according to the specified context.
        /// </summary>
        /// <param name="path">The path containing variables to resolve.</param>
        /// <param name="context">The validation context containing variable definitions.</param>
        /// <returns>The path with all variables resolved.</returns>
        string ResolveVariables(string path, PathValidationContext context);

        /// <summary>
        /// Checks if the path has necessary permissions for the requested operation.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="context">The validation context providing additional validation parameters.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A validation result containing permission-related issues.</returns>
        Task<ValidationResult> CheckPermissionsAsync(
            string path, 
            PathValidationContext context, 
            CancellationToken cancellationToken = default);
    }
} 