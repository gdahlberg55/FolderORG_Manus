using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Core.Interfaces
{
    /// <summary>
    /// Interface for the classification engine responsible for coordinating file classification.
    /// </summary>
    public interface IClassificationEngine
    {
        /// <summary>
        /// Registers a file classifier with the engine.
        /// </summary>
        /// <param name="classifier">The classifier to register.</param>
        void RegisterClassifier(IFileClassifier classifier);

        /// <summary>
        /// Unregisters a file classifier from the engine.
        /// </summary>
        /// <param name="classifierName">The name of the classifier to unregister.</param>
        /// <returns>True if the classifier was successfully unregistered; otherwise, false.</returns>
        bool UnregisterClassifier(string classifierName);

        /// <summary>
        /// Gets all registered classifiers.
        /// </summary>
        /// <returns>A collection of registered classifiers.</returns>
        IEnumerable<IFileClassifier> GetClassifiers();

        /// <summary>
        /// Gets a classifier by name.
        /// </summary>
        /// <param name="name">The name of the classifier to retrieve.</param>
        /// <returns>The classifier with the specified name, or null if not found.</returns>
        IFileClassifier? GetClassifier(string name);

        /// <summary>
        /// Classifies a file asynchronously using appropriate registered classifiers.
        /// </summary>
        /// <param name="filePath">The path of the file to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        Task<ClassificationResult> ClassifyFileAsync(string filePath);

        /// <summary>
        /// Classifies a file asynchronously using appropriate registered classifiers.
        /// </summary>
        /// <param name="fileInfo">Information about the file to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        Task<ClassificationResult> ClassifyFileAsync(FileInfo fileInfo);

        /// <summary>
        /// Classifies a file asynchronously using the specified classifier.
        /// </summary>
        /// <param name="filePath">The path of the file to classify.</param>
        /// <param name="classifierName">The name of the specific classifier to use.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        Task<ClassificationResult> ClassifyFileWithClassifierAsync(string filePath, string classifierName);

        /// <summary>
        /// Classifies multiple files asynchronously.
        /// </summary>
        /// <param name="filePaths">The paths of the files to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification results.</returns>
        Task<IEnumerable<ClassificationResult>> BatchClassifyAsync(IEnumerable<string> filePaths);

        /// <summary>
        /// Extracts metadata from a file.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the extracted metadata.</returns>
        Task<FileMetadata> ExtractMetadataAsync(string filePath);
    }
} 