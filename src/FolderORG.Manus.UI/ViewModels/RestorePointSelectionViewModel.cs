using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FolderORG.Manus.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the RestorePointSelectionView.
    /// </summary>
    public class RestorePointSelectionViewModel : INotifyPropertyChanged
    {
        private readonly IRestorePointService _restorePointService;
        private readonly IFileTransactionService _fileTransactionService;
        private RestorePoint? _selectedRestorePoint;
        private bool _isDetailsExpanded;
        private string _searchText = string.Empty;
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;
        private bool _isLoading;
        private CancellationTokenSource? _cancellationSource;

        /// <summary>
        /// Initializes a new instance of the RestorePointSelectionViewModel class.
        /// </summary>
        /// <param name="restorePointService">The restore point service.</param>
        /// <param name="fileTransactionService">The file transaction service.</param>
        public RestorePointSelectionViewModel(IRestorePointService restorePointService, IFileTransactionService fileTransactionService)
        {
            _restorePointService = restorePointService ?? throw new ArgumentNullException(nameof(restorePointService));
            _fileTransactionService = fileTransactionService ?? throw new ArgumentNullException(nameof(fileTransactionService));
            
            RestorePoints = new ObservableCollection<RestorePoint>();
            
            RefreshCommand = new RelayCommand(_ => RefreshRestorePointsAsync());
            RestoreCommand = new RelayCommand(_ => RestoreSelectedPointAsync(), _ => CanRestore);
            PreviewRestoreCommand = new RelayCommand(_ => PreviewRestoreAsync(), _ => SelectedRestorePoint != null);
            
            // Load restore points initially
            RefreshRestorePointsAsync();
        }

        /// <summary>
        /// Gets the collection of restore points.
        /// </summary>
        public ObservableCollection<RestorePoint> RestorePoints { get; }

        /// <summary>
        /// Gets or sets the selected restore point.
        /// </summary>
        public RestorePoint? SelectedRestorePoint
        {
            get => _selectedRestorePoint;
            set
            {
                if (_selectedRestorePoint != value)
                {
                    _selectedRestorePoint = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanRestore));
                    IsDetailsExpanded = value != null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the details panel is expanded.
        /// </summary>
        public bool IsDetailsExpanded
        {
            get => _isDetailsExpanded;
            set
            {
                if (_isDetailsExpanded != value)
                {
                    _isDetailsExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterRestorePointsAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets the start date for filtering.
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                    FilterRestorePointsAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets the end date for filtering.
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                    FilterRestorePointsAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a restore operation can be performed.
        /// </summary>
        public bool CanRestore => SelectedRestorePoint != null && SelectedRestorePoint.Status == RestorePointStatus.Valid;

        /// <summary>
        /// Gets the command for refreshing restore points.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Gets the command for restoring from a selected point.
        /// </summary>
        public ICommand RestoreCommand { get; }

        /// <summary>
        /// Gets the command for previewing a restore operation.
        /// </summary>
        public ICommand PreviewRestoreCommand { get; }

        /// <summary>
        /// Refreshes the list of restore points.
        /// </summary>
        private async void RefreshRestorePointsAsync()
        {
            try
            {
                // Cancel any ongoing operation
                _cancellationSource?.Cancel();
                _cancellationSource = new CancellationTokenSource();
                
                IsLoading = true;
                
                // Clear current list
                RestorePoints.Clear();
                
                // Load restore points from service
                var restorePoints = await _restorePointService.GetRestorePointsByDateRangeAsync(StartDate, EndDate);
                
                // Apply text filter if needed
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    restorePoints = restorePoints.Where(rp => 
                        rp.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                        rp.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        rp.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }
                
                // Add to collection
                foreach (var restorePoint in restorePoints.OrderByDescending(rp => rp.CreationTime))
                {
                    RestorePoints.Add(restorePoint);
                }
            }
            catch (Exception ex)
            {
                // Handle errors (in a real application, show a message to the user)
                System.Diagnostics.Debug.WriteLine($"Error loading restore points: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Filters the restore points based on search criteria.
        /// </summary>
        private async void FilterRestorePointsAsync()
        {
            // Just refresh for now - in a real application, we might optimize this
            await Task.Delay(300); // Debounce
            RefreshRestorePointsAsync();
        }

        /// <summary>
        /// Creates a preview of the restore operation.
        /// </summary>
        private async void PreviewRestoreAsync()
        {
            if (SelectedRestorePoint == null)
                return;
            
            try
            {
                IsLoading = true;
                
                // Get preview from service
                var preview = await _restorePointService.CreateRestorePreviewAsync(SelectedRestorePoint.Id);
                
                // In a real application, display this preview in a dialog or new view
                // For now, just write to debug output
                System.Diagnostics.Debug.WriteLine($"Restore preview for {SelectedRestorePoint.Name}:");
                System.Diagnostics.Debug.WriteLine($"Files to restore: {preview.FilesToRestore.Count}");
                System.Diagnostics.Debug.WriteLine($"Total size: {preview.TotalSize} bytes");
                System.Diagnostics.Debug.WriteLine($"Estimated time: {preview.EstimatedTimeInSeconds} seconds");
                System.Diagnostics.Debug.WriteLine($"Potential conflicts: {preview.PotentialConflicts.Count}");
            }
            catch (Exception ex)
            {
                // Handle errors
                System.Diagnostics.Debug.WriteLine($"Error creating restore preview: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Restores from the selected restore point.
        /// </summary>
        private async void RestoreSelectedPointAsync()
        {
            if (SelectedRestorePoint == null)
                return;
            
            try
            {
                // Cancel any ongoing operation
                _cancellationSource?.Cancel();
                _cancellationSource = new CancellationTokenSource();
                
                IsLoading = true;
                
                // Create progress reporter
                var progress = new Progress<(int ProgressPercentage, string StatusMessage)>(update =>
                {
                    // Update progress in UI
                    System.Diagnostics.Debug.WriteLine($"Restore progress: {update.ProgressPercentage}% - {update.StatusMessage}");
                });
                
                // Perform restore
                var result = await _restorePointService.RestoreFromPointAsync(
                    SelectedRestorePoint.Id,
                    _cancellationSource.Token,
                    progress);
                
                // Handle result
                if (result.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"Restore completed successfully. Files restored: {result.FilesRestored}");
                    
                    // In a real application, show success message to user
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Restore failed: {result.ErrorMessage}. Files restored: {result.FilesRestored}, Failed: {result.FilesFailed}");
                    
                    // In a real application, show error message to user
                }
                
                // Refresh list after restore
                RefreshRestorePointsAsync();
            }
            catch (Exception ex)
            {
                // Handle errors
                System.Diagnostics.Debug.WriteLine($"Error during restore: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Simple implementation of ICommand for the MVVM pattern.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter!);
        }

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            _execute(parameter!);
        }
    }
} 