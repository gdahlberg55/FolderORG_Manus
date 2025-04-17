using FolderORG.Manus.Core.Interfaces;
using System.Collections.ObjectModel;

namespace FolderORG.Manus.UI.ViewModels
{
    /// <summary>
    /// Represents a conflict with resolution selection capability.
    /// </summary>
    public class SelectableConflict : ViewModelBase
    {
        private string _selectedResolution;

        /// <summary>
        /// Initializes a new instance of the SelectableConflict class.
        /// </summary>
        /// <param name="conflict">The conflict to wrap.</param>
        public SelectableConflict(RestoreConflict conflict)
        {
            FilePath = conflict.FilePath;
            ConflictType = conflict.ConflictType;
            Description = conflict.Description;
            ResolutionOptions = new ObservableCollection<string>(conflict.ResolutionOptions);
            _selectedResolution = conflict.RecommendedResolution;
        }

        /// <summary>
        /// Gets the path of the file with a conflict.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the type of conflict.
        /// </summary>
        public string ConflictType { get; }

        /// <summary>
        /// Gets the description of the conflict.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the available resolution options.
        /// </summary>
        public ObservableCollection<string> ResolutionOptions { get; }

        /// <summary>
        /// Gets or sets the selected resolution option.
        /// </summary>
        public string SelectedResolution
        {
            get => _selectedResolution;
            set => SetProperty(ref _selectedResolution, value);
        }
    }
} 