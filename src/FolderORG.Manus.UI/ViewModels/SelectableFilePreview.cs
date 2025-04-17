using FolderORG.Manus.Core.Interfaces;
using System.Collections.ObjectModel;

namespace FolderORG.Manus.UI.ViewModels
{
    /// <summary>
    /// Represents a file preview with selection capability.
    /// </summary>
    public class SelectableFilePreview : ViewModelBase
    {
        private bool _isSelected;

        /// <summary>
        /// Initializes a new instance of the SelectableFilePreview class.
        /// </summary>
        /// <param name="filePreview">The file preview to wrap.</param>
        public SelectableFilePreview(RestoreFilePreview filePreview)
        {
            SourcePath = filePreview.SourcePath;
            TargetPath = filePreview.TargetPath;
            Size = filePreview.Size;
            TargetExists = filePreview.TargetExists;
            SourceExists = filePreview.SourceExists;
            Operation = filePreview.Operation;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this file is selected for restoration.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Gets the source path in the backup.
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Gets the target path where the file will be restored.
        /// </summary>
        public string TargetPath { get; }

        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets a value indicating whether the file exists at the target location.
        /// </summary>
        public bool TargetExists { get; }

        /// <summary>
        /// Gets a value indicating whether the source file exists in the backup.
        /// </summary>
        public bool SourceExists { get; }

        /// <summary>
        /// Gets the type of operation (e.g., Restore, Skip, Merge).
        /// </summary>
        public string Operation { get; }
    }
} 