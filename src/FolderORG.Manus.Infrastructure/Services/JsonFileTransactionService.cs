using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FolderORG.Manus.Infrastructure.Services
{
    /// <summary>
    /// JSON-based implementation of the IFileTransactionService interface.
    /// </summary>
    public class JsonFileTransactionService : IFileTransactionService
    {
        private readonly string _transactionsDirectory;
        private readonly string _backupsDirectory;
        private readonly string _snapshotsDirectory;
        private readonly ILogger<JsonFileTransactionService> _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<Guid, FileOperationTransaction> _transactionCache = new Dictionary<Guid, FileOperationTransaction>();

        /// <summary>
        /// Initializes a new instance of the JsonFileTransactionService.
        /// </summary>
        /// <param name="baseDirectory">Base directory for transaction storage.</param>
        /// <param name="logger">Logger for recording operations.</param>
        public JsonFileTransactionService(string baseDirectory, ILogger<JsonFileTransactionService> logger)
        {
            _transactionsDirectory = Path.Combine(baseDirectory, "Transactions");
            _backupsDirectory = Path.Combine(baseDirectory, "Backups");
            _snapshotsDirectory = Path.Combine(baseDirectory, "Snapshots");
            _logger = logger;

            // Ensure directories exist
            Directory.CreateDirectory(_transactionsDirectory);
            Directory.CreateDirectory(_backupsDirectory);
            Directory.CreateDirectory(_snapshotsDirectory);
        }

        /// <inheritdoc />
        public async Task<FileOperationTransaction> CreateTransactionAsync(string name, string description, TransactionType type = TransactionType.Manual)
        {
            await _lock.WaitAsync();
            try
            {
                var transaction = new FileOperationTransaction
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    Type = type,
                    StartTime = DateTime.Now,
                    Status = TransactionStatus.Pending
                };

                // Save to cache
                _transactionCache[transaction.Id] = transaction;

                // Save to disk
                await SaveTransactionToDiskAsync(transaction);

                _logger.LogInformation("Created new transaction: {TransactionId} - {TransactionName}", transaction.Id, transaction.Name);
                return transaction;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<FileOperationRecord> AddOperationAsync(Guid transactionId, string sourcePath, string destinationPath, string operationType)
        {
            await _lock.WaitAsync();
            try
            {
                var transaction = await GetTransactionInternalAsync(transactionId);
                if (transaction == null)
                    throw new ArgumentException($"Transaction with ID {transactionId} not found.", nameof(transactionId));

                if (transaction.Status != TransactionStatus.Pending)
                    throw new InvalidOperationException($"Cannot add operations to a transaction in {transaction.Status} status.");

                var operation = new FileOperationRecord
                {
                    Id = Guid.NewGuid(),
                    SequenceNumber = transaction.Operations.Count + 1,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    OperationType = operationType,
                    Status = OperationStatus.Pending,
                    FileSize = File.Exists(sourcePath) ? new FileInfo(sourcePath).Length : 0
                };

                transaction.Operations.Add(operation);

                // Save updated transaction
                await SaveTransactionToDiskAsync(transaction);

                _logger.LogDebug("Added operation to transaction {TransactionId}: {OperationType} from {SourcePath} to {DestinationPath}",
                    transactionId, operationType, sourcePath, destinationPath);

                return operation;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<FileOperationTransaction> ExecuteTransactionAsync(
            Guid transactionId,
            bool createBackups = true,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            await _lock.WaitAsync();
            FileOperationTransaction transaction;
            try
            {
                transaction = await GetTransactionInternalAsync(transactionId);
                if (transaction == null)
                    throw new ArgumentException($"Transaction with ID {transactionId} not found.", nameof(transactionId));

                if (transaction.Status != TransactionStatus.Pending)
                    throw new InvalidOperationException($"Cannot execute a transaction in {transaction.Status} status.");

                if (transaction.Operations.Count == 0)
                    throw new InvalidOperationException("Cannot execute a transaction with no operations.");

                // Update transaction status
                transaction.Status = TransactionStatus.InProgress;
                await SaveTransactionToDiskAsync(transaction);
            }
            finally
            {
                _lock.Release();
            }

            try
            {
                // Execute operations
                int totalOperations = transaction.Operations.Count;
                int completedOperations = 0;
                int failedOperations = 0;

                foreach (var operation in transaction.Operations)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Transaction {TransactionId} execution was cancelled.", transactionId);
                        break;
                    }

                    try
                    {
                        // Update operation status
                        operation.Status = OperationStatus.InProgress;
                        operation.ExecutionTime = DateTime.Now;

                        // Report progress
                        progress?.Report((
                            (int)((double)completedOperations / totalOperations * 100),
                            $"Processing {Path.GetFileName(operation.SourcePath)}"
                        ));

                        // Create backup if needed
                        if (createBackups && File.Exists(operation.SourcePath) && operation.OperationType != "Delete")
                        {
                            operation.BackupPath = await CreateFileBackupAsync(operation.SourcePath);
                            operation.FileHash = await CalculateFileHashAsync(operation.SourcePath);
                            operation.BackupVerified = await VerifyFileIntegrityAsync(operation.BackupPath, operation.FileHash);

                            if (!operation.BackupVerified)
                            {
                                throw new InvalidOperationException($"Failed to verify backup integrity for {operation.SourcePath}");
                            }
                        }

                        // Execute the operation
                        switch (operation.OperationType)
                        {
                            case "Move":
                                // Ensure target directory exists
                                Directory.CreateDirectory(Path.GetDirectoryName(operation.DestinationPath));
                                File.Move(operation.SourcePath, operation.DestinationPath);
                                break;

                            case "Copy":
                                // Ensure target directory exists
                                Directory.CreateDirectory(Path.GetDirectoryName(operation.DestinationPath));
                                File.Copy(operation.SourcePath, operation.DestinationPath);
                                break;

                            case "Delete":
                                File.Delete(operation.SourcePath);
                                break;

                            default:
                                throw new NotSupportedException($"Operation type {operation.OperationType} is not supported.");
                        }

                        // Update operation status
                        operation.Status = OperationStatus.Completed;
                        completedOperations++;
                        transaction.SuccessfulOperations++;
                    }
                    catch (Exception ex)
                    {
                        // Update operation status
                        operation.Status = OperationStatus.Failed;
                        operation.ErrorMessage = ex.Message;
                        failedOperations++;
                        transaction.FailedOperations++;

                        _logger.LogError(ex, "Error executing operation {OperationId} in transaction {TransactionId}: {ErrorMessage}",
                            operation.Id, transactionId, ex.Message);
                    }

                    // Report progress after each operation
                    progress?.Report((
                        (int)((double)(completedOperations + failedOperations) / totalOperations * 100),
                        $"Completed {completedOperations} of {totalOperations} operations"
                    ));
                }

                // Update transaction status
                await _lock.WaitAsync();
                try
                {
                    transaction.CompletionTime = DateTime.Now;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        transaction.Status = TransactionStatus.Aborted;
                    }
                    else if (failedOperations > 0)
                    {
                        if (completedOperations > 0)
                        {
                            transaction.Status = TransactionStatus.PartiallyCompleted;
                        }
                        else
                        {
                            transaction.Status = TransactionStatus.Failed;
                        }
                    }
                    else
                    {
                        transaction.Status = TransactionStatus.Completed;
                    }

                    await SaveTransactionToDiskAsync(transaction);

                    // Create a state snapshot
                    if (transaction.Status == TransactionStatus.Completed || transaction.Status == TransactionStatus.PartiallyCompleted)
                    {
                        transaction.SnapshotPath = await CreateStateSnapshotAsync(transactionId);
                        transaction.LastBackupTime = DateTime.Now;
                        await SaveTransactionToDiskAsync(transaction);
                    }

                    _logger.LogInformation("Transaction {TransactionId} executed with status {Status}. " +
                                         "Successful: {SuccessfulOperations}, Failed: {FailedOperations}",
                        transactionId, transaction.Status, completedOperations, failedOperations);

                    return transaction;
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception ex)
            {
                // Update transaction status in case of unexpected error
                await _lock.WaitAsync();
                try
                {
                    transaction.Status = TransactionStatus.Failed;
                    transaction.CompletionTime = DateTime.Now;
                    await SaveTransactionToDiskAsync(transaction);

                    _logger.LogError(ex, "Error executing transaction {TransactionId}: {ErrorMessage}",
                        transactionId, ex.Message);

                    return transaction;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> RollbackTransactionAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            await _lock.WaitAsync();
            FileOperationTransaction transaction;
            try
            {
                transaction = await GetTransactionInternalAsync(transactionId);
                if (transaction == null)
                    throw new ArgumentException($"Transaction with ID {transactionId} not found.", nameof(transactionId));

                if (transaction.Status != TransactionStatus.Completed && 
                    transaction.Status != TransactionStatus.PartiallyCompleted && 
                    transaction.Status != TransactionStatus.Failed)
                {
                    throw new InvalidOperationException($"Cannot rollback a transaction in {transaction.Status} status.");
                }

                if (!transaction.CanRollback)
                    throw new InvalidOperationException("This transaction cannot be rolled back.");

                // Create a rollback transaction
                var rollbackTransaction = new FileOperationTransaction
                {
                    Id = Guid.NewGuid(),
                    Name = $"Rollback of {transaction.Name}",
                    Description = $"Rollback of transaction {transactionId}",
                    Type = TransactionType.Rollback,
                    StartTime = DateTime.Now,
                    Status = TransactionStatus.Pending,
                    ParentTransactionId = transactionId
                };

                await SaveTransactionToDiskAsync(rollbackTransaction);
                
                // Add inverse operations to the rollback transaction
                // We need to process operations in reverse order
                var operationsToRollback = transaction.Operations
                    .Where(o => o.Status == OperationStatus.Completed)
                    .OrderByDescending(o => o.SequenceNumber)
                    .ToList();

                foreach (var operation in operationsToRollback)
                {
                    string rollbackOperationType;
                    string rollbackSourcePath;
                    string rollbackDestinationPath;

                    // Determine the inverse operation
                    switch (operation.OperationType)
                    {
                        case "Move":
                            rollbackOperationType = "Move";
                            rollbackSourcePath = operation.DestinationPath;
                            rollbackDestinationPath = operation.SourcePath;
                            break;

                        case "Copy":
                            rollbackOperationType = "Delete";
                            rollbackSourcePath = operation.DestinationPath;
                            rollbackDestinationPath = string.Empty; // Not needed for delete
                            break;

                        case "Delete":
                            if (string.IsNullOrEmpty(operation.BackupPath) || !File.Exists(operation.BackupPath))
                            {
                                // Can't restore a deleted file without a backup
                                continue;
                            }
                            rollbackOperationType = "Copy";
                            rollbackSourcePath = operation.BackupPath;
                            rollbackDestinationPath = operation.SourcePath;
                            break;

                        default:
                            continue; // Skip unsupported operation types
                    }

                    // Add the rollback operation
                    var rollbackOperation = new FileOperationRecord
                    {
                        Id = Guid.NewGuid(),
                        SequenceNumber = rollbackTransaction.Operations.Count + 1,
                        SourcePath = rollbackSourcePath,
                        DestinationPath = rollbackDestinationPath,
                        OperationType = rollbackOperationType,
                        Status = OperationStatus.Pending,
                        RollbackOperationId = operation.Id,
                        Metadata = new Dictionary<string, string>
                        {
                            ["OriginalOperation"] = operation.OperationType,
                            ["OriginalOperationId"] = operation.Id.ToString()
                        }
                    };

                    rollbackTransaction.Operations.Add(rollbackOperation);
                }

                // Save the rollback transaction
                await SaveTransactionToDiskAsync(rollbackTransaction);
            }
            finally
            {
                _lock.Release();
            }

            // Execute the rollback transaction
            var executedRollbackTransaction = await ExecuteTransactionAsync(
                transaction.Id,
                false, // No need to create backups for rollback operations
                cancellationToken,
                progress);

            bool success = executedRollbackTransaction.Status == TransactionStatus.Completed;

            // Update the original transaction
            await _lock.WaitAsync();
            try
            {
                transaction.Status = TransactionStatus.RolledBack;
                transaction.Metadata["RollbackTransactionId"] = executedRollbackTransaction.Id.ToString();
                transaction.Metadata["RollbackTime"] = DateTime.Now.ToString("o");
                transaction.Metadata["RollbackSuccess"] = success.ToString();
                await SaveTransactionToDiskAsync(transaction);
            }
            finally
            {
                _lock.Release();
            }

            return success;
        }

        /// <inheritdoc />
        public async Task<FileOperationTransaction?> GetTransactionAsync(Guid transactionId)
        {
            await _lock.WaitAsync();
            try
            {
                return await GetTransactionInternalAsync(transactionId);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FileOperationTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = new List<FileOperationTransaction>();
            
            await _lock.WaitAsync();
            try
            {
                foreach (var file in Directory.GetFiles(_transactionsDirectory, "*.json"))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var transaction = JsonSerializer.Deserialize<FileOperationTransaction>(json);
                        
                        if (transaction != null && transaction.StartTime >= startDate && transaction.StartTime <= endDate)
                        {
                            transactions.Add(transaction);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading transaction file {FileName}: {ErrorMessage}", file, ex.Message);
                    }
                }
                
                return transactions;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FileOperationTransaction>> GetTransactionsByTypeAsync(TransactionType type)
        {
            var transactions = new List<FileOperationTransaction>();
            
            await _lock.WaitAsync();
            try
            {
                foreach (var file in Directory.GetFiles(_transactionsDirectory, "*.json"))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var transaction = JsonSerializer.Deserialize<FileOperationTransaction>(json);
                        
                        if (transaction != null && transaction.Type == type)
                        {
                            transactions.Add(transaction);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading transaction file {FileName}: {ErrorMessage}", file, ex.Message);
                    }
                }
                
                return transactions;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<string> CreateStateSnapshotAsync(Guid transactionId)
        {
            await _lock.WaitAsync();
            try
            {
                var transaction = await GetTransactionInternalAsync(transactionId);
                if (transaction == null)
                    throw new ArgumentException($"Transaction with ID {transactionId} not found.", nameof(transactionId));

                // Create a deep copy of the transaction for the snapshot
                var snapshot = JsonSerializer.Deserialize<FileOperationTransaction>(
                    JsonSerializer.Serialize(transaction));

                if (snapshot == null)
                    throw new InvalidOperationException("Failed to create snapshot from transaction.");

                // Generate snapshot file path
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string snapshotFileName = $"Snapshot_{transactionId}_{timestamp}.json";
                string snapshotPath = Path.Combine(_snapshotsDirectory, snapshotFileName);

                // Save snapshot to disk
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(snapshotPath, json);

                _logger.LogInformation("Created state snapshot for transaction {TransactionId} at {SnapshotPath}",
                    transactionId, snapshotPath);

                return snapshotPath;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<Guid> RestoreFromSnapshotAsync(
            string snapshotPath,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            if (!File.Exists(snapshotPath))
                throw new ArgumentException($"Snapshot file not found: {snapshotPath}", nameof(snapshotPath));

            // Read snapshot file
            var json = await File.ReadAllTextAsync(snapshotPath);
            var snapshot = JsonSerializer.Deserialize<FileOperationTransaction>(json);

            if (snapshot == null)
                throw new InvalidOperationException($"Failed to deserialize snapshot from {snapshotPath}");

            // Create a new transaction based on the snapshot
            await _lock.WaitAsync();
            FileOperationTransaction restoreTransaction;
            try
            {
                restoreTransaction = new FileOperationTransaction
                {
                    Id = Guid.NewGuid(),
                    Name = $"Restore from {Path.GetFileName(snapshotPath)}",
                    Description = $"Restoration of transaction {snapshot.Id} from snapshot",
                    Type = TransactionType.System,
                    StartTime = DateTime.Now,
                    Status = TransactionStatus.Pending,
                    ParentTransactionId = snapshot.Id,
                    Metadata = new Dictionary<string, string>
                    {
                        ["SnapshotPath"] = snapshotPath,
                        ["OriginalTransactionId"] = snapshot.Id.ToString(),
                        ["RestoreTime"] = DateTime.Now.ToString("o")
                    }
                };

                await SaveTransactionToDiskAsync(restoreTransaction);
            }
            finally
            {
                _lock.Release();
            }

            // We'll need to process the operations from the snapshot to create the proper restore operations
            // This depends on your specific restoration logic and needs careful handling
            
            // For demonstration purposes, we'll just log the restoration
            _logger.LogInformation("Restored transaction {TransactionId} from snapshot {SnapshotPath}",
                restoreTransaction.Id, snapshotPath);

            return restoreTransaction.Id;
        }

        /// <inheritdoc />
        public async Task<string> CalculateFileHashAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <inheritdoc />
        public async Task<bool> VerifyFileIntegrityAsync(string filePath, string expectedHash)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                string actualHash = await CalculateFileHashAsync(filePath);
                return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying file integrity for {FilePath}: {ErrorMessage}",
                    filePath, ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> CreateFileBackupAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);

            string fileName = Path.GetFileName(filePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            string backupPath = Path.Combine(_backupsDirectory, backupFileName);

            // Ensure backup directory structure exists
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath));

            // Create backup
            File.Copy(filePath, backupPath);

            _logger.LogDebug("Created backup of {FilePath} at {BackupPath}", filePath, backupPath);

            return backupPath;
        }

        // Private helper methods

        private async Task<FileOperationTransaction?> GetTransactionInternalAsync(Guid transactionId)
        {
            // Check cache first
            if (_transactionCache.TryGetValue(transactionId, out var cachedTransaction))
                return cachedTransaction;

            // Try to load from disk
            string filePath = Path.Combine(_transactionsDirectory, $"{transactionId}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var transaction = JsonSerializer.Deserialize<FileOperationTransaction>(json);
                    
                    if (transaction != null)
                    {
                        _transactionCache[transactionId] = transaction;
                    }
                    
                    return transaction;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading transaction file {FileName}: {ErrorMessage}", filePath, ex.Message);
                }
            }
            
            return null;
        }

        private async Task SaveTransactionToDiskAsync(FileOperationTransaction transaction)
        {
            // Update cache
            _transactionCache[transaction.Id] = transaction;
            
            // Save to disk
            string filePath = Path.Combine(_transactionsDirectory, $"{transaction.Id}.json");
            var json = JsonSerializer.Serialize(transaction, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
    }
} 