using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Domain.Rules.Services
{
    /// <summary>
    /// Service for validating file paths with variable resolution, path normalization, and error reporting.
    /// Implements the path validation system for rule target paths.
    /// </summary>
    public class PathValidationService : IPathValidationService
    {
        private readonly Dictionary<string, Func<string, Task<string>>> _variableResolvers;
        private readonly Dictionary<string, string> _cachedResolutions;

        public PathValidationService()
        {
            _variableResolvers = new Dictionary<string, Func<string, Task<string>>>();
            _cachedResolutions = new Dictionary<string, string>();
            
            // Register built-in variable resolvers
            RegisterBuiltInVariableResolvers();
        }

        /// <summary>
        /// Validates a path, resolving any variables and checking for path validity.
        /// </summary>
        /// <param name="path">The path to validate, which may contain variables.</param>
        /// <param name="context">Optional context information for validation.</param>
        /// <returns>A validation result containing the resolved path and any validation issues.</returns>
        public async Task<PathValidationResult> ValidatePathAsync(string path, PathValidationContext context = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new PathValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Path cannot be empty",
                    OriginalPath = path
                };
            }

            try
            {
                // Resolve variables in the path
                string resolvedPath = await ResolveVariablesAsync(path, context);
                
                // Normalize the path
                string normalizedPath = NormalizePath(resolvedPath);
                
                // Validate the normalized path
                var validationIssues = ValidatePath(normalizedPath);
                
                return new PathValidationResult
                {
                    IsValid = validationIssues.Count == 0,
                    ResolvedPath = normalizedPath,
                    OriginalPath = path,
                    ValidationIssues = validationIssues,
                    ErrorMessage = validationIssues.Count > 0 ? string.Join("; ", validationIssues) : null
                };
            }
            catch (Exception ex)
            {
                return new PathValidationResult
                {
                    IsValid = false,
                    OriginalPath = path,
                    ErrorMessage = $"Error validating path: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Resolves variables in a path using registered variable resolvers.
        /// </summary>
        /// <param name="path">The path containing variables to resolve.</param>
        /// <param name="context">Optional context information for variable resolution.</param>
        /// <returns>The path with all variables resolved.</returns>
        public async Task<string> ResolveVariablesAsync(string path, PathValidationContext context = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            // Pattern for variable placeholders: ${variableName} or %variableName%
            var pattern = @"\$\{([^}]+)\}|%([^%]+)%";
            
            return await Regex.Replace(path, pattern, async match =>
            {
                // Extract variable name from either ${name} or %name% format
                string variableName = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                
                // Check if we have already resolved this variable
                string cacheKey = $"{variableName}:{context?.ContextId ?? "default"}";
                if (_cachedResolutions.TryGetValue(cacheKey, out string resolvedValue))
                    return resolvedValue;
                
                // Try to resolve the variable
                string value = await ResolveVariableAsync(variableName, context);
                
                // Cache the resolution if successful
                if (!string.IsNullOrEmpty(value))
                    _cachedResolutions[cacheKey] = value;
                
                return value ?? match.Value; // Return original if resolution failed
            }, RegexOptions.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Resolves a single variable using the appropriate resolver.
        /// </summary>
        /// <param name="variableName">The name of the variable to resolve.</param>
        /// <param name="context">Optional context information for variable resolution.</param>
        /// <returns>The resolved value or null if resolution failed.</returns>
        private async Task<string> ResolveVariableAsync(string variableName, PathValidationContext context)
        {
            // First check for environment variables
            string envValue = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrEmpty(envValue))
                return envValue;
            
            // Check for context-provided variables
            if (context?.Variables != null && context.Variables.TryGetValue(variableName, out string contextValue))
                return contextValue;
            
            // Check for registered variable resolvers
            if (_variableResolvers.TryGetValue(variableName, out var resolver))
                return await resolver(variableName);
            
            // Special case for date/time variables using format: date:format
            if (variableName.StartsWith("date:", StringComparison.OrdinalIgnoreCase))
            {
                string format = variableName.Substring(5);
                return string.IsNullOrEmpty(format) ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Now.ToString(format);
            }
            
            // Failed to resolve
            return null;
        }

        /// <summary>
        /// Normalizes a path by removing redundancies and ensuring proper format.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        public string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            
            try
            {
                // Convert to absolute path if it's not already
                if (!Path.IsPathRooted(path))
                {
                    // If no root, assume it's relative to the current directory
                    path = Path.Combine(Environment.CurrentDirectory, path);
                }
                
                // Use GetFullPath to normalize the path (resolves . and .. segments)
                string fullPath = Path.GetFullPath(path);
                
                // Ensure consistent directory separators
                fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar);
                
                return fullPath;
            }
            catch (Exception)
            {
                // If normalization fails, return the original path
                return path;
            }
        }

        /// <summary>
        /// Validates a path for various issues like existence, permissions, etc.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>A list of validation issues, empty if the path is valid.</returns>
        private List<string> ValidatePath(string path)
        {
            var issues = new List<string>();
            
            if (string.IsNullOrWhiteSpace(path))
            {
                issues.Add("Path is empty");
                return issues;
            }
            
            try
            {
                // Check for invalid characters in path
                if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                    issues.Add("Path contains invalid characters");
                
                // Check path length
                if (path.Length > 260 && !path.StartsWith(@"\\?\"))
                    issues.Add("Path exceeds maximum length (260 characters)");
                
                // Check if path exists
                bool isDirectory = Directory.Exists(path);
                bool isFile = File.Exists(path);
                
                if (!isDirectory && !isFile)
                {
                    // Path doesn't exist, check if parent directory exists
                    string directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        issues.Add($"Parent directory does not exist: {directory}");
                }
                
                // Check for permissions (just attempt to access for read)
                if (isDirectory)
                {
                    try
                    {
                        Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        issues.Add("Insufficient permissions to access directory");
                    }
                }
                else if (isFile)
                {
                    try
                    {
                        using (File.OpenRead(path)) { }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        issues.Add("Insufficient permissions to access file");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error validating path: {ex.Message}");
            }
            
            return issues;
        }

        /// <summary>
        /// Registers a custom variable resolver.
        /// </summary>
        /// <param name="variableName">The variable name to register.</param>
        /// <param name="resolver">The resolver function that returns the variable value.</param>
        public void RegisterVariableResolver(string variableName, Func<string, Task<string>> resolver)
        {
            if (string.IsNullOrWhiteSpace(variableName))
                throw new ArgumentException("Variable name cannot be empty", nameof(variableName));
            
            _variableResolvers[variableName] = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// Registers the built-in variable resolvers.
        /// </summary>
        private void RegisterBuiltInVariableResolvers()
        {
            // Common special folders
            RegisterVariableResolver("Desktop", _ => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
            RegisterVariableResolver("Documents", _ => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            RegisterVariableResolver("Pictures", _ => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));
            RegisterVariableResolver("Music", _ => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));
            RegisterVariableResolver("Videos", _ => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)));
            RegisterVariableResolver("Downloads", _ => Task.FromResult(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")));
            RegisterVariableResolver("AppData", _ => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
            RegisterVariableResolver("Temp", _ => Task.FromResult(Path.GetTempPath()));
            
            // Date/time variables with common formats
            RegisterVariableResolver("Today", _ => Task.FromResult(DateTime.Now.ToString("yyyy-MM-dd")));
            RegisterVariableResolver("Year", _ => Task.FromResult(DateTime.Now.Year.ToString()));
            RegisterVariableResolver("Month", _ => Task.FromResult(DateTime.Now.Month.ToString()));
            RegisterVariableResolver("Day", _ => Task.FromResult(DateTime.Now.Day.ToString()));
            
            // System variables
            RegisterVariableResolver("UserName", _ => Task.FromResult(Environment.UserName));
            RegisterVariableResolver("MachineName", _ => Task.FromResult(Environment.MachineName));
        }

        /// <summary>
        /// Clears the variable resolution cache.
        /// </summary>
        public void ClearCache()
        {
            _cachedResolutions.Clear();
        }
    }
} 