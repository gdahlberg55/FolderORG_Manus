using System;
using System.Collections.Generic;

namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Represents a logical grouping of rules with AND/OR relationships
    /// </summary>
    public class RuleGroup
    {
        /// <summary>
        /// Unique identifier for the rule group
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the rule group
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the rule group
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Logical operator to apply to the rules in this group
        /// </summary>
        public LogicalOperator Operator { get; set; } = LogicalOperator.And;
        
        /// <summary>
        /// Collection of rule definitions in this group
        /// </summary>
        public List<RuleDefinition> Rules { get; set; } = new List<RuleDefinition>();
        
        /// <summary>
        /// Nested rule groups (for complex logic)
        /// </summary>
        public List<RuleGroup> NestedGroups { get; set; } = new List<RuleGroup>();
        
        /// <summary>
        /// Whether this group is currently enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Priority of the rule group
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Category or tag for the rule group
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Creates a new rule group with a generated ID
        /// </summary>
        public RuleGroup()
        {
            Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// Adds a rule to the group
        /// </summary>
        /// <param name="rule">The rule to add</param>
        public void AddRule(RuleDefinition rule)
        {
            Rules.Add(rule);
        }
        
        /// <summary>
        /// Adds a nested group to this group
        /// </summary>
        /// <param name="group">The group to add</param>
        public void AddNestedGroup(RuleGroup group)
        {
            NestedGroups.Add(group);
        }
    }
} 