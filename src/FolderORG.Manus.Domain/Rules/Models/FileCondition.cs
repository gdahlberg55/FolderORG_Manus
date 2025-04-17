using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Represents a condition that can be evaluated against a file
    /// </summary>
    public class FileCondition
    {
        /// <summary>
        /// Unique identifier for the condition
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The type of condition being evaluated
        /// </summary>
        public ConditionType Type { get; set; }
        
        /// <summary>
        /// The operator to use for comparison
        /// </summary>
        public ConditionOperator Operator { get; set; }
        
        /// <summary>
        /// The value to compare against
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// For numeric comparisons, the lower bound of a range
        /// </summary>
        public double? NumericValueLower { get; set; }
        
        /// <summary>
        /// For numeric comparisons, the upper bound of a range
        /// </summary>
        public double? NumericValueUpper { get; set; }
        
        /// <summary>
        /// For date comparisons, the lower bound of a range
        /// </summary>
        public DateTime? DateValueLower { get; set; }
        
        /// <summary>
        /// For date comparisons, the upper bound of a range
        /// </summary>
        public DateTime? DateValueUpper { get; set; }
        
        /// <summary>
        /// For pattern matching, the compiled regular expression
        /// </summary>
        public Regex? Pattern { get; private set; }
        
        /// <summary>
        /// Indicates if the condition should be negated (NOT)
        /// </summary>
        public bool IsNegated { get; set; }
        
        /// <summary>
        /// Additional options for condition evaluation
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Creates a new condition with a generated ID
        /// </summary>
        public FileCondition()
        {
            Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// Compiles the pattern value into a regex if applicable
        /// </summary>
        public void CompilePattern()
        {
            if (Type == ConditionType.FileName || Type == ConditionType.FileContent)
            {
                if (!string.IsNullOrEmpty(Value) && Operator == ConditionOperator.Matches)
                {
                    var options = Options.ContainsKey("CaseSensitive") && 
                                 bool.TryParse(Options["CaseSensitive"], out bool caseSensitive) && 
                                 caseSensitive 
                        ? RegexOptions.None 
                        : RegexOptions.IgnoreCase;
                    
                    Pattern = new Regex(Value, options);
                }
            }
        }
    }
} 