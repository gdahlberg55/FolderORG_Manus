using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FolderORG.Manus.Domain.Services
{
    /// <summary>
    /// Implementation of the IPathValidator interface for validating file and directory paths.
    /// </summary>
    public class PathValidator : IPathValidator
    {
        private const int MAX_PATH = 260;
        private const int MAX_DIRECTORY_PATH = 248;
        private static readonly Regex InvalidCharsRegex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidPathChars()))}]", RegexOptions.Compiled);
        private static readonly Regex InvalidFileNameCharsRegex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);
        private static readonly Regex EnvironmentVariableRegex = new Regex(@"%([^%]+)%", RegexOptions.Compiled);
        private static readonly Regex VariableRegex = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);

        // Reserved Windows device names that cannot be used as filenames
        private static readonly HashSet<string> ReservedDeviceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Validates a path according to the specified context.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="context">The validation context containing validation settings.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A validation result containing the normalized path and any validation issues.</returns>
        public async Task<ValidationResult> ValidatePathAsync(string path, PathValidationContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ValidationResult.Failure("Path cannot be null or empty.", "PATH_EMPTY");
            }

            var result = new ValidationResult();

            try
            {
                // Apply normalization if requested in context
                string normalizedPath = path;
                if (context.NormalizePath)
                {
                    normalizedPath = NormalizePath(path, context);
                }

                // Resolve variables if requested in context
                if (context.ResolveEnvironmentVariables || context.Variables.Count > 0)
                {
                    normalizedPath = ResolveVariables(normalizedPath, context);
                }

                // Set the normalized path in the result
                result.NormalizedPath = normalizedPath;

                // Validate path format
                ValidatePathFormat(normalizedPath, context, result);

                if (!result.IsValid)
                {
                    return result;
                }

                // Check path length
                ValidatePathLength(normalizedPath, context, result);

                if (!result.IsValid)
                {
                    return result;
                }

                // Check if path has valid characters
                ValidatePathCharacters(normalizedPath, context, result);

                if (!result.IsValid)
                {
                    return result;
                }

                // Check if path segments have valid names (not reserved device names, etc.)
                ValidatePathSegments(normalizedPath, result);

                if (!result.IsValid)
                {
                    return result;
                }

                // Check if the path's parent directory exists
                if (context.CheckExistence)
                {
                    await ValidatePathExistenceAsync(normalizedPath, context, result, cancellationToken);
                }

                if (!result.IsValid)
                {
                    return result;
                }

                // Check permissions
                if (context.CheckReadPermissions || context.CheckWritePermissions)
                {
                    var permissionResult = await CheckPermissionsAsync(normalizedPath, context, cancellationToken);
                    result.MergeWith(permissionResult);
                }

                // Create directories if needed and requested
                if (context.CreateDirectories && context.ExpectDirectory)
                {
                    try
                    {
                        if (!Directory.Exists(normalizedPath))
                        {
                            Directory.CreateDirectory(normalizedPath);
                            result.AddIssue("Directory was created as it did not exist.", ValidationSeverity.Information, "DIR_CREATED");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AddIssue($"Failed to create directory: {ex.Message}", ValidationSeverity.Error, "DIR_CREATE_FAILED");
                    }
                }
                else if (context.CreateDirectories && context.ExpectFile)
                {
                    try
                    {
                        string directoryName = Path.GetDirectoryName(normalizedPath);
                        if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                            result.AddIssue("Parent directory was created as it did not exist.", ValidationSeverity.Information, "DIR_CREATED");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AddIssue($"Failed to create parent directory: {ex.Message}", ValidationSeverity.Error, "DIR_CREATE_FAILED");
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddIssue($"Unexpected error during validation: {ex.Message}", ValidationSeverity.Error, "VALIDATION_ERROR");
            }

            return result;
        }

        /// <summary>
        /// Validates a path according to the specified context (synchronous version).
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="context">The validation context containing validation settings.</param>
        /// <returns>A validation result containing the normalized path and any validation issues.</returns>
        public ValidationResult ValidatePath(string path, PathValidationContext context)
        {
            return ValidatePathAsync(path, context).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Normalizes a path according to the specified context.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <param name="context">The validation context containing normalization settings.</param>
        /// <returns>The normalized path.</returns>
        public string NormalizePath(string path, PathValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            try
            {
                // Convert relative paths to absolute if base directory is provided
                string normalizedPath = path;
                if (!string.IsNullOrEmpty(context.BaseDirectory) && !Path.IsPathRooted(path))
                {
                    normalizedPath = Path.Combine(context.BaseDirectory, path);
                }

                // Normalize path separators to system-specific separator
                normalizedPath = normalizedPath.Replace('/', Path.DirectorySeparatorChar)
                                             .Replace('\\', Path.DirectorySeparatorChar);

                // Remove trailing directory separators
                normalizedPath = normalizedPath.TrimEnd(Path.DirectorySeparatorChar);

                // Resolve . and .. segments
                string[] segments = normalizedPath.Split(Path.DirectorySeparatorChar);
                var resolvedSegments = new List<string>();

                foreach (var segment in segments)
                {
                    if (segment == ".")
                    {
                        // Skip current directory marker
                        continue;
                    }
                    else if (segment == "..")
                    {
                        // Go up one directory level
                        if (resolvedSegments.Count > 0)
                        {
                            resolvedSegments.RemoveAt(resolvedSegments.Count - 1);
                        }
                        else
                        {
                            // Keep .. at the beginning if we can't go up further
                            resolvedSegments.Add(segment);
                        }
                    }
                    else if (!string.IsNullOrEmpty(segment))
                    {
                        resolvedSegments.Add(segment);
                    }
                }

                // Preserve network share notation if present
                if (path.StartsWith("\\\\") || path.StartsWith("//"))
                {
                    normalizedPath = "\\\\" + string.Join(Path.DirectorySeparatorChar.ToString(), resolvedSegments);
                }
                else
                {
                    normalizedPath = string.Join(Path.DirectorySeparatorChar.ToString(), resolvedSegments);
                    
                    // Add root separator back if it was a rooted path
                    if (Path.IsPathRooted(path) && normalizedPath.Length > 0)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            // For Windows, add drive letter back if there was one
                            if (path.Length >= 2 && path[1] == ':' && char.IsLetter(path[0]))
                            {
                                normalizedPath = path.Substring(0, 2) + Path.DirectorySeparatorChar + normalizedPath;
                            }
                            else
                            {
                                normalizedPath = Path.DirectorySeparatorChar + normalizedPath;
                            }
                        }
                        else
                        {
                            // For Linux/Mac, add leading slash
                            normalizedPath = Path.DirectorySeparatorChar + normalizedPath;
                        }
                    }
                }

                // Make sure to handle drive-relative paths on Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
                    path.Length >= 2 && path[1] == ':' && !path.Contains(Path.DirectorySeparatorChar))
                {
                    // Drive-relative path like "C:file.txt" should be preserved
                    return path;
                }

                return normalizedPath;
            }
            catch (Exception)
            {
                // If normalization fails, return the original path
                return path;
            }
        }

        /// <summary>
        /// Resolves variables in a path according to the specified context.
        /// </summary>
        /// <param name="path">The path containing variables to resolve.</param>
        /// <param name="context">The validation context containing variable definitions.</param>
        /// <returns>The path with all variables resolved.</returns>
        public string ResolveVariables(string path, PathValidationContext context)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string resolvedPath = path;

            // Resolve environment variables if requested
            if (context.ResolveEnvironmentVariables)
            {
                resolvedPath = EnvironmentVariableRegex.Replace(resolvedPath, match =>
                {
                    string varName = match.Groups[1].Value;
                    string varValue = Environment.GetEnvironmentVariable(varName);
                    return varValue ?? match.Value;
                });
            }

            // Resolve custom variables from context
            if (context.Variables.Count > 0)
            {
                resolvedPath = VariableRegex.Replace(resolvedPath, match =>
                {
                    string varName = match.Groups[1].Value;
                    if (context.Variables.TryGetValue(varName, out string varValue))
                    {
                        return varValue;
                    }
                    return match.Value;
                });
            }

            return resolvedPath;
        }

        /// <summary>
        /// Checks if the path has necessary permissions for the requested operation.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="context">The validation context providing additional validation parameters.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A validation result containing permission-related issues.</returns>
        public async Task<ValidationResult> CheckPermissionsAsync(string path, PathValidationContext context, CancellationToken cancellationToken = default)
        {
            var result = new ValidationResult();
            
            try
            {
                // For permission checks, we need to check if the path exists first
                bool pathExists = File.Exists(path) || Directory.Exists(path);
                
                // If the path doesn't exist, check the parent directory for write permissions
                if (!pathExists)
                {
                    if (context.CheckWritePermissions)
                    {
                        string directoryPath = context.ExpectFile ? Path.GetDirectoryName(path) : path;
                        
                        // If directory path is empty, we can't check permissions
                        if (string.IsNullOrEmpty(directoryPath))
                        {
                            return result;
                        }
                        
                        // If the directory doesn't exist, go up the tree until we find an existing directory
                        while (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                        {
                            directoryPath = Path.GetDirectoryName(directoryPath);
                        }
                        
                        // If we couldn't find an existing directory, we can't check permissions
                        if (string.IsNullOrEmpty(directoryPath))
                        {
                            result.AddIssue("Could not check write permissions because no parent directory exists.", 
                                ValidationSeverity.Warning, "PARENT_DIR_NOT_FOUND");
                            return result;
                        }
                        
                        // Check write permissions on the existing parent directory
                        if (!HasWritePermission(directoryPath))
                        {
                            result.AddIssue($"Write permission denied on parent directory: {directoryPath}", 
                                ValidationSeverity.Error, "WRITE_PERMISSION_DENIED");
                        }
                    }
                }
                else
                {
                    // Path exists, check permissions as requested
                    if (context.CheckReadPermissions)
                    {
                        if (!HasReadPermission(path))
                        {
                            result.AddIssue($"Read permission denied: {path}", 
                                ValidationSeverity.Error, "READ_PERMISSION_DENIED");
                        }
                    }
                    
                    if (context.CheckWritePermissions)
                    {
                        if (!HasWritePermission(path))
                        {
                            result.AddIssue($"Write permission denied: {path}", 
                                ValidationSeverity.Error, "WRITE_PERMISSION_DENIED");
                        }
                    }
                }
                
                // Allow for some async behavior even though most file system operations are synchronous
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                result.AddIssue($"Error checking permissions: {ex.Message}", 
                    ValidationSeverity.Error, "PERMISSION_CHECK_ERROR");
            }
            
            return result;
        }

        #region Private Helper Methods

        private void ValidatePathFormat(string path, PathValidationContext context, ValidationResult result)
        {
            // Ensure path is not too short
            if (path.Length < 2)
            {
                if (path != "/" && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && path != "\\"))
                {
                    result.AddIssue("Path is too short to be valid.", ValidationSeverity.Error, "PATH_TOO_SHORT");
                }
            }

            // Ensure path is properly formatted according to OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Check for UNC paths
                if (path.StartsWith("\\\\"))
                {
                    string[] parts = path.Substring(2).Split('\\');
                    if (parts.Length < 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                    {
                        result.AddIssue("UNC path must include server and share name.", ValidationSeverity.Error, "INVALID_UNC_PATH");
                    }
                }
                // Check for drive-letter paths
                else if (path.Length >= 2 && path[1] == ':')
                {
                    if (!char.IsLetter(path[0]))
                    {
                        result.AddIssue("Drive letter must be a letter (A-Z).", ValidationSeverity.Error, "INVALID_DRIVE_LETTER");
                    }
                    
                    if (path.Length > 2 && path[2] != '\\')
                    {
                        result.AddIssue("Drive letter must be followed by ':\\'.", ValidationSeverity.Warning, "INVALID_DRIVE_FORMAT");
                    }
                }
                // Non-rooted paths without drive letter are relative, and that's fine
            }
            else
            {
                // Unix-like paths should start with / for absolute paths, otherwise they're relative
                // Both are valid, so no validation needed
            }
        }

        private void ValidatePathLength(string path, PathValidationContext context, ValidationResult result)
        {
            if (path.Length > context.MaxPathLength && !context.AllowLongPaths)
            {
                result.AddIssue($"Path exceeds maximum allowed length of {context.MaxPathLength} characters. Path length: {path.Length}", 
                    ValidationSeverity.Error, "PATH_TOO_LONG");
            }
            else if (path.Length > MAX_PATH && path.Length <= context.MaxPathLength && context.AllowLongPaths)
            {
                result.AddIssue($"Path exceeds standard MAX_PATH limit (260 characters), but is allowed by configuration. Consider using long path aware operations.", 
                    ValidationSeverity.Warning, "LONG_PATH");
            }

            // Check if the filename part is too long
            if (context.ExpectFile)
            {
                string fileName = Path.GetFileName(path);
                if (fileName.Length > 255)
                {
                    result.AddIssue($"Filename exceeds maximum allowed length of 255 characters. Filename length: {fileName.Length}", 
                        ValidationSeverity.Error, "FILENAME_TOO_LONG");
                }
            }
        }

        private void ValidatePathCharacters(string path, PathValidationContext context, ValidationResult result)
        {
            // Check for invalid path characters
            if (InvalidCharsRegex.IsMatch(path))
            {
                string invalidChars = string.Join(", ", Path.GetInvalidPathChars().Select(c => $"'{c}'"));
                result.AddIssue($"Path contains invalid characters: {invalidChars}", ValidationSeverity.Error, "INVALID_PATH_CHARS");
            }

            // Check for invalid filename characters in the filename part
            if (context.ExpectFile)
            {
                string fileName = Path.GetFileName(path);
                if (InvalidFileNameCharsRegex.IsMatch(fileName))
                {
                    string invalidChars = string.Join(", ", Path.GetInvalidFileNameChars().Select(c => $"'{c}'"));
                    result.AddIssue($"Filename contains invalid characters: {invalidChars}", ValidationSeverity.Error, "INVALID_FILENAME_CHARS");
                }
            }
        }

        private void ValidatePathSegments(string path, ValidationResult result)
        {
            string[] segments = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string segment in segments)
            {
                // Skip segments that are drive letters with colon in Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
                    segment.Length == 2 && segment[1] == ':' && char.IsLetter(segment[0]))
                {
                    continue;
                }

                // Check for reserved device names (Windows)
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string segmentName = segment;
                    int dotIndex = segment.IndexOf('.');
                    if (dotIndex > 0)
                    {
                        segmentName = segment.Substring(0, dotIndex);
                    }

                    if (ReservedDeviceNames.Contains(segmentName))
                    {
                        result.AddIssue($"Path segment '{segment}' is a reserved device name and cannot be used.", 
                            ValidationSeverity.Error, "RESERVED_NAME");
                    }
                }

                // Check for segments ending with periods or spaces
                if (segment.EndsWith(".") || segment.EndsWith(" "))
                {
                    result.AddIssue($"Path segment '{segment}' ends with a period or space, which can cause issues.", 
                        ValidationSeverity.Warning, "TRAILING_PERIOD_OR_SPACE");
                }
            }
        }

        private async Task ValidatePathExistenceAsync(string path, PathValidationContext context, ValidationResult result, CancellationToken cancellationToken)
        {
            try
            {
                bool isFile = File.Exists(path);
                bool isDirectory = Directory.Exists(path);
                bool exists = isFile || isDirectory;

                // Check if path exists when required
                if (context.RequireExistingPath && !exists)
                {
                    result.AddIssue($"Path does not exist: {path}", ValidationSeverity.Error, "PATH_NOT_FOUND");
                }

                // Check file/directory type expectations
                if (exists)
                {
                    if (context.ExpectFile && !isFile)
                    {
                        result.AddIssue($"Expected a file but found a directory: {path}", ValidationSeverity.Error, "EXPECTED_FILE");
                    }
                    else if (context.ExpectDirectory && !isDirectory)
                    {
                        result.AddIssue($"Expected a directory but found a file: {path}", ValidationSeverity.Error, "EXPECTED_DIRECTORY");
                    }
                }
                else if (!exists && !context.CreateDirectories)
                {
                    // Path doesn't exist - check if parent directory exists
                    string directoryPath = context.ExpectFile ? Path.GetDirectoryName(path) : path;
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        result.AddIssue($"Parent directory does not exist: {directoryPath}", ValidationSeverity.Warning, "PARENT_DIR_NOT_FOUND");
                    }
                }

                // Allow for cancellation
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                result.AddIssue($"Error checking path existence: {ex.Message}", ValidationSeverity.Error, "EXISTENCE_CHECK_ERROR");
            }
        }

        private bool HasReadPermission(string path)
        {
            try
            {
                // For directories, we check if we can list files
                if (Directory.Exists(path))
                {
                    Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                    return true;
                }
                // For files, we check if we can open for reading
                else if (File.Exists(path))
                {
                    using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool HasWritePermission(string path)
        {
            try
            {
                // For directories, check directory permissions
                if (Directory.Exists(path))
                {
                    // Try to get directory security directly
                    DirectorySecurity ds = Directory.GetAccessControl(path);
                    var rules = ds.GetAccessRules(true, true, typeof(SecurityIdentifier));
                    var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);

                    bool hasWritePermission = false;
                    foreach (FileSystemAccessRule rule in rules)
                    {
                        if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                            continue;

                        if (rule.IdentityReference.Value == identity.User.Value)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                                hasWritePermission = true;
                            else if (rule.AccessControlType == AccessControlType.Deny)
                                return false;
                        }
                        
                        // Check if permission applies to a group the user is in
                        foreach (var group in identity.Groups)
                        {
                            if (rule.IdentityReference.Value == group.Value)
                            {
                                if (rule.AccessControlType == AccessControlType.Allow)
                                    hasWritePermission = true;
                                else if (rule.AccessControlType == AccessControlType.Deny)
                                    return false;
                            }
                        }
                    }
                    
                    if (hasWritePermission)
                        return true;

                    // Fallback: Try to create a temporary file in the directory
                    string testFile = Path.Combine(path, $"{Guid.NewGuid()}.tmp");
                    using (FileStream fs = File.Create(testFile, 1, FileOptions.DeleteOnClose))
                    {
                        return true;
                    }
                }
                // For files, check file permissions
                else if (File.Exists(path))
                {
                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        return false;

                    // Try to open the file for writing
                    using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        return true;
                    }
                }
                // For non-existent paths, we check the parent directory
                else
                {
                    string parentDir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(parentDir))
                        return HasWritePermission(parentDir);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
} 