using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Domain.Classification.Services
{
    /// <summary>
    /// Engine responsible for coordinating file classification using registered classifiers.
    /// </summary>
    public class ClassificationEngine : IClassificationEngine
    {
        private readonly Dictionary<string, IFileClassifier> _classifiers = new Dictionary<string, IFileClassifier>();
        private readonly List<IFileClassifier> _orderedClassifiers = new List<IFileClassifier>();
        private readonly object _classifierLock = new object();

        /// <summary>
        /// Initializes a new instance of the ClassificationEngine class.
        /// </summary>
        public ClassificationEngine()
        {
        }

        /// <summary>
        /// Registers a file classifier with the engine.
        /// </summary>
        /// <param name="classifier">The classifier to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the classifier is null.</exception>
        /// <exception cref="ArgumentException">Thrown if a classifier with the same name is already registered.</exception>
        public void RegisterClassifier(IFileClassifier classifier)
        {
            if (classifier == null)
                throw new ArgumentNullException(nameof(classifier));

            lock (_classifierLock)
            {
                if (_classifiers.ContainsKey(classifier.Name))
                    throw new ArgumentException($"A classifier with the name '{classifier.Name}' is already registered.");

                _classifiers.Add(classifier.Name, classifier);

                // Re-sort the ordered classifiers list by priority (descending)
                _orderedClassifiers.Add(classifier);
                _orderedClassifiers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }

        /// <summary>
        /// Unregisters a file classifier from the engine.
        /// </summary>
        /// <param name="classifierName">The name of the classifier to unregister.</param>
        /// <returns>True if the classifier was successfully unregistered; otherwise, false.</returns>
        public bool UnregisterClassifier(string classifierName)
        {
            if (string.IsNullOrEmpty(classifierName))
                return false;

            lock (_classifierLock)
            {
                if (!_classifiers.TryGetValue(classifierName, out var classifier))
                    return false;

                _classifiers.Remove(classifierName);
                _orderedClassifiers.Remove(classifier);
                return true;
            }
        }

        /// <summary>
        /// Gets all registered classifiers.
        /// </summary>
        /// <returns>A collection of registered classifiers.</returns>
        public IEnumerable<IFileClassifier> GetClassifiers()
        {
            lock (_classifierLock)
            {
                return _orderedClassifiers.ToList();
            }
        }

        /// <summary>
        /// Gets a classifier by name.
        /// </summary>
        /// <param name="name">The name of the classifier to retrieve.</param>
        /// <returns>The classifier with the specified name, or null if not found.</returns>
        public IFileClassifier? GetClassifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            lock (_classifierLock)
            {
                _classifiers.TryGetValue(name, out var classifier);
                return classifier;
            }
        }

        /// <summary>
        /// Classifies a file asynchronously using appropriate registered classifiers.
        /// </summary>
        /// <param name="filePath">The path of the file to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        /// <exception cref="ArgumentException">Thrown if the file path is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        public async Task<ClassificationResult> ClassifyFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found.", filePath);

            return await ClassifyFileAsync(fileInfo);
        }

        /// <summary>
        /// Classifies a file asynchronously using appropriate registered classifiers.
        /// </summary>
        /// <param name="fileInfo">Information about the file to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if fileInfo is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        public async Task<ClassificationResult> ClassifyFileAsync(FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found.", fileInfo.FullName);

            // Extract metadata
            var metadata = await ExtractMetadataAsync(fileInfo.FullName);

            // Get a list of classifiers that can process this file
            IEnumerable<IFileClassifier> applicableClassifiers;
            lock (_classifierLock)
            {
                applicableClassifiers = _orderedClassifiers
                    .Where(c => c.CanClassify(metadata))
                    .ToList();
            }

            if (!applicableClassifiers.Any())
            {
                // No applicable classifiers found, return a default result
                return new ClassificationResult
                {
                    FileMetadata = metadata,
                    PrimaryCategory = "Unknown",
                    SubCategory = "Unknown",
                    Confidence = 0.0f,
                    ClassifierName = "None",
                    ClassificationTime = DateTime.Now
                };
            }

            // Use the first classifier with the highest priority
            var primaryClassifier = applicableClassifiers.First();
            var result = await primaryClassifier.ClassifyAsync(metadata);

            // If the confidence is low, try additional classifiers
            if (result.Confidence < 0.8f && applicableClassifiers.Count() > 1)
            {
                foreach (var classifier in applicableClassifiers.Skip(1))
                {
                    var additionalResult = await classifier.ClassifyAsync(metadata);
                    
                    // If this classifier has higher confidence, use its result instead
                    if (additionalResult.Confidence > result.Confidence)
                    {
                        result = additionalResult;
                    }
                    else
                    {
                        // Otherwise, merge tags and attributes
                        foreach (var tag in additionalResult.Tags)
                        {
                            result.AddTag(tag);
                        }

                        foreach (var attr in additionalResult.ClassificationAttributes)
                        {
                            if (!result.ClassificationAttributes.ContainsKey(attr.Key) || 
                                result.ClassificationAttributes[attr.Key] < attr.Value)
                            {
                                result.ClassificationAttributes[attr.Key] = attr.Value;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Classifies a file asynchronously using the specified classifier.
        /// </summary>
        /// <param name="filePath">The path of the file to classify.</param>
        /// <param name="classifierName">The name of the specific classifier to use.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        /// <exception cref="ArgumentException">Thrown if the file path or classifier name is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified classifier does not exist or cannot classify the file.</exception>
        public async Task<ClassificationResult> ClassifyFileWithClassifierAsync(string filePath, string classifierName)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (string.IsNullOrEmpty(classifierName))
                throw new ArgumentException("Classifier name cannot be null or empty.", nameof(classifierName));

            var classifier = GetClassifier(classifierName);
            if (classifier == null)
                throw new InvalidOperationException($"Classifier '{classifierName}' not found.");

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found.", filePath);

            var metadata = await ExtractMetadataAsync(filePath);

            if (!classifier.CanClassify(metadata))
                throw new InvalidOperationException($"Classifier '{classifierName}' cannot classify this file.");

            return await classifier.ClassifyAsync(metadata);
        }

        /// <summary>
        /// Classifies multiple files asynchronously.
        /// </summary>
        /// <param name="filePaths">The paths of the files to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification results.</returns>
        /// <exception cref="ArgumentNullException">Thrown if filePaths is null.</exception>
        public async Task<IEnumerable<ClassificationResult>> BatchClassifyAsync(IEnumerable<string> filePaths)
        {
            if (filePaths == null)
                throw new ArgumentNullException(nameof(filePaths));

            var tasks = filePaths
                .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
                .Select(ClassifyFileAsync);

            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Extracts metadata from a file.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the extracted metadata.</returns>
        /// <exception cref="ArgumentException">Thrown if the file path is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        public Task<FileMetadata> ExtractMetadataAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found.", filePath);

            var metadata = new FileMetadata
            {
                FullPath = fileInfo.FullName,
                FileName = fileInfo.Name,
                Extension = fileInfo.Extension.ToLowerInvariant(),
                Directory = fileInfo.DirectoryName ?? string.Empty,
                Size = fileInfo.Length,
                CreationTime = fileInfo.CreationTime,
                LastAccessTime = fileInfo.LastAccessTime,
                LastWriteTime = fileInfo.LastWriteTime,
                IsReadOnly = fileInfo.IsReadOnly
            };

            var attributes = File.GetAttributes(filePath);
            metadata.IsHidden = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            metadata.IsSystem = (attributes & FileAttributes.System) == FileAttributes.System;

            // Simple content type detection based on extension
            // In a real implementation, this would use a more robust content type detection library
            switch (metadata.Extension)
            {
                case ".jpg":
                case ".jpeg":
                    metadata.ContentType = "image/jpeg";
                    break;
                case ".png":
                    metadata.ContentType = "image/png";
                    break;
                case ".gif":
                    metadata.ContentType = "image/gif";
                    break;
                case ".pdf":
                    metadata.ContentType = "application/pdf";
                    break;
                case ".doc":
                case ".docx":
                    metadata.ContentType = "application/msword";
                    break;
                case ".xls":
                case ".xlsx":
                    metadata.ContentType = "application/vnd.ms-excel";
                    break;
                case ".txt":
                    metadata.ContentType = "text/plain";
                    break;
                case ".html":
                case ".htm":
                    metadata.ContentType = "text/html";
                    break;
                case ".mp3":
                    metadata.ContentType = "audio/mpeg";
                    break;
                case ".mp4":
                    metadata.ContentType = "video/mp4";
                    break;
                case ".zip":
                    metadata.ContentType = "application/zip";
                    break;
                default:
                    metadata.ContentType = "application/octet-stream";
                    break;
            }

            // In a real implementation, we would extract additional metadata
            // based on file type (e.g., image dimensions, document properties)

            return Task.FromResult(metadata);
        }
    }
} 