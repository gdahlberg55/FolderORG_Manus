using FolderORG.Manus.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderORG.Manus.Core.Interfaces
{
    /// <summary>
    /// Interface for handling transaction-based file operations with rollback capabilities.
    /// </summary>
    public interface IFileTransactionService
    {
        /// <summary>
        /// Creates a new file operation transaction.
        /// </summary>
        /// <param name="name">Name of the transaction.</param>
        /// <param name="description">Description of the transaction.</param>
        /// <param name="type">Type of the transaction.</param>
        /// <returns>The created transaction.</returns>
        Task<FileOperationTransaction> CreateTransactionAsync(string name, string description, TransactionType type = TransactionType.Manual);

        /// <summary>
        /// Adds a file operation to an existing transaction.
        /// </summary>
        /// <param name="transactionId">ID of the transaction to add the operation to.</param>
        /// <param name="sourcePath">Source file path.</param>
        /// <param name="destinationPath">Destination file path.</param>
        /// <param name="operationType">Type of operation.</param>
        /// <returns>The created operation record.</returns>
        Task<FileOperationRecord> AddOperationAsync(Guid transactionId, string sourcePath, string destinationPath, string operationType);

        /// <summary>
        /// Begins execution of a transaction.
        /// </summary>
        /// <param name="transactionId">ID of the transaction to execute.</param>
        /// <param name="createBackups">Whether to create backups of files for potential rollback.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progressCallback">Callback for reporting progress (0-100).</param>
        /// <returns>The executed transaction with updated status.</returns>
        Task<FileOperationTransaction> ExecuteTransactionAsync(
            Guid transactionId,
            bool createBackups = true,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null);

        /// <summary>
        /// Rolls back a previously executed transaction.
        /// </summary>
        /// <param name="transactionId">ID of the transaction to roll back.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progressCallback">Callback for reporting progress (0-100).</param>
        /// <returns>True if rollback was successful; otherwise, false.</returns>
        Task<bool> RollbackTransactionAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null);

        /// <summary>
        /// Gets a transaction by its ID.
        /// </summary>
        /// <param name="transactionId">ID of the transaction to retrieve.</param>
        /// <returns>The transaction if found; otherwise, null.</returns>
        Task<FileOperationTransaction?> GetTransactionAsync(Guid transactionId);

        /// <summary>
        /// Gets all transactions within a specified date range.
        /// </summary>
        /// <param name="startDate">Start date of the range.</param>
        /// <param name="endDate">End date of the range.</param>
        /// <returns>Collection of transactions within the date range.</returns>
        Task<IEnumerable<FileOperationTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets all transactions of a specified type.
        /// </summary>
        /// <param name="type">Type of transactions to retrieve.</param>
        /// <returns>Collection of transactions of the specified type.</returns>
        Task<IEnumerable<FileOperationTransaction>> GetTransactionsByTypeAsync(TransactionType type);

        /// <summary>
        /// Creates a state snapshot for a transaction.
        /// </summary>
        /// <param name="transactionId">ID of the transaction to snapshot.</param>
        /// <returns>Path to the created snapshot file.</returns>
        Task<string> CreateStateSnapshotAsync(Guid transactionId);

        /// <summary>
        /// Restores from a state snapshot.
        /// </summary>
        /// <param name="snapshotPath">Path to the snapshot file.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progressCallback">Callback for reporting progress (0-100).</param>
        /// <returns>ID of the newly created transaction representing the restore operation.</returns>
        Task<Guid> RestoreFromSnapshotAsync(
            string snapshotPath,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null);

        /// <summary>
        /// Calculates a hash for a file to be used for verification.
        /// </summary>
        /// <param name="filePath">Path to the file to hash.</param>
        /// <returns>Hash string for the file.</returns>
        Task<string> CalculateFileHashAsync(string filePath);

        /// <summary>
        /// Verifies the integrity of a file against its stored hash.
        /// </summary>
        /// <param name="filePath">Path to the file to verify.</param>
        /// <param name="expectedHash">Expected hash value.</param>
        /// <returns>True if the file hash matches the expected hash; otherwise, false.</returns>
        Task<bool> VerifyFileIntegrityAsync(string filePath, string expectedHash);

        /// <summary>
        /// Creates a backup of a file for potential rollback.
        /// </summary>
        /// <param name="filePath">Path of the file to back up.</param>
        /// <returns>Path to the backup file.</returns>
        Task<string> CreateFileBackupAsync(string filePath);
    }
} 