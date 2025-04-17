namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents an entry in the Memory Bank, storing information about an organized file.
    /// </summary>
    public class MemoryBankEntry
    {
        /// <summary>
        /// Unique identifier for the entry.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Original path of the file before organization.
        /// </summary>
        public string OriginalPath { get; set; } = string.Empty;

        /// <summary>
        /// Current path of the file after organization.
        /// </summary>
        public string CurrentPath { get; set; } = string.Empty;

        /// <summary>
        /// Original file name.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Date and time when the file was organized.
        /// </summary>
        public DateTime OrganizedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Type of operation performed (e.g., Move, Copy).
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Primary category assigned to the file.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Subcategory assigned to the file.
        /// </summary>
        public string SubCategory { get; set; } = string.Empty;

        /// <summary>
        /// Classification method used (e.g., by extension, by size).
        /// </summary>
        public string ClassificationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Tags associated with the file.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Additional attributes or metadata associated with the file.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Whether the file still exists at the organized location.
        /// </summary>
        public bool StillExists { get; set; } = true;
        
        /// <summary>
        /// Date when the entry was last verified.
        /// </summary>
        public DateTime LastVerifiedDate { get; set; } = DateTime.Now;
    }
} 