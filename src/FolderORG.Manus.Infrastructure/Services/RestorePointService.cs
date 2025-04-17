using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
    /// Implementation of the IRestorePointService interface.
    /// </summary>
    public class RestorePointService : IRestorePointService
    {
        private readonly string _restorePointsDirectory;
        private readonly string _snapshotsDirectory;
        private readonly IFileTransactionService _fileTransactionService;
        private readonly ILogger<RestorePointService> _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        
        /// <summary>
        /// Initializes a new instance of the RestorePointService.
        /// </summary>
        /// <param name="baseDirectory">Base directory for restore point storage.</param>
        /// <param name="fileTransactionService">Transaction service for file operations.</param>
        /// <param name="logger">Logger for recording operations.</param>
        public RestorePointService(
            string baseDirectory,
            IFileTransactionService fileTransactionService,
            ILogger<RestorePointService> logger)
        {
            _restorePointsDirectory = Path.Combine(baseDirectory, "RestorePoints");
            _snapshotsDirectory = Path.Combine(baseDirectory, "Snapshots");
            _fileTransactionService = fileTransactionService ?? throw new ArgumentNullException(nameof(fileTransactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Ensure directories exist
            Directory.CreateDirectory(_restorePointsDirectory);
            Directory.CreateDirectory(_snapshotsDirectory);
        }
        
        /// <inheritdoc />
        public async Task<RestorePoint> CreateRestorePointAsync(string name, string description, Guid? transactionId = null)
        {
            await _lock.WaitAsync();
            try
            {
                var restorePoint = new RestorePoint
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    CreationTime = DateTime.Now,
                    TransactionId = transactionId,
                    Status = RestorePointStatus.Creating
                };
                
                // Create snapshot if associated with a transaction
                if (transactionId.HasValue)
                {
                    try
                    {
                        restorePoint.SnapshotPath = await _fileTransactionService.CreateStateSnapshotAsync(transactionId.Value);
                        _logger.LogInformation("Created snapshot for restore point {RestorePointId}", restorePoint.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create snapshot for restore point {RestorePointId}", restorePoint.Id);
                        restorePoint.Status = RestorePointStatus.Partial;
                    }
                }
                
                // Save restore point to disk
                await SaveRestorePointToDiskAsync(restorePoint);
                
                // Update status if everything was successful
                if (restorePoint.Status == RestorePointStatus.Creating)
                {
                    restorePoint.Status = RestorePointStatus.Valid;
                    await SaveRestorePointToDiskAsync(restorePoint);
                }
                
                _logger.LogInformation("Created restore point: {RestorePointId} - {RestorePointName}", restorePoint.Id, restorePoint.Name);
                return restorePoint;
            }
            finally
            {
                _lock.Release();
            }
        }
        
        /// <inheritdoc />
        public async Task<RestorePoint?> GetRestorePointAsync(Guid restorePointId)
        {
            string filePath = GetRestorePointFilePath(restorePointId);
            if (!File.Exists(filePath))
                return null;
                
            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<RestorePoint>(json);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<RestorePoint>> GetRestorePointsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            List<RestorePoint> result = new List<RestorePoint>();
            
            foreach (string filePath in Directory.GetFiles(_restorePointsDirectory, "*.json"))
            {
                string json = await File.ReadAllTextAsync(filePath);
                var restorePoint = JsonSerializer.Deserialize<RestorePoint>(json);
                
                if (restorePoint != null && restorePoint.CreationTime >= startDate && restorePoint.CreationTime <= endDate)
                {
                    result.Add(restorePoint);
                }
            }
            
            return result.OrderByDescending(rp => rp.CreationTime);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<RestorePoint>> GetRestorePointsByTransactionAsync(Guid transactionId)
        {
            List<RestorePoint> result = new List<RestorePoint>();
            
            foreach (string filePath in Directory.GetFiles(_restorePointsDirectory, "*.json"))
            {
                string json = await File.ReadAllTextAsync(filePath);
                var restorePoint = JsonSerializer.Deserialize<RestorePoint>(json);
                
                if (restorePoint != null && restorePoint.TransactionId == transactionId)
                {
                    result.Add(restorePoint);
                }
            }
            
            return result.OrderByDescending(rp => rp.CreationTime);
        }
        
        /// <inheritdoc />
        public async Task<RestoreResult> RestoreFromPointAsync(
            Guid restorePointId,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            // Get restore point
            var restorePoint = await GetRestorePointAsync(restorePointId);
            if (restorePoint == null)
                throw new ArgumentException($"Restore point with ID {restorePointId} not found.", nameof(restorePointId));
                
            if (restorePoint.Status != RestorePointStatus.Valid)
                throw new InvalidOperationException($"Cannot restore from an invalid restore point (Status: {restorePoint.Status}).");
                
            // Create result object
            var result = new RestoreResult
            {
                RestorePointId = restorePointId,
                Success = false
            };
            
            var startTime = DateTime.Now;
            
            try
            {
                // If restore point has a transaction ID, restore using that transaction's snapshot
                if (restorePoint.TransactionId.HasValue && !string.IsNullOrEmpty(restorePoint.SnapshotPath))
                {
                    // Execute restore using the snapshot
                    var transactionId = await _fileTransactionService.RestoreFromSnapshotAsync(
                        restorePoint.SnapshotPath,
                        cancellationToken,
                        progress);
                        
                    // Get restored transaction to update result information
                    var transaction = await _fileTransactionService.GetTransactionAsync(transactionId);
                    if (transaction != null)
                    {
                        result.FilesRestored = transaction.Operations.Count(o => o.Status == OperationStatus.Completed);
                        result.FilesFailed = transaction.Operations.Count(o => o.Status == OperationStatus.Failed);
                        result.Success = transaction.Status == TransactionStatus.Completed;
                        
                        // Capture details of restored files
                        foreach (var op in transaction.Operations.Where(o => o.Status == OperationStatus.Completed))
                        {
                            result.RestoredFiles.Add(op.DestinationPath);
                        }
                        
                        // Capture details of failed files
                        foreach (var op in transaction.Operations.Where(o => o.Status == OperationStatus.Failed))
                        {
                            result.FailedFiles[op.DestinationPath] = op.ErrorMessage ?? "Unknown error";
                        }
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Restore point does not have an associated transaction or snapshot.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring from point {RestorePointId}", restorePointId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            
            result.Duration = DateTime.Now - startTime;
            return result;
        }
        
        /// <inheritdoc />
        public async Task<RestorePreview> CreateRestorePreviewAsync(Guid restorePointId)
        {
            // Get restore point
            var restorePoint = await GetRestorePointAsync(restorePointId);
            if (restorePoint == null)
                throw new ArgumentException($"Restore point with ID {restorePointId} not found.", nameof(restorePointId));
                
            // Create preview
            var preview = new RestorePreview
            {
                RestorePointId = restorePointId
            };
            
            // Read snapshot data
            if (restorePoint.TransactionId.HasValue && !string.IsNullOrEmpty(restorePoint.SnapshotPath))
            {
                if (File.Exists(restorePoint.SnapshotPath))
                {
                    string json = await File.ReadAllTextAsync(restorePoint.SnapshotPath);
                    var snapshot = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    // In a real implementation, this would parse the snapshot and build the preview
                    // For simplicity, we'll create a mock preview
                    preview.TotalSize = 1024 * 1024 * 100; // 100 MB
                    preview.EstimatedTimeInSeconds = 30;   // 30 seconds
                    
                    // Add sample files
                    for (int i = 0; i < 10; i++)
                    {
                        var file = new RestoreFilePreview
                        {
                            SourcePath = $"C:\\Backup\\file{i}.txt",
                            TargetPath = $"C:\\Restored\\file{i}.txt",
                            Size = 1024 * 1024,  // 1 MB
                            SourceExists = true,
                            TargetExists = i % 3 == 0,  // Every third file exists at target
                            Operation = i % 3 == 0 ? "Overwrite" : "Restore"
                        };
                        
                        preview.FilesToRestore.Add(file);
                        
                        // Add conflict for files that exist at target
                        if (file.TargetExists)
                        {
                            var conflict = new RestoreConflict
                            {
                                FilePath = file.SourcePath,
                                ConflictType = "FileExists",
                                Description = "File already exists at target location.",
                                ResolutionOptions = new List<string> { "Overwrite", "Skip", "Rename" },
                                RecommendedResolution = "Overwrite"
                            };
                            
                            preview.PotentialConflicts.Add(conflict);
                        }
                    }
                }
            }
            
            return preview;
        }
        
        /// <inheritdoc />
        public async Task<VerificationResult> VerifyRestorePointAsync(Guid restorePointId)
        {
            // Get restore point
            var restorePoint = await GetRestorePointAsync(restorePointId);
            if (restorePoint == null)
                throw new ArgumentException($"Restore point with ID {restorePointId} not found.", nameof(restorePointId));
                
            var result = new VerificationResult
            {
                RestorePointId = restorePointId,
                Success = true,
                SnapshotIntact = true,
                MetadataValid = true
            };
            
            // Verify snapshot exists and is valid
            if (!string.IsNullOrEmpty(restorePoint.SnapshotPath))
            {
                if (File.Exists(restorePoint.SnapshotPath))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(restorePoint.SnapshotPath);
                        var snapshot = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                        
                        // In a real implementation, this would verify all files listed in the snapshot
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error verifying snapshot for restore point {RestorePointId}", restorePointId);
                        result.SnapshotIntact = false;
                        result.Success = false;
                    }
                }
                else
                {
                    result.SnapshotIntact = false;
                    result.Success = false;
                    result.Issues["Snapshot"] = $"Snapshot file not found: {restorePoint.SnapshotPath}";
                }
            }
            
            return result;
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteRestorePointAsync(Guid restorePointId)
        {
            await _lock.WaitAsync();
            try
            {
                // Get restore point
                var restorePoint = await GetRestorePointAsync(restorePointId);
                if (restorePoint == null)
                    return false;
                    
                // Delete snapshot if it exists
                if (!string.IsNullOrEmpty(restorePoint.SnapshotPath) && File.Exists(restorePoint.SnapshotPath))
                {
                    File.Delete(restorePoint.SnapshotPath);
                }
                
                // Delete restore point file
                string filePath = GetRestorePointFilePath(restorePointId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting restore point {RestorePointId}", restorePointId);
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }
        
        /// <inheritdoc />
        public async Task<string> ExportRestorePointAsync(Guid restorePointId, string exportPath, bool includeFileBackups = true)
        {
            // Get restore point
            var restorePoint = await GetRestorePointAsync(restorePointId);
            if (restorePoint == null)
                throw new ArgumentException($"Restore point with ID {restorePointId} not found.", nameof(restorePointId));
                
            // Create export directory
            Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
            
            // Serialize restore point
            string json = JsonSerializer.Serialize(restorePoint);
            await File.WriteAllTextAsync(exportPath, json);
            
            return exportPath;
        }
        
        /// <inheritdoc />
        public async Task<RestorePoint> ImportRestorePointAsync(string importPath)
        {
            if (!File.Exists(importPath))
                throw new ArgumentException($"Import file not found: {importPath}", nameof(importPath));
                
            string json = await File.ReadAllTextAsync(importPath);
            var restorePoint = JsonSerializer.Deserialize<RestorePoint>(json);
            
            if (restorePoint == null)
                throw new InvalidOperationException("Failed to deserialize restore point from import file.");
                
            // Save to disk
            await SaveRestorePointToDiskAsync(restorePoint);
            
            return restorePoint;
        }
        
        /// <inheritdoc />
        public async Task<int> GetRestorePointCountAsync()
        {
            return Directory.GetFiles(_restorePointsDirectory, "*.json").Length;
        }
        
        /// <inheritdoc />
        public async Task<long> GetRestorePointStorageSizeAsync()
        {
            long totalSize = 0;
            
            // Get size of restore point files
            foreach (string filePath in Directory.GetFiles(_restorePointsDirectory, "*.json"))
            {
                totalSize += new FileInfo(filePath).Length;
            }
            
            // Get size of snapshot files
            foreach (string filePath in Directory.GetFiles(_snapshotsDirectory))
            {
                totalSize += new FileInfo(filePath).Length;
            }
            
            return totalSize;
        }
        
        /// <inheritdoc />
        public async Task<int> ApplyCleanupPolicyAsync(TimeSpan maxAge, int maxCount, long maxSize)
        {
            await _lock.WaitAsync();
            try
            {
                // Get all restore points
                var restorePoints = new List<RestorePoint>();
                foreach (string filePath in Directory.GetFiles(_restorePointsDirectory, "*.json"))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var restorePoint = JsonSerializer.Deserialize<RestorePoint>(json);
                    if (restorePoint != null)
                    {
                        restorePoints.Add(restorePoint);
                    }
                }
                
                // Sort by creation time (oldest first)
                restorePoints = restorePoints.OrderBy(rp => rp.CreationTime).ToList();
                
                // Apply age policy
                var now = DateTime.Now;
                var maxDate = now - maxAge;
                var toDelete = restorePoints.Where(rp => rp.CreationTime < maxDate).ToList();
                
                // Apply count policy
                if (restorePoints.Count - toDelete.Count > maxCount)
                {
                    var remainingPoints = restorePoints.Except(toDelete).OrderBy(rp => rp.CreationTime).ToList();
                    var countToDelete = remainingPoints.Count - maxCount;
                    
                    if (countToDelete > 0)
                    {
                        toDelete.AddRange(remainingPoints.Take(countToDelete));
                    }
                }
                
                // Delete restore points
                int deleted = 0;
                foreach (var rp in toDelete)
                {
                    if (await DeleteRestorePointAsync(rp.Id))
                    {
                        deleted++;
                    }
                }
                
                return deleted;
            }
            finally
            {
                _lock.Release();
            }
        }
        
        /// <inheritdoc />
        public async Task<RestoreResult> RestoreSelectedFilesAsync(
            Guid restorePointId,
            IEnumerable<string> selectedFilePaths,
            RestoreOptions options,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            // Get restore point
            var restorePoint = await GetRestorePointAsync(restorePointId);
            if (restorePoint == null)
                throw new ArgumentException($"Restore point with ID {restorePointId} not found.", nameof(restorePointId));
                
            if (restorePoint.Status != RestorePointStatus.Valid)
                throw new InvalidOperationException($"Cannot restore from an invalid restore point (Status: {restorePoint.Status}).");
                
            // Create result object
            var result = new RestoreResult
            {
                RestorePointId = restorePointId,
                Success = false
            };
            
            var startTime = DateTime.Now;
            var filesToRestore = selectedFilePaths.ToList();
            
            try
            {
                // Report progress
                progress?.Report((0, $"Starting selective restore of {filesToRestore.Count} files..."));
                
                // Create a list to track restored files
                var restoredFiles = new ConcurrentBag<string>();
                var failedFiles = new ConcurrentDictionary<string, string>();
                
                // Create a transaction for this restore operation
                var transaction = await _fileTransactionService.CreateTransactionAsync(
                    $"Restore from {restorePoint.Name}",
                    $"Selective restore of {filesToRestore.Count} files from restore point {restorePointId}",
                    TransactionType.Restore);
                
                // Process files in parallel or sequentially based on options
                if (options.UseParallelProcessing && filesToRestore.Count > 1)
                {
                    int maxDegree = Math.Min(options.MaxDegreeOfParallelism, Environment.ProcessorCount);
                    var parallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = maxDegree,
                        CancellationToken = cancellationToken
                    };
                    
                    int processedCount = 0;
                    int totalCount = filesToRestore.Count;
                    
                    await Parallel.ForEachAsync(filesToRestore, parallelOptions, async (filePath, ct) =>
                    {
                        try
                        {
                            // Determine target path (would come from snapshot in real implementation)
                            string targetPath = filePath.Replace("Backup", "Restored");
                            
                            // Check for conflicts
                            bool hasConflict = File.Exists(targetPath);
                            string resolution = options.OverwriteExisting ? "Overwrite" : "Skip";
                            
                            // Apply conflict resolution if available
                            if (hasConflict && options.ConflictResolutions.TryGetValue(filePath, out string customResolution))
                            {
                                resolution = customResolution;
                            }
                            
                            // Process based on resolution
                            if (resolution == "Skip")
                            {
                                // Skip this file
                                return;
                            }
                            else if (resolution == "Rename")
                            {
                                // Generate a new name
                                targetPath = GetUniqueFilePath(targetPath);
                            }
                            
                            // Ensure target directory exists
                            if (options.CreateMissingDirectories)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            }
                            
                            // Restore the file
                            File.Copy(filePath, targetPath, true);
                            
                            // Set original timestamps if requested
                            if (options.PreserveTimestamps)
                            {
                                var sourceInfo = new FileInfo(filePath);
                                File.SetCreationTime(targetPath, sourceInfo.CreationTime);
                                File.SetLastWriteTime(targetPath, sourceInfo.LastWriteTime);
                                File.SetLastAccessTime(targetPath, sourceInfo.LastAccessTime);
                            }
                            
                            // Track success
                            restoredFiles.Add(targetPath);
                            
                            // Report individual progress
                            int newCount = Interlocked.Increment(ref processedCount);
                            int progressPercent = (int)((double)newCount / totalCount * 100);
                            progress?.Report((progressPercent, $"Restored {newCount} of {totalCount} files"));
                        }
                        catch (Exception ex)
                        {
                            // Track failure
                            failedFiles[filePath] = ex.Message;
                        }
                    });
                }
                else
                {
                    // Sequential processing
                    for (int i = 0; i < filesToRestore.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                            
                        string filePath = filesToRestore[i];
                        
                        try
                        {
                            // Determine target path (would come from snapshot in real implementation)
                            string targetPath = filePath.Replace("Backup", "Restored");
                            
                            // Check for conflicts
                            bool hasConflict = File.Exists(targetPath);
                            string resolution = options.OverwriteExisting ? "Overwrite" : "Skip";
                            
                            // Apply conflict resolution if available
                            if (hasConflict && options.ConflictResolutions.TryGetValue(filePath, out string customResolution))
                            {
                                resolution = customResolution;
                            }
                            
                            // Process based on resolution
                            if (resolution == "Skip")
                            {
                                // Skip this file
                                continue;
                            }
                            else if (resolution == "Rename")
                            {
                                // Generate a new name
                                targetPath = GetUniqueFilePath(targetPath);
                            }
                            
                            // Ensure target directory exists
                            if (options.CreateMissingDirectories)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            }
                            
                            // Restore the file
                            File.Copy(filePath, targetPath, true);
                            
                            // Set original timestamps if requested
                            if (options.PreserveTimestamps)
                            {
                                var sourceInfo = new FileInfo(filePath);
                                File.SetCreationTime(targetPath, sourceInfo.CreationTime);
                                File.SetLastWriteTime(targetPath, sourceInfo.LastWriteTime);
                                File.SetLastAccessTime(targetPath, sourceInfo.LastAccessTime);
                            }
                            
                            // Track success
                            restoredFiles.Add(targetPath);
                            
                            // Report progress
                            int progressPercent = (int)((double)(i + 1) / filesToRestore.Count * 100);
                            progress?.Report((progressPercent, $"Restored {i + 1} of {filesToRestore.Count} files"));
                        }
                        catch (Exception ex)
                        {
                            // Track failure
                            failedFiles[filePath] = ex.Message;
                        }
                    }
                }
                
                // Update result
                result.Success = restoredFiles.Count > 0 || filesToRestore.Count == 0;
                result.FilesRestored = restoredFiles.Count;
                result.FilesFailed = failedFiles.Count;
                result.RestoredFiles = restoredFiles.ToList();
                result.FailedFiles = new Dictionary<string, string>(failedFiles);
                
                // Report final progress
                progress?.Report((100, $"Restored {result.FilesRestored} files, {result.FilesFailed} failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during selective restore from point {RestorePointId}", restorePointId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            
            result.Duration = DateTime.Now - startTime;
            return result;
        }
        
        /// <summary>
        /// Generates a unique file path by appending a number if the file already exists.
        /// </summary>
        /// <param name="filePath">Original file path.</param>
        /// <returns>Unique file path.</returns>
        private string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;
                
            string directory = Path.GetDirectoryName(filePath);
            string filename = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            
            int counter = 1;
            string newPath;
            
            do
            {
                newPath = Path.Combine(directory, $"{filename} ({counter}){extension}");
                counter++;
            }
            while (File.Exists(newPath));
            
            return newPath;
        }
        
        /// <summary>
        /// Gets the file path for a restore point.
        /// </summary>
        /// <param name="restorePointId">ID of the restore point.</param>
        /// <returns>File path.</returns>
        private string GetRestorePointFilePath(Guid restorePointId)
        {
            return Path.Combine(_restorePointsDirectory, $"{restorePointId}.json");
        }
        
        /// <summary>
        /// Saves a restore point to disk.
        /// </summary>
        /// <param name="restorePoint">Restore point to save.</param>
        private async Task SaveRestorePointToDiskAsync(RestorePoint restorePoint)
        {
            string filePath = GetRestorePointFilePath(restorePoint.Id);
            string json = JsonSerializer.Serialize(restorePoint);
            await File.WriteAllTextAsync(filePath, json);
        }
    }
} 