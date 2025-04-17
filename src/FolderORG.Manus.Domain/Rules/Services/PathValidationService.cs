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
                var validationIssues = ValidatePath(normalizedPath, context);
                
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
            
            // File metadata tokens: file:property
            if (variableName.StartsWith("file:", StringComparison.OrdinalIgnoreCase) && context?.FileMetadata != null)
            {
                return ResolveFileMetadataToken(variableName.Substring(5), context.FileMetadata);
            }

            // Expression-based computed values: expr:expression
            if (variableName.StartsWith("expr:", StringComparison.OrdinalIgnoreCase))
            {
                return await EvaluateExpressionAsync(variableName.Substring(5), context);
            }
            
            // Failed to resolve
            return null;
        }

        /// <summary>
        /// Resolves a file metadata token to its value.
        /// </summary>
        /// <param name="property">The file metadata property to retrieve.</param>
        /// <param name="metadata">The file metadata object.</param>
        /// <returns>The resolved value or null if resolution failed.</returns>
        private string ResolveFileMetadataToken(string property, FileMetadata metadata)
        {
            if (metadata == null)
                return null;

            switch (property.ToLowerInvariant())
            {
                case "name":
                    return metadata.Name;
                case "extension":
                    return metadata.Extension;
                case "namewithoutext":
                    return Path.GetFileNameWithoutExtension(metadata.Name);
                case "creationdate":
                    return metadata.CreationDate?.ToString("yyyy-MM-dd");
                case "creationtime":
                    return metadata.CreationDate?.ToString("HH-mm-ss");
                case "modificationdate":
                    return metadata.ModificationDate?.ToString("yyyy-MM-dd");
                case "modificationtime":
                    return metadata.ModificationDate?.ToString("HH-mm-ss");
                case "size":
                    return metadata.Size.ToString();
                case "sizekb":
                    return (metadata.Size / 1024).ToString();
                case "sizemb":
                    return (metadata.Size / (1024 * 1024)).ToString();
                case "type":
                    return metadata.FileType;
                case "category":
                    return metadata.Category;
                case "hash":
                    return metadata.Hash;
                default:
                    // Try to get a custom property
                    if (metadata.CustomProperties != null && 
                        metadata.CustomProperties.TryGetValue(property, out string value))
                        return value;
                    return null;
            }
        }

        /// <summary>
        /// Evaluates an expression to produce a computed value.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="context">The validation context.</param>
        /// <returns>The result of the expression evaluation.</returns>
        private async Task<string> EvaluateExpressionAsync(string expression, PathValidationContext context)
        {
            try
            {
                // Simple expression parsing for common operations
                if (expression.Contains("+"))
                {
                    string[] parts = expression.Split('+');
                    string result = string.Empty;
                    
                    foreach (var part in parts)
                    {
                        string trimmed = part.Trim();
                        if (trimmed.StartsWith("${") && trimmed.EndsWith("}"))
                        {
                            string varName = trimmed.Substring(2, trimmed.Length - 3);
                            string resolvedValue = await ResolveVariableAsync(varName, context);
                            result += resolvedValue ?? string.Empty;
                        }
                        else
                        {
                            result += trimmed;
                        }
                    }
                    
                    return result;
                }

                // Additional complex expressions could be implemented here
                
                return null;
            }
            catch (Exception ex)
            {
                // Log the error and return null
                // TODO: Add proper logging
                return null;
            }
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
                // Replace any environment variables or user profile references
                path = Environment.ExpandEnvironmentVariables(path);
                
                // Handle Windows-specific path conventions
                if (path.StartsWith("~", StringComparison.Ordinal))
                {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                        path.Substring(1).TrimStart('/', '\\'));
                }
                
                // Convert separators to the system's preferred separator
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                
                // Handle UNC paths correctly
                bool isUnc = path.StartsWith(@"\\", StringComparison.Ordinal);
                
                // Handle long path prefix
                bool hasLongPathPrefix = path.StartsWith(@"\\?\", StringComparison.Ordinal);
                
                // Convert to absolute path if it's not already
                if (!Path.IsPathRooted(path))
                {
                    // If context has a base directory, use that as the base
                    string baseDir = Environment.CurrentDirectory;
                    path = Path.Combine(baseDir, path);
                }
                
                // Use GetFullPath to normalize the path (resolves . and .. segments)
                // This also handles redundant separators
                string fullPath = Path.GetFullPath(path);
                
                // Remove trailing separators unless it's a root directory
                if (fullPath.Length > 3 && fullPath.EndsWith(Path.DirectorySeparatorChar))
                {
                    fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar);
                }
                
                // Preserve UNC format if the original path was a UNC path
                if (isUnc && !fullPath.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    fullPath = @"\\" + fullPath.TrimStart('\\');
                }
                
                // Add long path prefix for paths exceeding MAX_PATH (260 characters)
                // if needed and if it wasn't already present
                if (!hasLongPathPrefix && fullPath.Length >= 260 && !fullPath.StartsWith(@"\\?\", StringComparison.Ordinal))
                {
                    if (isUnc)
                    {
                        fullPath = @"\\?\UNC\" + fullPath.Substring(2);
                    }
                    else
                    {
                        fullPath = @"\\?\" + fullPath;
                    }
                }
                
                // Convert to lowercase for case-insensitive comparison (Windows)
                // Note: This is safe for Windows but might not be for case-sensitive file systems
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Only lowercase the drive letter, keep the rest of the path case-sensitive
                    if (fullPath.Length >= 2 && fullPath[1] == ':')
                    {
                        char driveLetter = char.ToLowerInvariant(fullPath[0]);
                        fullPath = driveLetter + fullPath.Substring(1);
                    }
                }
                
                return fullPath;
            }
            catch (Exception ex)
            {
                // Log the error
                // TODO: Add proper logging
                
                // If normalization fails, return the original path
                return path;
            }
        }

        /// <summary>
        /// Validates a path for various issues like existence, permissions, etc.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="context">Optional context information for validation.</param>
        /// <returns>A list of validation issues, empty if the path is valid.</returns>
        private List<string> ValidatePath(string path, PathValidationContext context = null)
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
                    issues.Add("Path exceeds maximum length (260 characters). Consider using long path format (\\\\?\\)");
                
                // Check if the path should be checked for existence (default is true)
                bool checkExistence = context?.CheckExistence ?? true;
                if (checkExistence)
                {
                    // Check if path exists
                    bool isDirectory = Directory.Exists(path);
                    bool isFile = File.Exists(path);
                    
                    if (!isDirectory && !isFile)
                    {
                        // Path doesn't exist, check if parent directory exists
                        string directory = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            issues.Add($"Parent directory does not exist: {directory}");
                            
                            // Check if directories should be created automatically
                            if (context?.CreateDirectories == true)
                            {
                                try
                                {
                                    Directory.CreateDirectory(directory);
                                    // Remove the issue if creation was successful
                                    issues.RemoveAt(issues.Count - 1);
                                    issues.Add($"Parent directory created: {directory}");
                                }
                                catch (Exception ex)
                                {
                                    issues.Add($"Failed to create parent directory: {ex.Message}");
                                }
                            }
                        }
                    }
                    
                    // Check for permissions only if the path exists and permission checking is enabled
                    bool validatePermissions = context?.ValidatePermissions ?? true;
                    if (validatePermissions && (isDirectory || isFile))
                    {
                        if (isDirectory)
                        {
                            try
                            {
                                // Check read permissions by attempting to list files
                                Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                                
                                // Check write permissions by attempting to create a temporary file
                                string tempFile = Path.Combine(path, $"temp_{Guid.NewGuid()}.tmp");
                                using (File.Create(tempFile)) { }
                                File.Delete(tempFile);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                issues.Add("Insufficient permissions to access directory");
                            }
                            catch (IOException ex)
                            {
                                issues.Add($"I/O error checking directory permissions: {ex.Message}");
                            }
                        }
                        else if (isFile)
                        {
                            try
                            {
                                // Check read permissions
                                using (File.OpenRead(path)) { }
                                
                                // Check write permissions
                                if (!File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly))
                                {
                                    FileAttributes originalAttributes = File.GetAttributes(path);
                                    try
                                    {
                                        // Try to open for writing
                                        using (File.OpenWrite(path)) { }
                                    }
                                    finally
                                    {
                                        // Restore original attributes
                                        File.SetAttributes(path, originalAttributes);
                                    }
                                }
                                else
                                {
                                    issues.Add("File is read-only, cannot be modified");
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                issues.Add("Insufficient permissions to access file");
                            }
                            catch (IOException ex)
                            {
                                issues.Add($"I/O error checking file permissions: {ex.Message}");
                            }
                        }
                    }
                }

                // Check for potential issues with directory nesting
                if (path.Length > 50 && Directory.Exists(path)) 
                {
                    string[] pathSegments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (pathSegments.Length > 15)
                    {
                        issues.Add("Path has excessive directory nesting (> 15 levels), which may cause issues on some systems");
                    }

                    // Check for redundancy in path segments
                    var uniqueSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int redundantSegments = 0;
                    foreach (var segment in pathSegments)
                    {
                        if (!string.IsNullOrWhiteSpace(segment) && !uniqueSegments.Add(segment))
                        {
                            redundantSegments++;
                        }
                    }
                    
                    if (redundantSegments > 3)
                    {
                        issues.Add($"Path contains {redundantSegments} redundant named segments, consider simplifying");
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