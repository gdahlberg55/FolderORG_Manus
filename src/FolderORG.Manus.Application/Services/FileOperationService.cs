using FolderORG.Manus.Core.Models;
using System.IO;

namespace FolderORG.Manus.Application.Services
{
    /// <summary>
    /// Service for handling file operations like moving, copying, and validation.
    /// </summary>
    public class FileOperationService
    {
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