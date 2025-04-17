using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using System.IO;
using System.Text.Json;

namespace FolderORG.Manus.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the Memory Bank service using JSON file storage.
    /// </summary>
    public class JsonMemoryBankService : IMemoryBankService
    {
        private readonly string _storageFilePath;
        private List<MemoryBankEntry> _entries = new List<MemoryBankEntry>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the JsonMemoryBankService class.
        /// </summary>
        /// <param name="storageFilePath">The path to the JSON file for storing Memory Bank entries.</param>
        public JsonMemoryBankService(string storageFilePath)
        {
            _storageFilePath = storageFilePath;
        }

        /// <summary>
        /// Initializes the Memory Bank service by loading entries from the JSON file.
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _semaphore.WaitAsync();
            try
            {
                if (_isInitialized)
                    return;

                if (File.Exists(_storageFilePath))
                {
                    string json = await File.ReadAllTextAsync(_storageFilePath);
                    var deserializedEntries = JsonSerializer.Deserialize<List<MemoryBankEntry>>(json);
                    if (deserializedEntries != null)
                        _entries = deserializedEntries;
                }
                else
                {
                    // Create the directory if it doesn't exist
                    string directory = Path.GetDirectoryName(_storageFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // Create empty entries list and save it
                    _entries = new List<MemoryBankEntry>();
                    await SaveEntriesAsync();
                }

                _isInitialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Saves the Memory Bank entries to the JSON file.
        /// </summary>
        private async Task SaveEntriesAsync()
        {
            string json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_storageFilePath, json);
        }

        /// <summary>
        /// Adds a new entry to the Memory Bank.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        /// <returns>The added entry with its assigned ID.</returns>
        public async Task<MemoryBankEntry> AddEntryAsync(MemoryBankEntry entry)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                // Ensure the entry has a unique ID
                if (entry.Id == Guid.Empty)
                    entry.Id = Guid.NewGuid();

                _entries.Add(entry);
                await SaveEntriesAsync();
                return entry;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Updates an existing entry in the Memory Bank.
        /// </summary>
        /// <param name="entry">The entry to update.</param>
        /// <returns>True if the entry was successfully updated; otherwise, false.</returns>
        public async Task<bool> UpdateEntryAsync(MemoryBankEntry entry)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                int index = _entries.FindIndex(e => e.Id == entry.Id);
                if (index < 0)
                    return false;

                _entries[index] = entry;
                await SaveEntriesAsync();
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets an entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the entry to retrieve.</param>
        /// <returns>The entry if found; otherwise, null.</returns>
        public async Task<MemoryBankEntry?> GetEntryByIdAsync(Guid id)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                return _entries.FirstOrDefault(e => e.Id == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets entries by file name.
        /// </summary>
        /// <param name="fileName">The file name to search for.</param>
        /// <returns>A collection of entries matching the file name.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> GetEntriesByFileNameAsync(string fileName)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                return _entries
                    .Where(e => e.FileName.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets entries by category.
        /// </summary>
        /// <param name="category">The category to search for.</param>
        /// <returns>A collection of entries matching the category.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> GetEntriesByCategoryAsync(string category)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                return _entries
                    .Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets entries organized within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>A collection of entries organized within the specified date range.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                return _entries
                    .Where(e => e.OrganizedDate >= startDate && e.OrganizedDate <= endDate)
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets all entries in the Memory Bank.
        /// </summary>
        /// <returns>A collection of all entries.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> GetAllEntriesAsync()
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                return _entries.ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Searches for entries based on various criteria.
        /// </summary>
        /// <param name="searchText">The text to search for in file names, categories, etc.</param>
        /// <returns>A collection of entries matching the search criteria.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> SearchEntriesAsync(string searchText)
        {
            await InitializeAsync();

            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllEntriesAsync();

            await _semaphore.WaitAsync();
            try
            {
                searchText = searchText.ToLowerInvariant();
                return _entries
                    .Where(e => 
                        e.FileName.ToLowerInvariant().Contains(searchText) ||
                        e.Category.ToLowerInvariant().Contains(searchText) ||
                        e.SubCategory.ToLowerInvariant().Contains(searchText) ||
                        e.Tags.Any(t => t.ToLowerInvariant().Contains(searchText)) ||
                        e.OriginalPath.ToLowerInvariant().Contains(searchText) ||
                        e.CurrentPath.ToLowerInvariant().Contains(searchText) ||
                        e.ClassificationMethod.ToLowerInvariant().Contains(searchText)
                    )
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Deletes an entry from the Memory Bank.
        /// </summary>
        /// <param name="id">The ID of the entry to delete.</param>
        /// <returns>True if the entry was successfully deleted; otherwise, false.</returns>
        public async Task<bool> DeleteEntryAsync(Guid id)
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                int initialCount = _entries.Count;
                _entries.RemoveAll(e => e.Id == id);
                
                if (_entries.Count != initialCount)
                {
                    await SaveEntriesAsync();
                    return true;
                }
                
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Verifies that the entries in the Memory Bank still exist at their specified locations.
        /// </summary>
        /// <returns>A collection of entries that no longer exist at their specified locations.</returns>
        public async Task<IEnumerable<MemoryBankEntry>> VerifyEntriesExistAsync()
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                var modifiedEntries = new List<MemoryBankEntry>();
                bool changed = false;

                foreach (var entry in _entries)
                {
                    bool exists = File.Exists(entry.CurrentPath);
                    if (entry.StillExists != exists)
                    {
                        entry.StillExists = exists;
                        entry.LastVerifiedDate = DateTime.Now;
                        modifiedEntries.Add(entry);
                        changed = true;
                    }
                }

                if (changed)
                    await SaveEntriesAsync();

                return modifiedEntries;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets the Memory Bank statistics (e.g., total entries, entries by category).
        /// </summary>
        /// <returns>A dictionary containing various statistics.</returns>
        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            await InitializeAsync();

            await _semaphore.WaitAsync();
            try
            {
                var stats = new Dictionary<string, object>
                {
                    ["TotalEntries"] = _entries.Count,
                    ["ExistingEntries"] = _entries.Count(e => e.StillExists),
                    ["MissingEntries"] = _entries.Count(e => !e.StillExists),
                    ["EntriesByCategory"] = _entries
                        .GroupBy(e => e.Category)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .ToDictionary(x => x.Category, x => x.Count),
                    ["EntriesBySubCategory"] = _entries
                        .GroupBy(e => e.SubCategory)
                        .Select(g => new { SubCategory = g.Key, Count = g.Count() })
                        .ToDictionary(x => x.SubCategory, x => x.Count),
                    ["EntriesByOperationType"] = _entries
                        .GroupBy(e => e.OperationType)
                        .Select(g => new { OperationType = g.Key, Count = g.Count() })
                        .ToDictionary(x => x.OperationType, x => x.Count),
                    ["TotalSizeBytes"] = _entries.Sum(e => e.FileSize),
                    ["AverageSizeBytes"] = _entries.Count > 0 ? _entries.Average(e => e.FileSize) : 0,
                    ["OldestEntry"] = _entries.Count > 0 ? _entries.Min(e => e.OrganizedDate) : DateTime.MinValue,
                    ["NewestEntry"] = _entries.Count > 0 ? _entries.Max(e => e.OrganizedDate) : DateTime.MinValue
                };

                return stats;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Exports the Memory Bank entries to a file.
        /// </summary>
        /// <param name="filePath">The path where the file will be saved.</param>
        /// <returns>True if the export was successful; otherwise, false.</returns>
        public async Task<bool> ExportToFileAsync(string filePath)
        {
            await InitializeAsync();

            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    string json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    await File.WriteAllTextAsync(filePath, json);
                    return true;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Imports Memory Bank entries from a file.
        /// </summary>
        /// <param name="filePath">The path of the file to import.</param>
        /// <returns>The number of entries imported.</returns>
        public async Task<int> ImportFromFileAsync(string filePath)
        {
            await InitializeAsync();

            if (!File.Exists(filePath))
                return 0;

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var importedEntries = JsonSerializer.Deserialize<List<MemoryBankEntry>>(json);
                if (importedEntries == null || importedEntries.Count == 0)
                    return 0;

                await _semaphore.WaitAsync();
                try
                {
                    // Preserve existing entries that don't clash with imported ones
                    var existingIds = _entries.Select(e => e.Id).ToHashSet();
                    var newEntries = importedEntries.Where(e => !existingIds.Contains(e.Id)).ToList();
                    
                    // Add the new entries
                    _entries.AddRange(newEntries);
                    await SaveEntriesAsync();
                    
                    return newEntries.Count;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch
            {
                return 0;
            }
        }
    }
} 