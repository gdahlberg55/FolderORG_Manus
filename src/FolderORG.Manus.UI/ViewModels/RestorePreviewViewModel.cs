using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using FolderORG.Manus.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FolderORG.Manus.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the RestorePreviewView to handle selective restoration.
    /// </summary>
    public class RestorePreviewViewModel : ViewModelBase
    {
        private readonly IRestorePointService _restorePointService;
        private CancellationTokenSource? _cancellationSource;
        private string _statusMessage = string.Empty;
        private bool _isLoading;
        private bool _hasStatusMessage;
        private readonly Action<bool, string> _completionCallback;
        private long _totalSize;
        private string _estimatedTime = "Calculating...";
        private string _searchText = string.Empty;
        private string _selectedFileType = "All Types";
        private ObservableCollection<SelectableFilePreview> _filteredFilesToRestore;
        private Dictionary<string, int> _fileTypeCount = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the RestorePreviewViewModel class.
        /// </summary>
        /// <param name="restorePointService">The restore point service.</param>
        /// <param name="restorePointId">The ID of the restore point to preview.</param>
        /// <param name="restorePointName">The name of the restore point.</param>
        /// <param name="completionCallback">Callback to execute after restoration completes.</param>
        public RestorePreviewViewModel(
            IRestorePointService restorePointService,
            Guid restorePointId,
            string restorePointName,
            Action<bool, string> completionCallback)
        {
            _restorePointService = restorePointService ?? throw new ArgumentNullException(nameof(restorePointService));
            _completionCallback = completionCallback ?? throw new ArgumentNullException(nameof(completionCallback));
            
            RestorePointId = restorePointId;
            RestorePointName = restorePointName;
            
            FilesToRestore = new ObservableCollection<SelectableFilePreview>();
            FilteredFilesToRestore = new ObservableCollection<SelectableFilePreview>();
            PotentialConflicts = new ObservableCollection<SelectableConflict>();
            FileTypes = new ObservableCollection<string>() { "All Types" };
            
            // Commands
            CancelCommand = new RelayCommand(_ => CancelOperation());
            RestoreCommand = new AsyncRelayCommand(_ => RestoreSelectedFilesAsync(), _ => CanRestore);
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            SelectNoneCommand = new RelayCommand(_ => SelectNone());
            ApplyFilterCommand = new RelayCommand(_ => ApplyFilter());
            
            // Start loading preview data
            _ = InitializeAsync();
        }

        /// <summary>
        /// Initializes the view model asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            await LoadPreviewDataAsync();
        }

        /// <summary>
        /// Gets the ID of the restore point.
        /// </summary>
        public Guid RestorePointId { get; }

        /// <summary>
        /// Gets the name of the restore point.
        /// </summary>
        public string RestorePointName { get; }

        /// <summary>
        /// Gets the collection of files that can be restored.
        /// </summary>
        public ObservableCollection<SelectableFilePreview> FilesToRestore { get; }

        /// <summary>
        /// Gets the filtered collection of files that can be restored.
        /// </summary>
        public ObservableCollection<SelectableFilePreview> FilteredFilesToRestore
        {
            get => _filteredFilesToRestore;
            private set => SetProperty(ref _filteredFilesToRestore, value);
        }

        /// <summary>
        /// Gets the collection of file types available for filtering.
        /// </summary>
        public ObservableCollection<string> FileTypes { get; }

        /// <summary>
        /// Gets or sets the selected file type for filtering.
        /// </summary>
        public string SelectedFileType
        {
            get => _selectedFileType;
            set
            {
                if (SetProperty(ref _selectedFileType, value))
                {
                    ApplyFilter();
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text for filtering files.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        /// <summary>
        /// Gets the collection of potential conflicts.
        /// </summary>
        public ObservableCollection<SelectableConflict> PotentialConflicts { get; }

        /// <summary>
        /// Gets or sets the total number of files to restore.
        /// </summary>
        public int FileCount => FilesToRestore.Count;

        /// <summary>
        /// Gets the count of filtered files.
        /// </summary>
        public int FilteredFileCount => FilteredFilesToRestore.Count;

        /// <summary>
        /// Gets or sets the total size of files to restore.
        /// </summary>
        public long TotalSize
        {
            get => _totalSize;
            private set => SetProperty(ref _totalSize, value);
        }

        /// <summary>
        /// Gets the estimated time for the restore operation.
        /// </summary>
        public string EstimatedTime
        {
            get => _estimatedTime;
            private set => SetProperty(ref _estimatedTime, value);
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetProperty(ref _statusMessage, value))
                {
                    HasStatusMessage = !string.IsNullOrWhiteSpace(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether there is a status message.
        /// </summary>
        public bool HasStatusMessage
        {
            get => _hasStatusMessage;
            private set => SetProperty(ref _hasStatusMessage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Gets a value indicating whether a restore operation can be performed.
        /// </summary>
        public bool CanRestore => FilesToRestore.Any(f => f.IsSelected) && !IsLoading;

        /// <summary>
        /// Gets the command for canceling the operation.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Gets the command for restoring selected files.
        /// </summary>
        public ICommand RestoreCommand { get; }

        /// <summary>
        /// Gets the command for selecting all files.
        /// </summary>
        public ICommand SelectAllCommand { get; }

        /// <summary>
        /// Gets the command for deselecting all files.
        /// </summary>
        public ICommand SelectNoneCommand { get; }

        /// <summary>
        /// Gets the command for applying the filter.
        /// </summary>
        public ICommand ApplyFilterCommand { get; }

        /// <summary>
        /// Loads preview data for the restore point.
        /// </summary>
        private async Task LoadPreviewDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading restore preview...";
                
                // Load preview from service
                var preview = await _restorePointService.CreateRestorePreviewAsync(RestorePointId);
                
                // Update UI properties
                TotalSize = preview.TotalSize;
                
                // Format estimated time
                TimeSpan estimatedTimeSpan = TimeSpan.FromSeconds(preview.EstimatedTimeInSeconds);
                EstimatedTime = estimatedTimeSpan.TotalHours >= 1
                    ? $"{estimatedTimeSpan.Hours}h {estimatedTimeSpan.Minutes}m"
                    : estimatedTimeSpan.TotalMinutes >= 1
                        ? $"{estimatedTimeSpan.Minutes}m {estimatedTimeSpan.Seconds}s"
                        : $"{estimatedTimeSpan.Seconds}s";
                
                // Populate files to restore
                foreach (var filePreview in preview.FilesToRestore)
                {
                    FilesToRestore.Add(new SelectableFilePreview(filePreview));
                    
                    // Extract file extension for filtering
                    string extension = Path.GetExtension(filePreview.SourcePath).ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = "(No Extension)";
                    }
                    
                    // Count file types
                    if (_fileTypeCount.ContainsKey(extension))
                    {
                        _fileTypeCount[extension]++;
                    }
                    else
                    {
                        _fileTypeCount[extension] = 1;
                    }
                }
                
                // Add file types to the filter collection
                foreach (var fileType in _fileTypeCount.OrderByDescending(f => f.Value))
                {
                    FileTypes.Add(fileType.Key);
                }
                
                // Populate potential conflicts
                foreach (var conflict in preview.PotentialConflicts)
                {
                    PotentialConflicts.Add(new SelectableConflict(conflict));
                }
                
                // Select all files by default
                SelectAll();
                
                // Initialize filtered collection
                ApplyFilter();
                
                StatusMessage = "Restore preview loaded successfully";
                OnPropertyChanged(nameof(FileCount));
                OnPropertyChanged(nameof(FilteredFileCount));
                OnPropertyChanged(nameof(TotalSize));
                OnPropertyChanged(nameof(EstimatedTime));
                OnPropertyChanged(nameof(CanRestore));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading preview: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Selects all files for restoration.
        /// </summary>
        private void SelectAll()
        {
            // If we have a filter active, only select files in the filtered view
            if (SelectedFileType != "All Types" || !string.IsNullOrWhiteSpace(SearchText))
            {
                // First, keep track of currently selected files
                var currentlySelected = FilesToRestore
                    .Where(f => f.IsSelected)
                    .Select(f => f.SourcePath)
                    .ToHashSet();

                // Only modify selection state for files in the filtered view
                foreach (var file in FilteredFilesToRestore)
                {
                    file.IsSelected = true;
                }
            }
            else
            {
                // No filter, select all files
                foreach (var file in FilesToRestore)
                {
                    file.IsSelected = true;
                }
            }
            
            OnPropertyChanged(nameof(CanRestore));
        }

        /// <summary>
        /// Deselects all files.
        /// </summary>
        private void SelectNone()
        {
            // If we have a filter active, only deselect files in the filtered view
            if (SelectedFileType != "All Types" || !string.IsNullOrWhiteSpace(SearchText))
            {
                // First, keep track of currently selected files
                var currentlySelected = FilesToRestore
                    .Where(f => f.IsSelected)
                    .Select(f => f.SourcePath)
                    .ToHashSet();

                // Only modify selection state for files in the filtered view
                foreach (var file in FilteredFilesToRestore)
                {
                    file.IsSelected = false;
                }
            }
            else
            {
                // No filter, deselect all files
                foreach (var file in FilesToRestore)
                {
                    file.IsSelected = false;
                }
            }
            
            OnPropertyChanged(nameof(CanRestore));
        }

        /// <summary>
        /// Applies the current filter settings to the files list.
        /// </summary>
        private void ApplyFilter()
        {
            FilteredFilesToRestore.Clear();
            
            IEnumerable<SelectableFilePreview> filtered = FilesToRestore;
            
            // Apply file type filter
            if (SelectedFileType != "All Types")
            {
                filtered = filtered.Where(f => Path.GetExtension(f.SourcePath).ToLowerInvariant() == SelectedFileType 
                    || (string.IsNullOrEmpty(Path.GetExtension(f.SourcePath)) && SelectedFileType == "(No Extension)"));
            }
            
            // Apply search text filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(f => 
                    f.SourcePath.ToLowerInvariant().Contains(search) || 
                    f.TargetPath.ToLowerInvariant().Contains(search) ||
                    f.Operation.ToLowerInvariant().Contains(search));
            }
            
            // Add filtered items
            foreach (var item in filtered)
            {
                FilteredFilesToRestore.Add(item);
            }
            
            OnPropertyChanged(nameof(FilteredFileCount));
        }

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        private void CancelOperation()
        {
            _cancellationSource?.Cancel();
            _completionCallback(false, "Operation cancelled by user");
        }

        /// <summary>
        /// Restores selected files.
        /// </summary>
        private async Task RestoreSelectedFilesAsync()
        {
            try
            {
                // Cancel any ongoing operation
                _cancellationSource?.Cancel();
                _cancellationSource = new CancellationTokenSource();
                
                IsLoading = true;
                StatusMessage = "Starting restore operation...";
                
                // Get selected file paths
                var selectedFilePaths = FilesToRestore
                    .Where(f => f.IsSelected)
                    .Select(f => f.SourcePath)
                    .ToList();
                
                if (selectedFilePaths.Count == 0)
                {
                    StatusMessage = "No files selected for restoration";
                    IsLoading = false;
                    return;
                }
                
                // Create progress reporter
                var progress = new Progress<(int ProgressPercentage, string StatusMessage)>(update =>
                {
                    StatusMessage = $"{update.StatusMessage} ({update.ProgressPercentage}%)";
                });
                
                // Collect conflict resolutions
                Dictionary<string, string> conflictResolutions = new Dictionary<string, string>();
                foreach (var conflict in PotentialConflicts)
                {
                    // Check if the file is selected for restoration
                    var filePreview = FilesToRestore.FirstOrDefault(f => f.SourcePath == conflict.FilePath);
                    if (filePreview != null && filePreview.IsSelected && !string.IsNullOrEmpty(conflict.SelectedResolution))
                    {
                        // Add to resolutions dictionary
                        conflictResolutions[conflict.FilePath] = conflict.SelectedResolution;
                    }
                }
                
                // Configure restoration options
                var options = new RestoreOptions
                {
                    // If restoring more than 50 files, enable parallel processing
                    UseParallelProcessing = selectedFilePaths.Count > 50,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    ConflictResolutions = conflictResolutions,
                    PreserveTimestamps = true,
                    CreateMissingDirectories = true
                };
                
                // Perform selective restore
                var result = await _restorePointService.RestoreSelectedFilesAsync(
                    RestorePointId,
                    selectedFilePaths,
                    options,
                    _cancellationSource.Token,
                    progress);
                
                // Handle result
                if (result.Success)
                {
                    StatusMessage = $"Restore completed successfully. Files restored: {result.FilesRestored}";
                    _completionCallback(true, StatusMessage);
                }
                else
                {
                    StatusMessage = $"Restore failed: {result.ErrorMessage}. Files restored: {result.FilesRestored}, Failed: {result.FilesFailed}";
                    _completionCallback(false, StatusMessage);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during restore: {ex.Message}";
                _completionCallback(false, StatusMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 