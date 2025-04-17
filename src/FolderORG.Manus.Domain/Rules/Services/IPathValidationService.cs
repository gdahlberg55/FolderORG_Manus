using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Domain.Rules.Services
{
    /// <summary>
    /// Interface for path validation service that handles path validation, normalization,
    /// and variable resolution for rule target paths.
    /// </summary>
    public interface IPathValidationService
    {
        /// <summary>
        /// Validates a path, resolving any variables and checking for path validity.
        /// </summary>
        /// <param name="path">The path to validate, which may contain variables.</param>
        /// <param name="context">Optional context information for validation.</param>
        /// <returns>A validation result containing the resolved path and any validation issues.</returns>
        Task<PathValidationResult> ValidatePathAsync(string path, PathValidationContext context = null);
        
        /// <summary>
        /// Resolves variables in a path using registered variable resolvers.
        /// </summary>
        /// <param name="path">The path containing variables to resolve.</param>
        /// <param name="context">Optional context information for variable resolution.</param>
        /// <returns>The path with all variables resolved.</returns>
        Task<string> ResolveVariablesAsync(string path, PathValidationContext context = null);
        
        /// <summary>
        /// Normalizes a path by removing redundancies and ensuring proper format.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        string NormalizePath(string path);
        
        /// <summary>
        /// Registers a custom variable resolver.
        /// </summary>
        /// <param name="variableName">The variable name to register.</param>
        /// <param name="resolver">The resolver function that returns the variable value.</param>
        void RegisterVariableResolver(string variableName, Func<string, Task<string>> resolver);
        
        /// <summary>
        /// Clears the variable resolution cache.
        /// </summary>
        void ClearCache();
    }
} 