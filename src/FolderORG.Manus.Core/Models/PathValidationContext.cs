using System.Collections.Generic;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Provides context information for path validation operations.
    /// </summary>
    public class PathValidationContext
    {
        /// <summary>
        /// Gets or sets a unique identifier for this validation context.
        /// </summary>
        public string ContextId { get; set; }
        
        /// <summary>
        /// Gets or sets a dictionary of variables and their values for this context.
        /// </summary>
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets or sets a value indicating whether to create directories that don't exist.
        /// </summary>
        public bool CreateDirectories { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to validate permissions.
        /// </summary>
        public bool ValidatePermissions { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to check for path existence.
        /// </summary>
        public bool CheckExistence { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to allow long paths.
        /// </summary>
        public bool AllowLongPaths { get; set; }
        
        /// <summary>
        /// Gets or sets the base directory for resolving relative paths.
        /// </summary>
        public string BaseDirectory { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to normalize the path.
        /// </summary>
        public bool NormalizePath { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the source file or directory path associated with this validation context.
        /// </summary>
        public string SourcePath { get; set; }
        
        /// <summary>
        /// Gets or sets metadata about the file being validated, which can be used for variable resolution.
        /// </summary>
        public FileMetadata FileMetadata { get; set; }
    }
} 