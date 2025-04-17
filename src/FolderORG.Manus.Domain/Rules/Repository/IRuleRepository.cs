using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FolderORG.Manus.Domain.Rules.Models;

namespace FolderORG.Manus.Domain.Rules.Repository
{
    /// <summary>
    /// Repository for storing and retrieving rules
    /// </summary>
    public interface IRuleRepository
    {
        /// <summary>
        /// Gets all rules
        /// </summary>
        /// <returns>Collection of all rule definitions</returns>
        Task<IEnumerable<RuleDefinition>> GetAllRulesAsync();
        
        /// <summary>
        /// Gets a rule by its ID
        /// </summary>
        /// <param name="id">The rule ID</param>
        /// <returns>The rule definition, or null if not found</returns>
        Task<RuleDefinition?> GetRuleByIdAsync(Guid id);
        
        /// <summary>
        /// Gets rules by category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>Collection of rules in the specified category</returns>
        Task<IEnumerable<RuleDefinition>> GetRulesByCategoryAsync(string category);
        
        /// <summary>
        /// Gets enabled rules
        /// </summary>
        /// <returns>Collection of enabled rules</returns>
        Task<IEnumerable<RuleDefinition>> GetEnabledRulesAsync();
        
        /// <summary>
        /// Adds a new rule
        /// </summary>
        /// <param name="rule">The rule to add</param>
        /// <returns>The added rule with generated ID</returns>
        Task<RuleDefinition> AddRuleAsync(RuleDefinition rule);
        
        /// <summary>
        /// Updates an existing rule
        /// </summary>
        /// <param name="rule">The rule to update</param>
        /// <returns>True if updated successfully, false if rule not found</returns>
        Task<bool> UpdateRuleAsync(RuleDefinition rule);
        
        /// <summary>
        /// Deletes a rule by its ID
        /// </summary>
        /// <param name="id">The ID of the rule to delete</param>
        /// <returns>True if deleted successfully, false if rule not found</returns>
        Task<bool> DeleteRuleAsync(Guid id);
        
        /// <summary>
        /// Gets all rule groups
        /// </summary>
        /// <returns>Collection of all rule groups</returns>
        Task<IEnumerable<RuleGroup>> GetAllRuleGroupsAsync();
        
        /// <summary>
        /// Gets a rule group by its ID
        /// </summary>
        /// <param name="id">The group ID</param>
        /// <returns>The rule group, or null if not found</returns>
        Task<RuleGroup?> GetRuleGroupByIdAsync(Guid id);
        
        /// <summary>
        /// Adds a new rule group
        /// </summary>
        /// <param name="group">The group to add</param>
        /// <returns>The added group with generated ID</returns>
        Task<RuleGroup> AddRuleGroupAsync(RuleGroup group);
        
        /// <summary>
        /// Updates an existing rule group
        /// </summary>
        /// <param name="group">The group to update</param>
        /// <returns>True if updated successfully, false if group not found</returns>
        Task<bool> UpdateRuleGroupAsync(RuleGroup group);
        
        /// <summary>
        /// Deletes a rule group by its ID
        /// </summary>
        /// <param name="id">The ID of the group to delete</param>
        /// <returns>True if deleted successfully, false if group not found</returns>
        Task<bool> DeleteRuleGroupAsync(Guid id);
    }
} 