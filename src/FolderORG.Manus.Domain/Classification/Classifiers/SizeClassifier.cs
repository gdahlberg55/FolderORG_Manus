using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Domain.Classification.Classifiers
{
    /// <summary>
    /// Classifier that categorizes files based on their size.
    /// </summary>
    public class SizeClassifier : IFileClassifier
    {
        // Size thresholds in bytes
        private const long _tinyThreshold = 10 * 1024; // 10 KB
        private const long _smallThreshold = 1 * 1024 * 1024; // 1 MB
        private const long _mediumThreshold = 10 * 1024 * 1024; // 10 MB
        private const long _largeThreshold = 100 * 1024 * 1024; // 100 MB
        private const long _hugeThreshold = 1 * 1024 * 1024 * 1024; // 1 GB

        /// <summary>
        /// Gets the unique name of the classifier.
        /// </summary>
        public string Name => "SizeClassifier";

        /// <summary>
        /// Gets the description of the classifier.
        /// </summary>
        public string Description => "Classifies files based on their size";

        /// <summary>
        /// Gets the priority of the classifier. Lower priority than extension classifier.
        /// </summary>
        public int Priority => 80;

        /// <summary>
        /// Determines whether the classifier can process the specified file based on its metadata.
        /// </summary>
        /// <param name="metadata">The metadata of the file to check.</param>
        /// <returns>True if the classifier can process the file; otherwise, false.</returns>
        public bool CanClassify(FileMetadata metadata)
        {
            // Can classify any file with a size
            return true;
        }

        /// <summary>
        /// Classifies a file asynchronously based on its metadata.
        /// </summary>
        /// <param name="metadata">The metadata of the file to classify.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the classification result.</returns>
        public Task<ClassificationResult> ClassifyAsync(FileMetadata metadata)
        {
            var result = new ClassificationResult
            {
                FileMetadata = metadata,
                ClassifierName = Name,
                ClassificationTime = DateTime.Now,
                PrimaryCategory = "Size",
                Confidence = 1.0f // Size classification is always certain
            };

            string sizeCategory;
            if (metadata.Size < _tinyThreshold)
            {
                sizeCategory = "Tiny";
            }
            else if (metadata.Size < _smallThreshold)
            {
                sizeCategory = "Small";
            }
            else if (metadata.Size < _mediumThreshold)
            {
                sizeCategory = "Medium";
            }
            else if (metadata.Size < _largeThreshold)
            {
                sizeCategory = "Large";
            }
            else if (metadata.Size < _hugeThreshold)
            {
                sizeCategory = "VeryLarge";
            }
            else
            {
                sizeCategory = "Huge";
            }

            result.SubCategory = sizeCategory;
            result.AddTag("Size-" + sizeCategory);
            result.AddAttribute("Size", 1.0f);
            result.AddAttribute(sizeCategory, 1.0f);

            // Add human-readable size information as additional attributes
            string sizeFormatted;
            float sizeValue;
            
            if (metadata.Size < 1024)
            {
                sizeFormatted = $"{metadata.Size} bytes";
                sizeValue = metadata.Size;
                result.AddAttribute("ByteSize", sizeValue);
            }
            else if (metadata.Size < 1024 * 1024)
            {
                sizeValue = (float)metadata.Size / 1024;
                sizeFormatted = $"{sizeValue:F2} KB";
                result.AddAttribute("KilobyteSize", sizeValue);
            }
            else if (metadata.Size < 1024 * 1024 * 1024)
            {
                sizeValue = (float)metadata.Size / (1024 * 1024);
                sizeFormatted = $"{sizeValue:F2} MB";
                result.AddAttribute("MegabyteSize", sizeValue);
            }
            else
            {
                sizeValue = (float)metadata.Size / (1024 * 1024 * 1024);
                sizeFormatted = $"{sizeValue:F2} GB";
                result.AddAttribute("GigabyteSize", sizeValue);
            }

            result.FileMetadata.ExtendedProperties["FormattedSize"] = sizeFormatted;

            // Determine a suggested path based on the size classification
            result.SuggestedPath = Path.Combine("By Size", sizeCategory);

            return Task.FromResult(result);
        }
    }
} 