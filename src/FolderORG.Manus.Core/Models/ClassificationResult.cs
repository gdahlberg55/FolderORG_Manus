namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents the result of a file classification operation.
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// The metadata of the classified file
        /// </summary>
        public FileMetadata FileMetadata { get; set; } = new FileMetadata();

        /// <summary>
        /// Primary category assigned to the file (e.g., Document, Image, Video)
        /// </summary>
        public string PrimaryCategory { get; set; } = string.Empty;

        /// <summary>
        /// Subcategory for more specific classification (e.g., Spreadsheet, Photo, Movie)
        /// </summary>
        public string SubCategory { get; set; } = string.Empty;

        /// <summary>
        /// Collection of tags associated with the file based on classification
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Confidence level of the classification (0.0 to 1.0)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Collection of classification attributes with confidence levels
        /// </summary>
        public Dictionary<string, float> ClassificationAttributes { get; set; } = new Dictionary<string, float>();

        /// <summary>
        /// Suggested target path based on classification
        /// </summary>
        public string? SuggestedPath { get; set; }

        /// <summary>
        /// Name of the classifier that performed the classification
        /// </summary>
        public string ClassifierName { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the classification was performed
        /// </summary>
        public DateTime ClassificationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Adds a tag to the classification result if it doesn't already exist
        /// </summary>
        public void AddTag(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
            {
                Tags.Add(tag);
            }
        }

        /// <summary>
        /// Adds a classification attribute with confidence level
        /// </summary>
        public void AddAttribute(string attribute, float confidence)
        {
            if (!string.IsNullOrWhiteSpace(attribute))
            {
                ClassificationAttributes[attribute] = confidence;
            }
        }
    }
} 