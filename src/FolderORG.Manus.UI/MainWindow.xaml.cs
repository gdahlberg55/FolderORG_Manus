using FolderORG.Manus.Application.Services;
using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;

namespace FolderORG.Manus.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IClassificationEngine _classificationEngine;
        private readonly MemoryBankManager _memoryBankManager;
        private readonly ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
        private readonly ICollectionView _filesView;
        private bool _isBusy = false;

        public MainWindow(IClassificationEngine classificationEngine, MemoryBankManager memoryBankManager)
        {
            InitializeComponent();
            _classificationEngine = classificationEngine;
            _memoryBankManager = memoryBankManager;

            // Set up the DataGrid binding
            FilesDataGrid.ItemsSource = _files;
            _filesView = CollectionViewSource.GetDefaultView(_files);
            
            // Initialize UI state
            UpdateUIState();
            LogMessage("Application started. Ready to organize files.");
        }

        private void UpdateUIState()
        {
            // Enable/disable the Organize button based on whether files are available and selected
            OrganizeFilesButton.IsEnabled = _files.Count > 0 && !_isBusy;
            
            // Update progress visibility
            OperationProgressBar.Visibility = _isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LogTextBox.AppendText($"[{timestamp}] {message}\r\n");
            LogTextBox.ScrollToEnd();
            StatusTextBlock.Text = message;
        }

        #region Event Handlers

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Source Folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void BrowseTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Target Folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TargetFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private async void ScanFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceFolderTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select a source folder.", "Missing Source Folder", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(SourceFolderTextBox.Text))
            {
                System.Windows.MessageBox.Show("The selected source folder does not exist.", "Invalid Source Folder", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isBusy = true;
                UpdateUIState();
                _files.Clear();

                LogMessage($"Scanning files in {SourceFolderTextBox.Text}...");
                
                // Get files from the selected directory
                var searchOption = IncludeSubfoldersCheckBox.IsChecked == true 
                    ? SearchOption.AllDirectories 
                    : SearchOption.TopDirectoryOnly;
                
                var directory = new DirectoryInfo(SourceFolderTextBox.Text);
                var files = directory.GetFiles("*.*", searchOption);

                LogMessage($"Found {files.Length} files. Analyzing...");

                int processedCount = 0;
                foreach (var file in files)
                {
                    try
                    {
                        // Classify the file
                        var result = await _classificationEngine.ClassifyFileAsync(file);
                        
                        // Create a view model for the file
                        var fileViewModel = new FileViewModel
                        {
                            IsSelected = true,
                            FileName = file.Name,
                            FullPath = file.FullName,
                            FormattedSize = result.FileMetadata.ExtendedProperties.ContainsKey("FormattedSize") 
                                ? result.FileMetadata.ExtendedProperties["FormattedSize"].ToString() 
                                : FormatFileSize(file.Length),
                            FormattedDate = file.LastWriteTime.ToString("yyyy-MM-dd"),
                            Category = result.PrimaryCategory,
                            SubCategory = result.SubCategory,
                            TargetPath = result.SuggestedPath,
                            ClassificationResult = result,
                            FileIcon = GetFileIconKind(file.Extension)
                        };

                        _files.Add(fileViewModel);
                        
                        // Update progress
                        processedCount++;
                        if (processedCount % 10 == 0 || processedCount == files.Length)
                        {
                            OperationProgressBar.Value = (double)processedCount / files.Length * 100;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error processing file {file.Name}: {ex.Message}");
                    }
                }

                LogMessage($"Scan completed. Processed {processedCount} files.");
                
                // Switch to the Files tab
                ResultsTabControl.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LogMessage($"Error scanning files: {ex.Message}");
                System.Windows.MessageBox.Show($"An error occurred while scanning files: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isBusy = false;
                UpdateUIState();
            }
        }

        private async void OrganizeFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TargetFolderTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select a target folder.", "Missing Target Folder", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(TargetFolderTextBox.Text))
            {
                // Create the directory if the option is checked
                if (CreateMissingFoldersCheckBox.IsChecked == true)
                {
                    try
                    {
                        Directory.CreateDirectory(TargetFolderTextBox.Text);
                        LogMessage($"Created target directory: {TargetFolderTextBox.Text}");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Failed to create target directory: {ex.Message}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("The selected target folder does not exist.", 
                        "Invalid Target Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Confirm the operation
            var selectedFiles = _files.Where(f => f.IsSelected).ToList();
            if (selectedFiles.Count == 0)
            {
                System.Windows.MessageBox.Show("No files selected for organization.", 
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var operationType = MoveFilesRadioButton.IsChecked == true 
                ? FileOperationService.FileOperationType.Move 
                : FileOperationService.FileOperationType.Copy;
            
            var operation = operationType == FileOperationService.FileOperationType.Move ? "move" : "copy";
            var result = System.Windows.MessageBox.Show(
                $"You are about to {operation} {selectedFiles.Count} files to {TargetFolderTextBox.Text}. Continue?",
                "Confirm Organization", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _isBusy = true;
                UpdateUIState();
                OperationProgressBar.Value = 0;

                LogMessage($"Starting file organization. Operation: {operation}");

                // Get the organization method
                var selectedMethod = ((ComboBoxItem)OrganizationMethodComboBox.SelectedItem).Content.ToString();
                
                // Process files using the Memory Bank Manager
                int processedCount = 0;
                int successCount = 0;
                
                foreach (var file in selectedFiles)
                {
                    try
                    {
                        var operationResult = await _memoryBankManager.OrganizeFileAsync(
                            file.ClassificationResult,
                            TargetFolderTextBox.Text,
                            operationType,
                            CreateMissingFoldersCheckBox.IsChecked == true,
                            selectedMethod);

                        if (operationResult.Success)
                        {
                            LogMessage($"{operation}d: {file.FileName} -> {operationResult.DestinationPath}");
                            successCount++;
                        }
                        else
                        {
                            LogMessage($"Error {operation}ing {file.FileName}: {operationResult.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error processing {file.FileName}: {ex.Message}");
                    }

                    // Update progress
                    processedCount++;
                    OperationProgressBar.Value = (double)processedCount / selectedFiles.Count * 100;
                }

                LogMessage($"Organization completed. {successCount} of {selectedFiles.Count} files processed successfully.");
                
                // If we moved files, refresh the file list
                if (operationType == FileOperationService.FileOperationType.Move && successCount > 0)
                {
                    // Remove processed files from the list if they were moved
                    foreach (var file in selectedFiles.Where(f => !File.Exists(f.FullPath)).ToList())
                    {
                        _files.Remove(file);
                    }
                }

                // Show completion message
                System.Windows.MessageBox.Show(
                    $"Organization completed. {successCount} of {selectedFiles.Count} files processed successfully.",
                    "Organization Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Update statistics tab if needed
                await UpdateStatisticsAsync();
            }
            catch (Exception ex)
            {
                LogMessage($"Error during organization: {ex.Message}");
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isBusy = false;
                UpdateUIState();
            }
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                // Get statistics from the Memory Bank
                var stats = await _memoryBankManager.GetStatisticsAsync();
                
                // TODO: Update the Statistics tab with the data
                // For now, just log some statistics
                LogMessage($"Memory Bank Statistics: {stats["TotalEntries"]} total entries");
            }
            catch (Exception ex)
            {
                LogMessage($"Error updating statistics: {ex.Message}");
            }
        }

        private void OrganizationMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_files.Count == 0 || OrganizationMethodComboBox.SelectedItem == null)
                return;

            var selectedMethod = ((ComboBoxItem)OrganizationMethodComboBox.SelectedItem).Content.ToString();
            
            // Update target paths based on selected method
            foreach (var file in _files)
            {
                switch (selectedMethod)
                {
                    case "By File Type":
                        file.TargetPath = Path.Combine(file.Category, file.SubCategory);
                        break;
                    case "By Date":
                        // Use creation date for organization (year/month)
                        var date = File.GetCreationTime(file.FullPath);
                        file.TargetPath = Path.Combine("By Date", date.Year.ToString(), date.ToString("MM - MMMM"));
                        break;
                    case "By Size":
                        // Use size category from the SizeClassifier
                        if (file.ClassificationResult?.ClassifierName == "SizeClassifier")
                        {
                            file.TargetPath = file.ClassificationResult.SuggestedPath;
                        }
                        else
                        {
                            // Fallback if not classified by size classifier
                            string sizeCategory = GetSizeCategory(file.FullPath);
                            file.TargetPath = Path.Combine("By Size", sizeCategory);
                        }
                        break;
                    case "Custom Rules":
                        // For now, just use a custom folder
                        file.TargetPath = "Custom";
                        break;
                }
            }

            LogMessage($"Updated organization method to: {selectedMethod}");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filesView != null)
            {
                string searchText = SearchTextBox.Text.ToLower();
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _filesView.Filter = null;
                }
                else
                {
                    _filesView.Filter = obj =>
                    {
                        if (obj is FileViewModel file)
                        {
                            return file.FileName.ToLower().Contains(searchText) ||
                                   file.Category.ToLower().Contains(searchText) ||
                                   file.SubCategory.ToLower().Contains(searchText);
                        }
                        return false;
                    };
                }
            }
        }

        private void FilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIState();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
            LogMessage("Log cleared.");
        }

        #endregion

        #region Helper Methods

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }

        private string GetSizeCategory(string filePath)
        {
            long length = new FileInfo(filePath).Length;
            
            if (length < 10 * 1024) // 10 KB
                return "Tiny";
            if (length < 1 * 1024 * 1024) // 1 MB
                return "Small";
            if (length < 10 * 1024 * 1024) // 10 MB
                return "Medium";
            if (length < 100 * 1024 * 1024) // 100 MB
                return "Large";
            if (length < 1 * 1024 * 1024 * 1024) // 1 GB
                return "VeryLarge";
            
            return "Huge";
        }

        private PackIconKind GetFileIconKind(string extension)
        {
            extension = extension.ToLower();
            
            switch (extension)
            {
                case ".pdf":
                    return PackIconKind.FilePdfBox;
                case ".doc":
                case ".docx":
                    return PackIconKind.FileWordBox;
                case ".xls":
                case ".xlsx":
                    return PackIconKind.FileExcelBox;
                case ".ppt":
                case ".pptx":
                    return PackIconKind.FilePowerpointBox;
                case ".txt":
                    return PackIconKind.FileDocumentOutline;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return PackIconKind.FileImageOutline;
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                    return PackIconKind.FileVideoOutline;
                case ".mp3":
                case ".wav":
                case ".ogg":
                case ".flac":
                    return PackIconKind.FileMusic;
                case ".zip":
                case ".rar":
                case ".7z":
                    return PackIconKind.FileZip;
                case ".exe":
                case ".msi":
                    return PackIconKind.ApplicationOutline;
                default:
                    return PackIconKind.File;
            }
        }

        #endregion
    }

    public class FileViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _targetPath;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string FormattedSize { get; set; }
        public string FormattedDate { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }

        public string TargetPath
        {
            get => _targetPath;
            set
            {
                if (_targetPath != value)
                {
                    _targetPath = value;
                    OnPropertyChanged(nameof(TargetPath));
                }
            }
        }

        public PackIconKind FileIcon { get; set; }
        public ClassificationResult ClassificationResult { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 