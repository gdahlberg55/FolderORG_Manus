using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Core.Interfaces
{
    /// <summary>
    /// Interface for the Memory Bank service responsible for tracking and managing file organization history.
    /// </summary>
    public interface IMemoryBankService
    {
        /// <summary>
        /// Adds a new entry to the Memory Bank.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        /// <returns>The added entry with its assigned ID.</returns>
        Task<MemoryBankEntry> AddEntryAsync(MemoryBankEntry entry);

        /// <summary>
        /// Updates an existing entry in the Memory Bank.
        /// </summary>
        /// <param name="entry">The entry to update.</param>
        /// <returns>True if the entry was successfully updated; otherwise, false.</returns>
        Task<bool> UpdateEntryAsync(MemoryBankEntry entry);

        /// <summary>
        /// Gets an entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the entry to retrieve.</param>
        /// <returns>The entry if found; otherwise, null.</returns>
        Task<MemoryBankEntry?> GetEntryByIdAsync(Guid id);

        /// <summary>
        /// Gets entries by file name.
        /// </summary>
        /// <param name="fileName">The file name to search for.</param>
        /// <returns>A collection of entries matching the file name.</returns>
        Task<IEnumerable<MemoryBankEntry>> GetEntriesByFileNameAsync(string fileName);

        /// <summary>
        /// Gets entries by category.
        /// </summary>
        /// <param name="category">The category to search for.</param>
        /// <returns>A collection of entries matching the category.</returns>
        Task<IEnumerable<MemoryBankEntry>> GetEntriesByCategoryAsync(string category);

        /// <summary>
        /// Gets entries organized within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>A collection of entries organized within the specified date range.</returns>
        Task<IEnumerable<MemoryBankEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets all entries in the Memory Bank.
        /// </summary>
        /// <returns>A collection of all entries.</returns>
        Task<IEnumerable<MemoryBankEntry>> GetAllEntriesAsync();

        /// <summary>
        /// Searches for entries based on various criteria.
        /// </summary>
        /// <param name="searchText">The text to search for in file names, categories, etc.</param>
        /// <returns>A collection of entries matching the search criteria.</returns>
        Task<IEnumerable<MemoryBankEntry>> SearchEntriesAsync(string searchText);

        /// <summary>
        /// Deletes an entry from the Memory Bank.
        /// </summary>
        /// <param name="id">The ID of the entry to delete.</param>
        /// <returns>True if the entry was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteEntryAsync(Guid id);

        /// <summary>
        /// Verifies that the entries in the Memory Bank still exist at their specified locations.
        /// </summary>
        /// <returns>A collection of entries that no longer exist at their specified locations.</returns>
        Task<IEnumerable<MemoryBankEntry>> VerifyEntriesExistAsync();

        /// <summary>
        /// Gets the Memory Bank statistics (e.g., total entries, entries by category).
        /// </summary>
        /// <returns>A dictionary containing various statistics.</returns>
        Task<Dictionary<string, object>> GetStatisticsAsync();

        /// <summary>
        /// Exports the Memory Bank entries to a file.
        /// </summary>
        /// <param name="filePath">The path where the file will be saved.</param>
        /// <returns>True if the export was successful; otherwise, false.</returns>
        Task<bool> ExportToFileAsync(string filePath);

        /// <summary>
        /// Imports Memory Bank entries from a file.
        /// </summary>
        /// <param name="filePath">The path of the file to import.</param>
        /// <returns>The number of entries imported.</returns>
        Task<int> ImportFromFileAsync(string filePath);
    }
} 