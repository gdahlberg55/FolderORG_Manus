namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Defines the types of conditions that can be applied to files
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// File name (with or without extension)
        /// </summary>
        FileName = 0,
        
        /// <summary>
        /// File extension
        /// </summary>
        FileExtension = 1,
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        FileSize = 2,
        
        /// <summary>
        /// File creation date
        /// </summary>
        CreationDate = 3,
        
        /// <summary>
        /// File last modified date
        /// </summary>
        ModifiedDate = 4,
        
        /// <summary>
        /// File last accessed date
        /// </summary>
        AccessDate = 5,
        
        /// <summary>
        /// File content (text-based)
        /// </summary>
        FileContent = 6,
        
        /// <summary>
        /// MIME type of the file
        /// </summary>
        MimeType = 7,
        
        /// <summary>
        /// Parent folder name
        /// </summary>
        ParentFolder = 8,
        
        /// <summary>
        /// File attributes (readonly, hidden, etc.)
        /// </summary>
        FileAttributes = 9,
        
        /// <summary>
        /// File owner or creator
        /// </summary>
        FileOwner = 10,
        
        /// <summary>
        /// Classification result from the classification engine
        /// </summary>
        Classification = 11
    }
} 