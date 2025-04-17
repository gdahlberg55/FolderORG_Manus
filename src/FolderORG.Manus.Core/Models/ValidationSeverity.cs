namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Defines severity levels for validation issues.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Information that does not affect validation outcome.
        /// </summary>
        Information = 0,
        
        /// <summary>
        /// Suggests potential improvements or non-critical issues.
        /// </summary>
        Suggestion = 1,
        
        /// <summary>
        /// Indicates a potential problem that doesn't prevent path usage.
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Indicates a problem that may result in unexpected behavior but doesn't prevent operation.
        /// </summary>
        Moderate = 3,
        
        /// <summary>
        /// Indicates a serious problem that may cause operational issues.
        /// </summary>
        Error = 4,
        
        /// <summary>
        /// Indicates a critical problem that prevents path usage entirely.
        /// </summary>
        Critical = 5
    }
} 