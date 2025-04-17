using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Core.Models;

namespace FolderORG.Manus.Domain.Classification.Classifiers
{
    /// <summary>
    /// Classifier that categorizes files based on their creation or modification dates.
    /// </summary>
    public class DateClassifier : IFileClassifier
    {
        private readonly bool _useModificationDate;

        /// <summary>
        /// Initializes a new instance of the DateClassifier class.
        /// </summary>
        /// <param name="useModificationDate">If true, uses the last modified date for classification; otherwise, uses the creation date.</param>
        public DateClassifier(bool useModificationDate = false)
        {
            _useModificationDate = useModificationDate;
        }

        /// <summary>
        /// Gets the unique name of the classifier.
        /// </summary>
        public string Name => "DateClassifier";

        /// <summary>
        /// Gets the description of the classifier.
        /// </summary>
        public string Description => _useModificationDate 
            ? "Classifies files based on their last modification date" 
            : "Classifies files based on their creation date";

        /// <summary>
        /// Gets the priority of the classifier. Lower priority than extension classifier.
        /// </summary>
        public int Priority => 70;

        /// <summary>
        /// Determines whether the classifier can process the specified file based on its metadata.
        /// </summary>
        /// <param name="metadata">The metadata of the file to check.</param>
        /// <returns>True if the classifier can process the file; otherwise, false.</returns>
        public bool CanClassify(FileMetadata metadata)
        {
            // Can classify any file with a valid date
            DateTime dateToCheck = _useModificationDate ? metadata.LastWriteTime : metadata.CreationTime;
            return dateToCheck != DateTime.MinValue;
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
                PrimaryCategory = "Date",
                Confidence = 1.0f // Date classification is always certain
            };

            // Get the relevant date
            DateTime fileDate = _useModificationDate ? metadata.LastWriteTime : metadata.CreationTime;
            
            // Current date for comparison
            DateTime now = DateTime.Now;
            TimeSpan age = now - fileDate;

            // Determine age category
            string ageCategory;
            if (age.TotalDays <= 1)
            {
                ageCategory = "Today";
            }
            else if (age.TotalDays <= 7)
            {
                ageCategory = "ThisWeek";
            }
            else if (age.TotalDays <= 30)
            {
                ageCategory = "ThisMonth";
            }
            else if (age.TotalDays <= 90)
            {
                ageCategory = "Last3Months";
            }
            else if (age.TotalDays <= 365)
            {
                ageCategory = "ThisYear";
            }
            else
            {
                ageCategory = "Older";
            }

            result.SubCategory = ageCategory;
            result.AddTag("Date-" + ageCategory);
            result.AddTag(fileDate.Year.ToString());
            result.AddTag(fileDate.ToString("yyyy-MM"));

            result.AddAttribute("Date", 1.0f);
            result.AddAttribute(ageCategory, 1.0f);
            result.AddAttribute("Year-" + fileDate.Year.ToString(), 0.9f);
            result.AddAttribute("Month-" + fileDate.ToString("yyyy-MM"), 0.8f);

            // Add date information as extended properties
            string dateType = _useModificationDate ? "Modified" : "Created";
            result.FileMetadata.ExtendedProperties[$"{dateType}Year"] = fileDate.Year;
            result.FileMetadata.ExtendedProperties[$"{dateType}Month"] = fileDate.Month;
            result.FileMetadata.ExtendedProperties[$"{dateType}Day"] = fileDate.Day;
            result.FileMetadata.ExtendedProperties[$"{dateType}Formatted"] = fileDate.ToString("yyyy-MM-dd");

            // Determine a suggested path based on the date classification
            string yearMonth = fileDate.ToString("yyyy-MM");
            result.SuggestedPath = Path.Combine("By Date", yearMonth);

            return Task.FromResult(result);
        }
    }
} 