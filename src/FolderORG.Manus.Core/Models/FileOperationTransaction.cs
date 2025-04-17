using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents a transaction for file operations, enabling atomic operations and rollback capabilities.
    /// </summary>
    public class FileOperationTransaction
    {
        /// <summary>
        /// Unique identifier for the transaction.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the transaction for display and identification purposes.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the transaction, providing additional context.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the transaction was started.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Date and time when the transaction was completed.
        /// </summary>
        public DateTime? CompletionTime { get; set; }

        /// <summary>
        /// Current status of the transaction.
        /// </summary>
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        /// <summary>
        /// Type of batch the transaction represents.
        /// </summary>
        public TransactionType Type { get; set; } = TransactionType.Manual;

        /// <summary>
        /// Collection of individual file operations included in this transaction.
        /// </summary>
        public List<FileOperationRecord> Operations { get; set; } = new List<FileOperationRecord>();

        /// <summary>
        /// Optional parent transaction ID if this is part of a larger transaction group.
        /// </summary>
        public Guid? ParentTransactionId { get; set; }

        /// <summary>
        /// Metadata and additional properties associated with the transaction.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Number of operations successfully completed.
        /// </summary>
        public int SuccessfulOperations { get; set; } = 0;

        /// <summary>
        /// Number of operations that failed.
        /// </summary>
        public int FailedOperations { get; set; } = 0;

        /// <summary>
        /// Timestamp of when this transaction was last backed up as a restore point.
        /// </summary>
        public DateTime? LastBackupTime { get; set; }

        /// <summary>
        /// Path to the snapshot file if a state snapshot was created.
        /// </summary>
        public string? SnapshotPath { get; set; }

        /// <summary>
        /// Whether this transaction can be rolled back.
        /// </summary>
        public bool CanRollback { get; set; } = true;

        /// <summary>
        /// User who initiated this transaction.
        /// </summary>
        public string InitiatedBy { get; set; } = Environment.UserName;
    }

    /// <summary>
    /// Status of a file operation transaction.
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Transaction is pending and has not been executed.
        /// </summary>
        Pending,

        /// <summary>
        /// Transaction is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Transaction has been successfully completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Transaction failed during execution.
        /// </summary>
        Failed,

        /// <summary>
        /// Transaction was aborted by the user.
        /// </summary>
        Aborted,

        /// <summary>
        /// Transaction was partially completed.
        /// </summary>
        PartiallyCompleted,

        /// <summary>
        /// Transaction was rolled back.
        /// </summary>
        RolledBack
    }

    /// <summary>
    /// Type of transaction batch.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Manual operation initiated by user.
        /// </summary>
        Manual,

        /// <summary>
        /// Scheduled operation run on a timer.
        /// </summary>
        Scheduled,

        /// <summary>
        /// Automatic operation triggered by an event.
        /// </summary>
        Automatic,

        /// <summary>
        /// Rollback operation to restore previous state.
        /// </summary>
        Rollback,

        /// <summary>
        /// System operation for maintenance.
        /// </summary>
        System
    }

    /// <summary>
    /// Represents a single file operation within a transaction.
    /// </summary>
    public class FileOperationRecord
    {
        /// <summary>
        /// Unique identifier for the operation.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Order of the operation within the transaction.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Type of file operation performed.
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Source path of the file.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Destination path for the file.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// Status of the operation.
        /// </summary>
        public OperationStatus Status { get; set; } = OperationStatus.Pending;

        /// <summary>
        /// Error message if the operation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when the operation was executed.
        /// </summary>
        public DateTime? ExecutionTime { get; set; }

        /// <summary>
        /// Path to the backup of the original file, if applicable.
        /// </summary>
        public string? BackupPath { get; set; }

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// File hash for verification.
        /// </summary>
        public string? FileHash { get; set; }

        /// <summary>
        /// Whether the backup was verified.
        /// </summary>
        public bool BackupVerified { get; set; }

        /// <summary>
        /// Additional metadata for the operation.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Rollback operation ID if this operation was rolled back.
        /// </summary>
        public Guid? RollbackOperationId { get; set; }
    }

    /// <summary>
    /// Status of an individual file operation.
    /// </summary>
    public enum OperationStatus
    {
        /// <summary>
        /// Operation is pending execution.
        /// </summary>
        Pending,

        /// <summary>
        /// Operation is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Operation completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Operation failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Operation was skipped.
        /// </summary>
        Skipped,

        /// <summary>
        /// Operation was rolled back.
        /// </summary>
        RolledBack
    }
} 