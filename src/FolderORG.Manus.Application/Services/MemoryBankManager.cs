using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using System.IO;

namespace FolderORG.Manus.Application.Services
{
    /// <summary>
    /// Manager for Memory Bank operations, connecting file operations with Memory Bank storage.
    /// </summary>
    public class MemoryBankManager
    {
        private readonly IMemoryBankService _memoryBankService;
        private readonly FileOperationService _fileOperationService;

        /// <summary>
        /// Initializes a new instance of the MemoryBankManager class.
        /// </summary>
        /// <param name="memoryBankService">The Memory Bank service for storing and retrieving entries.</param>
        public MemoryBankManager(IMemoryBankService memoryBankService)
        {
            _memoryBankService = memoryBankService;
            _fileOperationService = new FileOperationService();
        }

        /// <summary>
        /// Organizes a file based on its classification result and adds an entry to the Memory Bank.
        /// </summary>
        /// <param name="classificationResult">The classification result of the file.</param>
        /// <param name="targetRootFolder">The root folder where the file will be organized.</param>
        /// <param name="operationType">The type of file operation to perform.</param>
        /// <param name="createMissingFolders">Whether to create missing target folders.</param>
        /// <param name="organizationMethod">The method used for organization (e.g., "ByExtension", "ByDate").</param>
        /// <returns>The result of the organization operation.</returns>
        public async Task<FileOperationService.FileOperationResult> OrganizeFileAsync(
            ClassificationResult classificationResult,
            string targetRootFolder,
            FileOperationService.FileOperationType operationType,
            bool createMissingFolders = true,
            string organizationMethod = "Default")
        {
            // Perform the file operation
            var operationResult = await _fileOperationService.ProcessFileAsync(
                classificationResult, 
                targetRootFolder, 
                operationType, 
                createMissingFolders);

            if (operationResult.Success)
            {
                // Create and save a Memory Bank entry
                var entry = new MemoryBankEntry
                {
                    OriginalPath = operationResult.SourcePath,
                    CurrentPath = operationResult.DestinationPath,
                    FileName = Path.GetFileName(operationResult.DestinationPath),
                    FileSize = classificationResult.FileMetadata.Size,
                    OrganizedDate = DateTime.Now,
                    OperationType = operationType.ToString(),
                    Category = classificationResult.PrimaryCategory,
                    SubCategory = classificationResult.SubCategory,
                    ClassificationMethod = organizationMethod,
                    Tags = classificationResult.Tags,
                    StillExists = true,
                    LastVerifiedDate = DateTime.Now
                };

                // Convert classification attributes to string-based attributes for storage
                foreach (var attr in classificationResult.ClassificationAttributes)
                {
                    entry.Attributes[attr.Key] = attr.Value.ToString();
                }

                await _memoryBankService.AddEntryAsync(entry);
            }

            return operationResult;
        }

        /// <summary>
        /// Organizes multiple files based on their classification results and adds entries to the Memory Bank.
        /// </summary>
        /// <param name="classificationResults">The classification results of the files.</param>
        /// <param name="targetRootFolder">The root folder where files will be organized.</param>
        /// <param name="operationType">The type of file operation to perform.</param>
        /// <param name="createMissingFolders">Whether to create missing target folders.</param>
        /// <param name="organizationMethod">The method used for organization (e.g., "ByExtension", "ByDate").</param>
        /// <param name="progressCallback">Optional callback for reporting progress (0-100).</param>
        /// <returns>The results of the organization operations.</returns>
        public async Task<IEnumerable<FileOperationService.FileOperationResult>> OrganizeFilesAsync(
            IEnumerable<ClassificationResult> classificationResults,
            string targetRootFolder,
            FileOperationService.FileOperationType operationType,
            bool createMissingFolders = true,
            string organizationMethod = "Default",
            Action<int>? progressCallback = null)
        {
            var results = new List<FileOperationService.FileOperationResult>();
            var resultsList = classificationResults.ToList();
            int totalFiles = resultsList.Count;
            int processedCount = 0;

            foreach (var result in resultsList)
            {
                var operationResult = await OrganizeFileAsync(
                    result, 
                    targetRootFolder, 
                    operationType, 
                    createMissingFolders, 
                    organizationMethod);
                
                results.Add(operationResult);
                
                // Update progress if callback is provided
                processedCount++;
                if (progressCallback != null)
                {
                    int progressPercentage = (int)((double)processedCount / totalFiles * 100);
                    progressCallback(progressPercentage);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets statistics for the Memory Bank.
        /// </summary>
        /// <returns>A dictionary containing the statistics.</returns>
        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            return await _memoryBankService.GetStatisticsAsync();
        }

        /// <summary>
        /// Searches for entries in the Memory Bank.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        /// <returns>A collection of entries matching the search criteria.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> SearchMemoryBankAsync(string searchText)
        {
            return await _memoryBankService.SearchEntriesAsync(searchText);
        }

        /// <summary>
        /// Gets all entries in the Memory Bank.
        /// </summary>
        /// <returns>A collection of all entries.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> GetAllEntriesAsync()
        {
            return await _memoryBankService.GetAllEntriesAsync();
        }

        /// <summary>
        /// Verifies that the entries in the Memory Bank still exist at their specified locations.
        /// </summary>
        /// <returns>A collection of entries that no longer exist at their specified locations.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> VerifyEntriesExistAsync()
        {
            return await _memoryBankService.VerifyEntriesExistAsync();
        }

        /// <summary>
        /// Exports the Memory Bank entries to a file.
        /// </summary>
        /// <param name="filePath">The path where the file will be saved.</param>
        /// <returns>True if the export was successful; otherwise, false.</returns>
        public async Task<bool> ExportMemoryBankAsync(string filePath)
        {
            return await _memoryBankService.ExportToFileAsync(filePath);
        }

        /// <summary>
        /// Imports Memory Bank entries from a file.
        /// </summary>
        /// <param name="filePath">The path of the file to import.</param>
        /// <returns>The number of entries imported.</returns>
        public async Task<int> ImportMemoryBankAsync(string filePath)
        {
            return await _memoryBankService.ImportFromFileAsync(filePath);
        }
    }
} 