namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Defines the types of actions that can be performed on files
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Move file to a different location
        /// </summary>
        Move = 0,
        
        /// <summary>
        /// Copy file to a different location
        /// </summary>
        Copy = 1,
        
        /// <summary>
        /// Rename file without changing location
        /// </summary>
        Rename = 2,
        
        /// <summary>
        /// Delete file (with confirmation)
        /// </summary>
        Delete = 3,
        
        /// <summary>
        /// Apply tags to file
        /// </summary>
        Tag = 4,
        
        /// <summary>
        /// Modify file attributes
        /// </summary>
        SetAttributes = 5,
        
        /// <summary>
        /// Create directory structure
        /// </summary>
        CreateDirectory = 6,
        
        /// <summary>
        /// Archive file (compress)
        /// </summary>
        Archive = 7,
        
        /// <summary>
        /// Extract archived file
        /// </summary>
        Extract = 8,
        
        /// <summary>
        /// Run a custom command on the file
        /// </summary>
        CustomCommand = 9
    }
} 