namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Defines logical operators for combining conditions or rules
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// All conditions/rules must be true (logical AND)
        /// </summary>
        And = 0,
        
        /// <summary>
        /// At least one condition/rule must be true (logical OR)
        /// </summary>
        Or = 1,
        
        /// <summary>
        /// All conditions/rules must be false (logical NAND)
        /// </summary>
        NotAnd = 2,
        
        /// <summary>
        /// At least one condition/rule must be false (logical NOR)
        /// </summary>
        NotOr = 3
    }
} 