using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;
using System.Text.RegularExpressions;

namespace FolderORG.Manus.Domain.Classification.Classifiers
{
    /// <summary>
    /// Classifier that categorizes files based on their file extensions.
    /// </summary>
    public class ExtensionClassifier : IFileClassifier
    {
        private readonly Dictionary<string, ExtensionCategory> _extensionCategories = new Dictionary<string, ExtensionCategory>
        {
            // Documents
            { ".pdf", new ExtensionCategory("Document", "PDF", 0.95f) },
            { ".doc", new ExtensionCategory("Document", "Word", 0.95f) },
            { ".docx", new ExtensionCategory("Document", "Word", 0.95f) },
            { ".xls", new ExtensionCategory("Document", "Excel", 0.95f) },
            { ".xlsx", new ExtensionCategory("Document", "Excel", 0.95f) },
            { ".ppt", new ExtensionCategory("Document", "PowerPoint", 0.95f) },
            { ".pptx", new ExtensionCategory("Document", "PowerPoint", 0.95f) },
            { ".txt", new ExtensionCategory("Document", "Text", 0.90f) },
            { ".rtf", new ExtensionCategory("Document", "RichText", 0.90f) },
            { ".odt", new ExtensionCategory("Document", "OpenDocument", 0.90f) },
            
            // Images
            { ".jpg", new ExtensionCategory("Image", "JPEG", 0.95f) },
            { ".jpeg", new ExtensionCategory("Image", "JPEG", 0.95f) },
            { ".png", new ExtensionCategory("Image", "PNG", 0.95f) },
            { ".gif", new ExtensionCategory("Image", "GIF", 0.95f) },
            { ".bmp", new ExtensionCategory("Image", "Bitmap", 0.90f) },
            { ".tiff", new ExtensionCategory("Image", "TIFF", 0.90f) },
            { ".svg", new ExtensionCategory("Image", "Vector", 0.90f) },
            { ".webp", new ExtensionCategory("Image", "WebP", 0.90f) },
            
            // Videos
            { ".mp4", new ExtensionCategory("Video", "MP4", 0.95f) },
            { ".avi", new ExtensionCategory("Video", "AVI", 0.95f) },
            { ".mkv", new ExtensionCategory("Video", "MKV", 0.95f) },
            { ".mov", new ExtensionCategory("Video", "QuickTime", 0.95f) },
            { ".wmv", new ExtensionCategory("Video", "WindowsMedia", 0.95f) },
            { ".flv", new ExtensionCategory("Video", "Flash", 0.90f) },
            { ".webm", new ExtensionCategory("Video", "WebM", 0.90f) },
            
            // Audio
            { ".mp3", new ExtensionCategory("Audio", "MP3", 0.95f) },
            { ".wav", new ExtensionCategory("Audio", "WAV", 0.95f) },
            { ".ogg", new ExtensionCategory("Audio", "OGG", 0.95f) },
            { ".flac", new ExtensionCategory("Audio", "FLAC", 0.95f) },
            { ".aac", new ExtensionCategory("Audio", "AAC", 0.95f) },
            { ".wma", new ExtensionCategory("Audio", "WindowsMedia", 0.90f) },
            
            // Archives
            { ".zip", new ExtensionCategory("Archive", "ZIP", 0.95f) },
            { ".rar", new ExtensionCategory("Archive", "RAR", 0.95f) },
            { ".7z", new ExtensionCategory("Archive", "7Zip", 0.95f) },
            { ".tar", new ExtensionCategory("Archive", "TAR", 0.90f) },
            { ".gz", new ExtensionCategory("Archive", "GZip", 0.90f) },
            
            // Code
            { ".cs", new ExtensionCategory("Code", "CSharp", 0.95f) },
            { ".java", new ExtensionCategory("Code", "Java", 0.95f) },
            { ".py", new ExtensionCategory("Code", "Python", 0.95f) },
            { ".js", new ExtensionCategory("Code", "JavaScript", 0.95f) },
            { ".html", new ExtensionCategory("Code", "HTML", 0.95f) },
            { ".css", new ExtensionCategory("Code", "CSS", 0.95f) },
            { ".php", new ExtensionCategory("Code", "PHP", 0.95f) },
            { ".cpp", new ExtensionCategory("Code", "C++", 0.95f) },
            { ".c", new ExtensionCategory("Code", "C", 0.95f) },
            { ".swift", new ExtensionCategory("Code", "Swift", 0.95f) },
            
            // Executables
            { ".exe", new ExtensionCategory("Executable", "Windows", 0.95f) },
            { ".msi", new ExtensionCategory("Executable", "WindowsInstaller", 0.95f) },
            { ".app", new ExtensionCategory("Executable", "MacOS", 0.90f) },
            { ".deb", new ExtensionCategory("Executable", "LinuxDebian", 0.90f) },
            { ".rpm", new ExtensionCategory("Executable", "LinuxRPM", 0.90f) },
            
            // Data
            { ".csv", new ExtensionCategory("Data", "CSV", 0.95f) },
            { ".json", new ExtensionCategory("Data", "JSON", 0.95f) },
            { ".xml", new ExtensionCategory("Data", "XML", 0.95f) },
            { ".sql", new ExtensionCategory("Data", "SQL", 0.95f) },
            { ".db", new ExtensionCategory("Data", "Database", 0.90f) },
            { ".sqlite", new ExtensionCategory("Data", "SQLite", 0.90f) }
        };

        /// <summary>
        /// Gets the unique name of the classifier.
        /// </summary>
        public string Name => "ExtensionClassifier";

        /// <summary>
        /// Gets the description of the classifier.
        /// </summary>
        public string Description => "Classifies files based on their extensions";

        /// <summary>
        /// Gets the priority of the classifier. Higher priority classifiers are executed first.
        /// </summary>
        public int Priority => 100; // High priority since it's a simple and fast classification

        /// <summary>
        /// Determines whether the classifier can process the specified file based on its metadata.
        /// </summary>
        /// <param name="metadata">The metadata of the file to check.</param>
        /// <returns>True if the classifier can process the file; otherwise, false.</returns>
        public bool CanClassify(FileMetadata metadata)
        {
            // Can classify any file with an extension
            return !string.IsNullOrEmpty(metadata.Extension);
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
                ClassificationTime = DateTime.Now
            };

            string extension = metadata.Extension.ToLowerInvariant();

            // Lookup the extension in our mapping dictionary
            if (_extensionCategories.TryGetValue(extension, out var category))
            {
                result.PrimaryCategory = category.PrimaryCategory;
                result.SubCategory = category.SubCategory;
                result.Confidence = category.Confidence;
                result.AddTag(category.PrimaryCategory);
                result.AddTag(category.SubCategory);
                result.AddAttribute(category.PrimaryCategory, category.Confidence);
                result.AddAttribute(category.SubCategory, category.Confidence * 0.9f);
            }
            else
            {
                // If not in the dictionary, try to categorize based on common patterns
                if (Regex.IsMatch(extension, @"\.(doc|docx|txt|rtf|odt|pdf|xls|xlsx|ppt|pptx)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Document";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Document");
                }
                else if (Regex.IsMatch(extension, @"\.(jpg|jpeg|png|gif|bmp|tiff|svg|webp)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Image";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Image");
                }
                else if (Regex.IsMatch(extension, @"\.(mp4|avi|mkv|mov|wmv|flv|webm)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Video";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Video");
                }
                else if (Regex.IsMatch(extension, @"\.(mp3|wav|ogg|flac|aac|wma)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Audio";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Audio");
                }
                else if (Regex.IsMatch(extension, @"\.(zip|rar|7z|tar|gz)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Archive";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Archive");
                }
                else if (Regex.IsMatch(extension, @"\.(cs|java|py|js|html|css|php|cpp|c|swift|go)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Code";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Code");
                }
                else if (Regex.IsMatch(extension, @"\.(exe|msi|app|deb|rpm|apk)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Executable";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Executable");
                }
                else if (Regex.IsMatch(extension, @"\.(csv|json|xml|sql|db|sqlite)$", RegexOptions.IgnoreCase))
                {
                    result.PrimaryCategory = "Data";
                    result.SubCategory = "Other";
                    result.Confidence = 0.7f;
                    result.AddTag("Data");
                }
                else
                {
                    // Unknown extension
                    result.PrimaryCategory = "Unknown";
                    result.SubCategory = "Unknown";
                    result.Confidence = 0.5f;
                    result.AddTag("Unknown");
                }
            }

            // Determine a suggested path based on the classification
            result.SuggestedPath = Path.Combine(result.PrimaryCategory, result.SubCategory);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Represents a category associated with a file extension.
        /// </summary>
        private class ExtensionCategory
        {
            /// <summary>
            /// Gets the primary category.
            /// </summary>
            public string PrimaryCategory { get; }

            /// <summary>
            /// Gets the subcategory.
            /// </summary>
            public string SubCategory { get; }

            /// <summary>
            /// Gets the confidence level for this categorization.
            /// </summary>
            public float Confidence { get; }

            /// <summary>
            /// Initializes a new instance of the ExtensionCategory class.
            /// </summary>
            /// <param name="primaryCategory">The primary category.</param>
            /// <param name="subCategory">The subcategory.</param>
            /// <param name="confidence">The confidence level.</param>
            public ExtensionCategory(string primaryCategory, string subCategory, float confidence)
            {
                PrimaryCategory = primaryCategory;
                SubCategory = subCategory;
                Confidence = confidence;
            }
        }
    }
} 