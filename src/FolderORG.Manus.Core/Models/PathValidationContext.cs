using System;
using System.Collections.Generic;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Provides context for path validation operations.
    /// </summary>
    public class PathValidationContext
    {
        /// <summary>
        /// Gets or sets whether to allow long paths (exceeding the standard Windows 260 character limit).
        /// </summary>
        public bool AllowLongPaths { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to check if the path exists during validation.
        /// </summary>
        public bool CheckExistence { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to check write permissions during validation.
        /// </summary>
        public bool CheckWritePermissions { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to check read permissions during validation.
        /// </summary>
        public bool CheckReadPermissions { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to create directories if they don't exist.
        /// </summary>
        public bool CreateDirectories { get; set; } = false;

        /// <summary>
        /// Gets or sets the base directory for relative paths.
        /// If null, the current directory is used.
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed path length.
        /// Default is 260 characters (standard Windows limit).
        /// </summary>
        public int MaxPathLength { get; set; } = 260;

        /// <summary>
        /// Gets or sets whether to resolve environment variables in paths.
        /// </summary>
        public bool ResolveEnvironmentVariables { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to normalize paths (remove redundancies, standardize separators).
        /// </summary>
        public bool NormalizePath { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to allow only existing paths.
        /// </summary>
        public bool RequireExistingPath { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the path should be a file.
        /// If true, validation will check that the path is a file or could be a valid file.
        /// </summary>
        public bool ExpectFile { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the path should be a directory.
        /// If true, validation will check that the path is a directory or could be a valid directory.
        /// </summary>
        public bool ExpectDirectory { get; set; } = false;

        /// <summary>
        /// Gets or sets the dictionary of variable names to their values for variable resolution.
        /// </summary>
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a new instance of PathValidationContext with default settings.
        /// </summary>
        public PathValidationContext()
        {
            // Default constructor with settings initialized above
        }

        /// <summary>
        /// Creates a copy of this context.
        /// </summary>
        /// <returns>A new PathValidationContext with the same settings.</returns>
        public PathValidationContext Clone()
        {
            var clone = new PathValidationContext
            {
                AllowLongPaths = this.AllowLongPaths,
                CheckExistence = this.CheckExistence,
                CheckWritePermissions = this.CheckWritePermissions,
                CheckReadPermissions = this.CheckReadPermissions,
                CreateDirectories = this.CreateDirectories,
                BaseDirectory = this.BaseDirectory,
                MaxPathLength = this.MaxPathLength,
                ResolveEnvironmentVariables = this.ResolveEnvironmentVariables,
                NormalizePath = this.NormalizePath,
                RequireExistingPath = this.RequireExistingPath,
                ExpectFile = this.ExpectFile,
                ExpectDirectory = this.ExpectDirectory
            };

            // Copy variables dictionary
            foreach (var kvp in this.Variables)
            {
                clone.Variables[kvp.Key] = kvp.Value;
            }

            return clone;
        }

        /// <summary>
        /// Adds or updates a variable in the Variables dictionary.
        /// </summary>
        /// <param name="name">Variable name.</param>
        /// <param name="value">Variable value.</param>
        public void SetVariable(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

            Variables[name] = value;
        }
    }

    /// <summary>
    /// Represents access permissions for a path.
    /// </summary>
    public enum PathPermissions
    {
        /// <summary>
        /// No permissions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Read permission.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Write permission.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Execute permission.
        /// </summary>
        Execute = 4,

        /// <summary>
        /// All permissions.
        /// </summary>
        All = Read | Write | Execute
    }
} 