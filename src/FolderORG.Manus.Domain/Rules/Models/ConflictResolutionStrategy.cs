namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Defines strategies for resolving conflicts between rules
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        /// <summary>
        /// Use rule priority to determine which rule to apply
        /// </summary>
        Priority = 0,
        
        /// <summary>
        /// Use the most recently created rule
        /// </summary>
        MostRecent = 1,
        
        /// <summary>
        /// Use the most specific rule (with most conditions)
        /// </summary>
        MostSpecific = 2,
        
        /// <summary>
        /// Request user input for resolution
        /// </summary>
        AskUser = 3,
        
        /// <summary>
        /// Apply both rules (if possible)
        /// </summary>
        ApplyBoth = 4,
        
        /// <summary>
        /// Skip both rules when a conflict is detected
        /// </summary>
        SkipBoth = 5
    }
} 