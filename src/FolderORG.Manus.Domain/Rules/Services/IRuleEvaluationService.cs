using System.Collections.Generic;
using System.Threading.Tasks;
using FolderORG.Manus.Domain.Rules.Models;

namespace FolderORG.Manus.Domain.Rules.Services
{
    /// <summary>
    /// Service for evaluating rules against files
    /// </summary>
    public interface IRuleEvaluationService
    {
        /// <summary>
        /// Evaluates a file against a single rule
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="rule">The rule to evaluate</param>
        /// <returns>True if the rule conditions are met, false otherwise</returns>
        Task<bool> EvaluateRuleAsync(string filePath, RuleDefinition rule);
        
        /// <summary>
        /// Evaluates a file against a rule group
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="ruleGroup">The rule group to evaluate</param>
        /// <returns>True if the rule group conditions are met, false otherwise</returns>
        Task<bool> EvaluateRuleGroupAsync(string filePath, RuleGroup ruleGroup);
        
        /// <summary>
        /// Evaluates a collection of rules against a file and returns matching rules
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="rules">Collection of rules to evaluate</param>
        /// <returns>Collection of rules that match the file</returns>
        Task<IEnumerable<RuleDefinition>> GetMatchingRulesAsync(string filePath, IEnumerable<RuleDefinition> rules);
        
        /// <summary>
        /// Generates a set of actions to be performed based on matching rules
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="rules">Collection of rules to evaluate</param>
        /// <returns>Collection of actions to be performed on the file</returns>
        Task<IEnumerable<FolderAction>> GenerateActionsAsync(string filePath, IEnumerable<RuleDefinition> rules);
        
        /// <summary>
        /// Resolves conflicts between multiple actions
        /// </summary>
        /// <param name="actions">Collection of potentially conflicting actions</param>
        /// <returns>Collection of resolved actions</returns>
        Task<IEnumerable<FolderAction>> ResolveActionConflictsAsync(IEnumerable<FolderAction> actions);
    }
} 