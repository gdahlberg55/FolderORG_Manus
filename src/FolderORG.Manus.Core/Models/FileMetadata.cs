namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents metadata extracted from a file for use in classification and organization.
    /// </summary>
    public class FileMetadata
    {
        /// <summary>
        /// Full path to the file
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// File name with extension
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File extension (with the dot)
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Directory containing the file
        /// </summary>
        public string Directory { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Date when the file was created
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Date when the file was last accessed
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Date when the file was last modified
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Content type (MIME type) of the file, if available
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Indicates whether the file is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Indicates whether the file is hidden
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Indicates whether the file is a system file
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Collection of additional properties extracted from specific file types
        /// </summary>
        public Dictionary<string, object> ExtendedProperties { get; set; } = new Dictionary<string, object>();
    }
} 