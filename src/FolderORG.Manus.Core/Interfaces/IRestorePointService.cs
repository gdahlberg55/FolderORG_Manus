using FolderORG.Manus.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderORG.Manus.Core.Interfaces
{
    /// <summary>
    /// Interface for managing restore points and system state snapshots.
    /// </summary>
    public interface IRestorePointService
    {
        /// <summary>
        /// Creates a new restore point with the current system state.
        /// </summary>
        /// <param name="name">Name of the restore point.</param>
        /// <param name="description">Description of the restore point.</param>
        /// <param name="transactionId">Optional transaction ID to associate with this restore point.</param>
        /// <returns>The created restore point.</returns>
        Task<RestorePoint> CreateRestorePointAsync(string name, string description, Guid? transactionId = null);

        /// <summary>
        /// Gets a restore point by its ID.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to retrieve.</param>
        /// <returns>The restore point if found; otherwise, null.</returns>
        Task<RestorePoint?> GetRestorePointAsync(Guid restorePointId);

        /// <summary>
        /// Gets all restore points within a specified date range.
        /// </summary>
        /// <param name="startDate">Start date of the range.</param>
        /// <param name="endDate">End date of the range.</param>
        /// <returns>Collection of restore points within the date range.</returns>
        Task<IEnumerable<RestorePoint>> GetRestorePointsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets all restore points associated with a specific transaction.
        /// </summary>
        /// <param name="transactionId">ID of the transaction.</param>
        /// <returns>Collection of restore points associated with the transaction.</returns>
        Task<IEnumerable<RestorePoint>> GetRestorePointsByTransactionAsync(Guid transactionId);

        /// <summary>
        /// Restores the system to a specified restore point.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to restore from.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progress">Callback for reporting progress (0-100).</param>
        /// <returns>Result of the restoration process.</returns>
        Task<RestoreResult> RestoreFromPointAsync(
            Guid restorePointId, 
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null);

        /// <summary>
        /// Verifies the integrity of a restore point's backup data.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to verify.</param>
        /// <returns>Verification result with details of any issues found.</returns>
        Task<VerificationResult> VerifyRestorePointAsync(Guid restorePointId);

        /// <summary>
        /// Deletes a restore point.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to delete.</param>
        /// <returns>True if the restore point was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteRestorePointAsync(Guid restorePointId);

        /// <summary>
        /// Exports a restore point to a file.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to export.</param>
        /// <param name="exportPath">Path where the export file will be saved.</param>
        /// <param name="includeFileBackups">Whether to include file backups in the export.</param>
        /// <returns>Path to the exported file.</returns>
        Task<string> ExportRestorePointAsync(Guid restorePointId, string exportPath, bool includeFileBackups = true);

        /// <summary>
        /// Imports a restore point from a file.
        /// </summary>
        /// <param name="importPath">Path to the import file.</param>
        /// <returns>The imported restore point.</returns>
        Task<RestorePoint> ImportRestorePointAsync(string importPath);

        /// <summary>
        /// Creates a preview of what would be restored from a specific restore point.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to preview.</param>
        /// <returns>Preview of the restoration actions.</returns>
        Task<RestorePreview> CreateRestorePreviewAsync(Guid restorePointId);

        /// <summary>
        /// Gets the total number of restore points.
        /// </summary>
        /// <returns>Count of restore points in the system.</returns>
        Task<int> GetRestorePointCountAsync();

        /// <summary>
        /// Gets the total disk space used by restore points.
        /// </summary>
        /// <returns>Size in bytes used by all restore points.</returns>
        Task<long> GetRestorePointStorageSizeAsync();

        /// <summary>
        /// Applies a cleanup policy to remove old or redundant restore points.
        /// </summary>
        /// <param name="maxAge">Maximum age of restore points to keep.</param>
        /// <param name="maxCount">Maximum number of restore points to keep.</param>
        /// <param name="maxSize">Maximum total size of restore points in bytes.</param>
        /// <returns>Number of restore points removed.</returns>
        Task<int> ApplyCleanupPolicyAsync(TimeSpan maxAge, int maxCount, long maxSize);

        /// <summary>
        /// Restores selected files from a specified restore point.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point to restore from.</param>
        /// <param name="selectedFilePaths">Collection of file paths to restore. If null or empty, all files will be restored.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progress">Callback for reporting progress (0-100).</param>
        /// <returns>Result of the selective restoration process.</returns>
        Task<RestoreResult> RestoreSelectedFilesAsync(
            Guid restorePointId,
            IEnumerable<string> selectedFilePaths,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null);
    }

    /// <summary>
    /// Represents a restore point for system recovery.
    /// </summary>
    public class RestorePoint
    {
        /// <summary>
        /// Unique identifier for the restore point.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the restore point.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the restore point.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the restore point was created.
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Associated transaction ID, if applicable.
        /// </summary>
        public Guid? TransactionId { get; set; }

        /// <summary>
        /// Path to the snapshot file for this restore point.
        /// </summary>
        public string SnapshotPath { get; set; } = string.Empty;

        /// <summary>
        /// Size of the restore point data in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Whether this restore point includes file backups.
        /// </summary>
        public bool IncludesFileBackups { get; set; }

        /// <summary>
        /// Number of files included in the restore point.
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// Status of the restore point.
        /// </summary>
        public RestorePointStatus Status { get; set; } = RestorePointStatus.Valid;

        /// <summary>
        /// Type of the restore point.
        /// </summary>
        public RestorePointType Type { get; set; } = RestorePointType.Manual;

        /// <summary>
        /// Tags associated with this restore point.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// User who created the restore point.
        /// </summary>
        public string CreatedBy { get; set; } = Environment.UserName;
    }

    /// <summary>
    /// Status of a restore point.
    /// </summary>
    public enum RestorePointStatus
    {
        /// <summary>
        /// Restore point is valid and can be used.
        /// </summary>
        Valid,

        /// <summary>
        /// Restore point is invalid or corrupted.
        /// </summary>
        Invalid,

        /// <summary>
        /// Restore point is in the process of being created.
        /// </summary>
        Creating,

        /// <summary>
        /// Restore point has been partially created.
        /// </summary>
        Partial,

        /// <summary>
        /// Restore point is marked for deletion.
        /// </summary>
        MarkedForDeletion
    }

    /// <summary>
    /// Type of restore point.
    /// </summary>
    public enum RestorePointType
    {
        /// <summary>
        /// Manually created restore point.
        /// </summary>
        Manual,

        /// <summary>
        /// Automatically created restore point.
        /// </summary>
        Automatic,

        /// <summary>
        /// Restore point created before an operation.
        /// </summary>
        PreOperation,

        /// <summary>
        /// Restore point created as part of a scheduled backup.
        /// </summary>
        Scheduled,

        /// <summary>
        /// System-created restore point.
        /// </summary>
        System
    }

    /// <summary>
    /// Result of a restore operation.
    /// </summary>
    public class RestoreResult
    {
        /// <summary>
        /// Whether the restore was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ID of the associated restore point.
        /// </summary>
        public Guid RestorePointId { get; set; }

        /// <summary>
        /// Error message if the restore failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of files successfully restored.
        /// </summary>
        public int FilesRestored { get; set; }

        /// <summary>
        /// Number of files that failed to restore.
        /// </summary>
        public int FilesFailed { get; set; }

        /// <summary>
        /// List of files that were successfully restored.
        /// </summary>
        public List<string> RestoredFiles { get; set; } = new List<string>();

        /// <summary>
        /// List of files that failed to restore with error details.
        /// </summary>
        public Dictionary<string, string> FailedFiles { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Time taken to complete the restore operation.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Result of verifying a restore point.
    /// </summary>
    public class VerificationResult
    {
        /// <summary>
        /// Whether the verification was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Restore point being verified.
        /// </summary>
        public Guid RestorePointId { get; set; }

        /// <summary>
        /// Number of files verified.
        /// </summary>
        public int FilesVerified { get; set; }

        /// <summary>
        /// Number of files with integrity issues.
        /// </summary>
        public int FilesWithIssues { get; set; }

        /// <summary>
        /// List of files with integrity issues and error details.
        /// </summary>
        public Dictionary<string, string> Issues { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Whether the snapshot file is intact.
        /// </summary>
        public bool SnapshotIntact { get; set; }

        /// <summary>
        /// Whether the restore point metadata is valid.
        /// </summary>
        public bool MetadataValid { get; set; }
    }

    /// <summary>
    /// Preview of a restore operation.
    /// </summary>
    public class RestorePreview
    {
        /// <summary>
        /// ID of the restore point being previewed.
        /// </summary>
        public Guid RestorePointId { get; set; }

        /// <summary>
        /// Files that will be restored.
        /// </summary>
        public List<RestoreFilePreview> FilesToRestore { get; set; } = new List<RestoreFilePreview>();

        /// <summary>
        /// Total size of files to be restored in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Estimated time to complete the restore in seconds.
        /// </summary>
        public int EstimatedTimeInSeconds { get; set; }

        /// <summary>
        /// Potential conflicts that might occur during restore.
        /// </summary>
        public List<RestoreConflict> PotentialConflicts { get; set; } = new List<RestoreConflict>();
    }

    /// <summary>
    /// Preview of a file to be restored.
    /// </summary>
    public class RestoreFilePreview
    {
        /// <summary>
        /// Source path in the backup.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Target path where the file will be restored.
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Whether the file exists at the target location.
        /// </summary>
        public bool TargetExists { get; set; }

        /// <summary>
        /// Whether the source file exists in the backup.
        /// </summary>
        public bool SourceExists { get; set; }

        /// <summary>
        /// Type of operation (e.g., Restore, Skip, Merge).
        /// </summary>
        public string Operation { get; set; } = "Restore";
    }

    /// <summary>
    /// Represents a potential conflict during restore.
    /// </summary>
    public class RestoreConflict
    {
        /// <summary>
        /// Path of the file with a conflict.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Type of conflict.
        /// </summary>
        public string ConflictType { get; set; } = string.Empty;

        /// <summary>
        /// Description of the conflict.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Available resolution strategies.
        /// </summary>
        public List<string> ResolutionOptions { get; set; } = new List<string>();

        /// <summary>
        /// Recommended resolution option.
        /// </summary>
        public string RecommendedResolution { get; set; } = string.Empty;
    }
} 