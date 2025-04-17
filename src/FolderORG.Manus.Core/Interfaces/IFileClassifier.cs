using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Core.Interfaces
{
    /// <summary>
    /// Interface for file classifiers that analyze files and assign categories and attributes.
    /// </summary>
    public interface IFileClassifier
    {
        /// <summary>
        /// Gets the unique name of the classifier.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the classifier.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the priority of the classifier. Higher priority classifiers are executed first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Determines whether the classifier can process the specified file based on its metadata.
        /// </summary>
        /// <param name="metadata">The metadata of the file to check.</param>
        /// <returns>True if the classifier can process the file; otherwise, false.</returns>
        bool CanClassify(FileMetadata metadata);

        /// <summary>
        /// Classifies a file asynchronously based on its metadata.
        /// </summary>
        /// <param name="metadata">The metadata of the file to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        Task<ClassificationResult> ClassifyAsync(FileMetadata metadata);
    }
} 