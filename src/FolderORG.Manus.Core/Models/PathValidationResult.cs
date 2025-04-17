using System.Collections.Generic;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents the result of a path validation operation.
    /// </summary>
    public class PathValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the path is valid.
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Gets or sets the original path before validation.
        /// </summary>
        public string OriginalPath { get; set; }
        
        /// <summary>
        /// Gets or sets the resolved path after variable substitution and normalization.
        /// </summary>
        public string ResolvedPath { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if the path is invalid.
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets a list of validation issues with the path.
        /// </summary>
        public List<string> ValidationIssues { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets suggested corrections for the path if available.
        /// </summary>
        public List<string> SuggestedCorrections { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets a value indicating whether the path was modified during validation.
        /// </summary>
        public bool WasModified => !string.IsNullOrEmpty(OriginalPath) && !string.IsNullOrEmpty(ResolvedPath) && OriginalPath != ResolvedPath;
    }
} 