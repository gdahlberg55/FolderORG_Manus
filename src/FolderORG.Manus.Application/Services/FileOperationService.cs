using FolderORG.Manus.Core.Models;
using FolderORG.Manus.Core.Interfaces;
using System.IO;
using System.Threading;

namespace FolderORG.Manus.Application.Services
{
    /// <summary>
    /// Service for handling file operations like moving, copying, and validation.
    /// </summary>
    public class FileOperationService
    {
        private readonly IFileTransactionService? _fileTransactionService;

        /// <summary>
        /// Initializes a new instance of the FileOperationService.
        /// </summary>
        /// <param name="fileTransactionService">Optional transaction service for transactional operations.</param>
        public FileOperationService(IFileTransactionService? fileTransactionService = null)
        {
            _fileTransactionService = fileTransactionService;
        }

        /// <summary>
        /// Result of a file operation.
        /// </summary>
        public class FileOperationResult
        {
            /// <summary>
            /// Indicates whether the operation was successful.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// The source file path.
            /// </summary>
            public string SourcePath { get; set; } = string.Empty;

            /// <summary>
            /// The destination file path.
            /// </summary>
            public string DestinationPath { get; set; } = string.Empty;

            /// <summary>
            /// Error message if the operation failed.
            /// </summary>
            public string? ErrorMessage { get; set; }

            /// <summary>
            /// The type of operation performed.
            /// </summary>
            public FileOperationType OperationType { get; set; }

            /// <summary>
            /// Associated transaction ID if the operation was part of a transaction.
            /// </summary>
            public Guid? TransactionId { get; set; }
        }

        /// <summary>
        /// Type of file operation.
        /// </summary>
        public enum FileOperationType
        {
            /// <summary>
            /// Copy operation.
            /// </summary>
            Copy,

            /// <summary>
            /// Move operation.
            /// </summary>
            Move,

            /// <summary>
            /// Delete operation.
            /// </summary>
            Delete
        }

        /// <summary>
        /// Moves or copies a file based on classification results.
        /// </summary>
        /// <param name="result">The classification result containing file metadata.</param>
        /// <param name="targetRootFolder">The root folder where files will be organized.</param>
        /// <param name="operationType">The type of operation to perform (Move or Copy).</param>
        /// <param name="createMissingFolders">Whether to create missing folders in the target path.</param>
        /// <returns>A result object containing information about the operation.</returns>
        public async Task<FileOperationResult> ProcessFileAsync(ClassificationResult result, string targetRootFolder, 
            FileOperationType operationType, bool createMissingFolders = true)
        {
            if (result == null || result.FileMetadata == null)
                throw new ArgumentNullException(nameof(result));

            if (string.IsNullOrEmpty(targetRootFolder))
                throw new ArgumentException("Target root folder cannot be null or empty.", nameof(targetRootFolder));

            var operationResult = new FileOperationResult
            {
                SourcePath = result.FileMetadata.FullPath,
                OperationType = operationType
            };

            try
            {
                // Determine the target folder
                string targetFolder = targetRootFolder;
                
                if (!string.IsNullOrEmpty(result.SuggestedPath))
                {
                    targetFolder = Path.Combine(targetRootFolder, result.SuggestedPath);
                }

                // Create target directory if it doesn't exist
                if (!Directory.Exists(targetFolder))
                {
                    if (createMissingFolders)
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
                    else
                    {
                        operationResult.Success = false;
                        operationResult.ErrorMessage = $"Target directory does not exist: {targetFolder}";
                        return operationResult;
                    }
                }

                // Generate a unique destination path to avoid overwriting
                string fileName = Path.GetFileName(result.FileMetadata.FullPath);
                string destinationPath = Path.Combine(targetFolder, fileName);

                if (File.Exists(destinationPath))
                {
                    string extension = Path.GetExtension(fileName);
                    string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    destinationPath = Path.Combine(targetFolder, $"{nameWithoutExtension}_{timestamp}{extension}");
                }

                operationResult.DestinationPath = destinationPath;

                // Perform the file operation
                if (operationType == FileOperationType.Move)
                {
                    // Run the move operation
                    File.Move(result.FileMetadata.FullPath, destinationPath);
                }
                else if (operationType == FileOperationType.Copy)
                {
                    // Run the copy operation
                    File.Copy(result.FileMetadata.FullPath, destinationPath);
                }
                else if (operationType == FileOperationType.Delete)
                {
                    // Run the delete operation
                    File.Delete(result.FileMetadata.FullPath);
                }

                operationResult.Success = true;
                return operationResult;
            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.ErrorMessage = ex.Message;
                return operationResult;
            }
        }

        /// <summary>
        /// Batch processes multiple files based on classification results.
        /// </summary>
        /// <param name="results">The classification results containing file metadata.</param>
        /// <param name="targetRootFolder">The root folder where files will be organized.</param>
        /// <param name="operationType">The type of operation to perform (Move or Copy).</param>
        /// <param name="createMissingFolders">Whether to create missing folders in the target path.</param>
        /// <param name="progressCallback">Optional callback for reporting progress (0-100).</param>
        /// <returns>A collection of results for each file operation.</returns>
        public async Task<IEnumerable<FileOperationResult>> BatchProcessFilesAsync(
            IEnumerable<ClassificationResult> results, 
            string targetRootFolder, 
            FileOperationType operationType, 
            bool createMissingFolders = true, 
            Action<int>? progressCallback = null)
        {
            var operationResults = new List<FileOperationResult>();
            var resultsList = results.ToList();
            int totalFiles = resultsList.Count;
            int processedCount = 0;

            foreach (var result in resultsList)
            {
                var operationResult = await ProcessFileAsync(result, targetRootFolder, operationType, createMissingFolders);
                operationResults.Add(operationResult);
                
                // Update progress if callback is provided
                processedCount++;
                if (progressCallback != null)
                {
                    int progressPercentage = (int)((double)processedCount / totalFiles * 100);
                    progressCallback(progressPercentage);
                }
            }

            return operationResults;
        }

        /// <summary>
        /// Batch processes multiple files with transaction support for rollback capability.
        /// </summary>
        /// <param name="results">The classification results containing file metadata.</param>
        /// <param name="targetRootFolder">The root folder where files will be organized.</param>
        /// <param name="operationType">The type of operation to perform (Move or Copy).</param>
        /// <param name="transactionName">Name of the transaction for identification.</param>
        /// <param name="createMissingFolders">Whether to create missing folders in the target path.</param>
        /// <param name="createBackups">Whether to create backups for potential rollback.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progress">Callback for reporting progress (0-100).</param>
        /// <returns>A collection of results and the transaction ID for potential rollback.</returns>
        public async Task<(IEnumerable<FileOperationResult> Results, Guid TransactionId)> BatchProcessFilesWithTransactionAsync(
            IEnumerable<ClassificationResult> results,
            string targetRootFolder,
            FileOperationType operationType,
            string transactionName,
            bool createMissingFolders = true,
            bool createBackups = true,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            if (_fileTransactionService == null)
                throw new InvalidOperationException("Transaction service is not available. Please initialize the FileOperationService with an IFileTransactionService implementation.");

            var resultsList = results.ToList();
            int totalFiles = resultsList.Count;
            
            // Create a new transaction
            var transaction = await _fileTransactionService.CreateTransactionAsync(
                transactionName,
                $"Batch {operationType} operation for {totalFiles} files to {targetRootFolder}",
                TransactionType.Manual);
            
            var operationResults = new List<FileOperationResult>();
            int processedCount = 0;
            
            try
            {
                // Plan all operations and add them to the transaction
                foreach (var result in resultsList)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    
                    // Determine the target folder
                    string targetFolder = targetRootFolder;
                    
                    if (!string.IsNullOrEmpty(result.SuggestedPath))
                    {
                        targetFolder = Path.Combine(targetRootFolder, result.SuggestedPath);
                    }
                    
                    // Ensure target directory exists or can be created
                    if (!Directory.Exists(targetFolder))
                    {
                        if (!createMissingFolders)
                        {
                            // Skip this file and continue with others
                            var skipResult = new FileOperationResult
                            {
                                SourcePath = result.FileMetadata.FullPath,
                                OperationType = operationType,
                                Success = false,
                                ErrorMessage = $"Target directory does not exist: {targetFolder}",
                                TransactionId = transaction.Id
                            };
                            operationResults.Add(skipResult);
                            continue;
                        }
                    }
                    
                    // Generate a unique destination path
                    string fileName = Path.GetFileName(result.FileMetadata.FullPath);
                    string destinationPath = Path.Combine(targetFolder, fileName);
                    
                    if (File.Exists(destinationPath))
                    {
                        string extension = Path.GetExtension(fileName);
                        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        destinationPath = Path.Combine(targetFolder, $"{nameWithoutExtension}_{timestamp}{extension}");
                    }
                    
                    // Add the operation to the transaction
                    await _fileTransactionService.AddOperationAsync(
                        transaction.Id,
                        result.FileMetadata.FullPath,
                        destinationPath,
                        operationType.ToString());
                    
                    // Update progress
                    processedCount++;
                    progress?.Report((
                        (int)((double)processedCount / totalFiles * 50), // Use first 50% for planning
                        $"Planning operation for {fileName}"
                    ));
                }
                
                // Execute the transaction
                if (!cancellationToken.IsCancellationRequested)
                {
                    var executedTransaction = await _fileTransactionService.ExecuteTransactionAsync(
                        transaction.Id,
                        createBackups,
                        cancellationToken,
                        new Progress<(int ProgressPercentage, string StatusMessage)>(progressUpdate =>
                        {
                            // Transform progress from 0-100 to 50-100 (second half of operation)
                            int adjustedProgress = 50 + (progressUpdate.ProgressPercentage / 2);
                            progress?.Report((adjustedProgress, progressUpdate.StatusMessage));
                        }));
                    
                    // Map transaction operations to operation results
                    foreach (var operation in executedTransaction.Operations)
                    {
                        var operationResult = new FileOperationResult
                        {
                            SourcePath = operation.SourcePath,
                            DestinationPath = operation.DestinationPath,
                            OperationType = Enum.Parse<FileOperationType>(operation.OperationType),
                            Success = operation.Status == OperationStatus.Completed,
                            ErrorMessage = operation.Status != OperationStatus.Completed ? operation.ErrorMessage : null,
                            TransactionId = executedTransaction.Id
                        };
                        
                        operationResults.Add(operationResult);
                    }
                    
                    return (operationResults, executedTransaction.Id);
                }
                else
                {
                    // Cancellation was requested, return partial results
                    return (operationResults, transaction.Id);
                }
            }
            catch (Exception ex)
            {
                // Handle transaction failure
                // Add failure information to the results
                operationResults.Add(new FileOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Transaction failed: {ex.Message}",
                    TransactionId = transaction.Id
                });
                
                return (operationResults, transaction.Id);
            }
        }
        
        /// <summary>
        /// Rolls back a previously executed transaction.
        /// </summary>
        /// <param name="transactionId">ID of the transaction to roll back.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <param name="progress">Callback for reporting progress (0-100).</param>
        /// <returns>True if rollback was successful; otherwise, false.</returns>
        public async Task<bool> RollbackTransactionAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default,
            IProgress<(int ProgressPercentage, string StatusMessage)>? progress = null)
        {
            if (_fileTransactionService == null)
                throw new InvalidOperationException("Transaction service is not available. Please initialize the FileOperationService with an IFileTransactionService implementation.");
            
            return await _fileTransactionService.RollbackTransactionAsync(transactionId, cancellationToken, progress);
        }

        /// <summary>
        /// Checks if a file exists and is accessible.
        /// </summary>
        /// <param name="filePath">The path of the file to check.</param>
        /// <returns>True if the file exists and is accessible; otherwise, false.</returns>
        public bool FileExists(string filePath)
        {
            try
            {
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates whether a directory path is valid and accessible.
        /// </summary>
        /// <param name="directoryPath">The directory path to validate.</param>
        /// <param name="createIfMissing">Whether to create the directory if it doesn't exist.</param>
        /// <returns>True if the directory is valid and accessible; otherwise, false.</returns>
        public bool ValidateDirectory(string directoryPath, bool createIfMissing = false)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    // Test write access
                    string testFile = Path.Combine(directoryPath, $"test_{Guid.NewGuid()}.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    return true;
                }
                else if (createIfMissing)
                {
                    Directory.CreateDirectory(directoryPath);
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
} 