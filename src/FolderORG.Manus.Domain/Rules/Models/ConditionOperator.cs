namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Defines operators for condition comparison
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>
        /// Equals comparison
        /// </summary>
        Equals = 0,
        
        /// <summary>
        /// Not equals comparison
        /// </summary>
        NotEquals = 1,
        
        /// <summary>
        /// Contains string comparison
        /// </summary>
        Contains = 2,
        
        /// <summary>
        /// Starts with string comparison
        /// </summary>
        StartsWith = 3,
        
        /// <summary>
        /// Ends with string comparison
        /// </summary>
        EndsWith = 4,
        
        /// <summary>
        /// Greater than numeric comparison
        /// </summary>
        GreaterThan = 5,
        
        /// <summary>
        /// Less than numeric comparison
        /// </summary>
        LessThan = 6,
        
        /// <summary>
        /// Between range comparison (inclusive)
        /// </summary>
        Between = 7,
        
        /// <summary>
        /// Regular expression pattern matching
        /// </summary>
        Matches = 8,
        
        /// <summary>
        /// In a list of values
        /// </summary>
        InList = 9,
        
        /// <summary>
        /// Before date comparison
        /// </summary>
        Before = 10,
        
        /// <summary>
        /// After date comparison
        /// </summary>
        After = 11,
        
        /// <summary>
        /// Within date range comparison
        /// </summary>
        Within = 12,
        
        /// <summary>
        /// Has specific file attribute(s)
        /// </summary>
        HasAttribute = 13
    }
} 