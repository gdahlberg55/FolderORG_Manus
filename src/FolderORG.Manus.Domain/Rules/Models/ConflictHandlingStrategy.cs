namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Defines strategies for handling file naming conflicts
    /// </summary>
    public enum ConflictHandlingStrategy
    {
        /// <summary>
        /// Append a number to the filename (file.txt -> file (1).txt)
        /// </summary>
        AppendNumber = 0,
        
        /// <summary>
        /// Add a timestamp to the filename (file.txt -> file_20230405123456.txt)
        /// </summary>
        AddTimestamp = 1,
        
        /// <summary>
        /// Replace the existing file
        /// </summary>
        Overwrite = 2,
        
        /// <summary>
        /// Skip the operation if a conflict occurs
        /// </summary>
        Skip = 3,
        
        /// <summary>
        /// Ask the user how to handle the conflict
        /// </summary>
        AskUser = 4,
        
        /// <summary>
        /// Only copy newer files
        /// </summary>
        KeepNewer = 5,
        
        /// <summary>
        /// Append content for text files, overwrite for others
        /// </summary>
        AppendContent = 6
    }
} 