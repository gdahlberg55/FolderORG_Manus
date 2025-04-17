using System;
using System.Collections.Generic;

namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Represents a rule definition that can be applied to files for organization
    /// </summary>
    public class RuleDefinition
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Human-readable name for the rule
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed description of the rule's purpose
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Execution priority (higher numbers have higher priority)
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Indicates if the rule is currently active
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Collection of conditions that must be met for the rule to apply
        /// </summary>
        public List<FileCondition> Conditions { get; set; } = new List<FileCondition>();
        
        /// <summary>
        /// Collection of actions to perform when conditions are met
        /// </summary>
        public List<FolderAction> Actions { get; set; } = new List<FolderAction>();
        
        /// <summary>
        /// Strategy to use when rule conflicts occur
        /// </summary>
        public ConflictResolutionStrategy ConflictStrategy { get; set; } = ConflictResolutionStrategy.Priority;
        
        /// <summary>
        /// The date when this rule was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// The date when this rule was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// The date when this rule was last executed
        /// </summary>
        public DateTime? LastExecutedDate { get; set; }
        
        /// <summary>
        /// The number of times this rule has been successfully applied
        /// </summary>
        public int ExecutionCount { get; set; }
        
        /// <summary>
        /// Optional category or tag for organizing rules
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Creates a new rule definition with a generated ID
        /// </summary>
        public RuleDefinition()
        {
            Id = Guid.NewGuid();
        }
    }
} 