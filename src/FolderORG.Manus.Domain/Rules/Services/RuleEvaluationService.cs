using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FolderORG.Manus.Domain.Rules.Models;

namespace FolderORG.Manus.Domain.Rules.Services
{
    /// <summary>
    /// Implementation of rule evaluation service for file organization
    /// </summary>
    public class RuleEvaluationService : IRuleEvaluationService
    {
        /// <summary>
        /// Evaluates a file against a single rule
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="rule">The rule to evaluate</param>
        /// <returns>True if the rule conditions are met, false otherwise</returns>
        public async Task<bool> EvaluateRuleAsync(string filePath, RuleDefinition rule)
        {
            if (!rule.IsEnabled || rule.Conditions.Count == 0)
                return false;
                
            // All conditions must be met for the rule to apply
            foreach (var condition in rule.Conditions)
            {
                bool conditionResult = await EvaluateConditionAsync(filePath, condition);
                
                // If condition is negated, invert the result
                if (condition.IsNegated)
                    conditionResult = !conditionResult;
                    
                if (!conditionResult)
                    return false; // One condition failed, so rule doesn't apply
            }
            
            return true; // All conditions met
        }
        
        /// <summary>
        /// Evaluates a file against a rule group
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="ruleGroup">The rule group to evaluate</param>
        /// <returns>True if the rule group conditions are met, false otherwise</returns>
        public async Task<bool> EvaluateRuleGroupAsync(string filePath, RuleGroup ruleGroup)
        {
            if (!ruleGroup.IsEnabled)
                return false;
                
            List<bool> ruleResults = new List<bool>();
            
            // Evaluate all rules in the group
            foreach (var rule in ruleGroup.Rules)
            {
                bool ruleResult = await EvaluateRuleAsync(filePath, rule);
                ruleResults.Add(ruleResult);
            }
            
            // Evaluate all nested groups
            foreach (var nestedGroup in ruleGroup.NestedGroups)
            {
                bool nestedResult = await EvaluateRuleGroupAsync(filePath, nestedGroup);
                ruleResults.Add(nestedResult);
            }
            
            if (ruleResults.Count == 0)
                return false;
                
            // Apply the logical operator to the results
            switch (ruleGroup.Operator)
            {
                case LogicalOperator.And:
                    return ruleResults.All(result => result);
                    
                case LogicalOperator.Or:
                    return ruleResults.Any(result => result);
                    
                case LogicalOperator.NotAnd:
                    return !ruleResults.All(result => result);
                    
                case LogicalOperator.NotOr:
                    return !ruleResults.Any(result => result);
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Evaluates a collection of rules against a file and returns matching rules
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="rules">Collection of rules to evaluate</param>
        /// <returns>Collection of rules that match the file</returns>
        public async Task<IEnumerable<RuleDefinition>> GetMatchingRulesAsync(string filePath, IEnumerable<RuleDefinition> rules)
        {
            var matchingRules = new List<RuleDefinition>();
            
            foreach (var rule in rules.Where(r => r.IsEnabled))
            {
                if (await EvaluateRuleAsync(filePath, rule))
                {
                    matchingRules.Add(rule);
                }
            }
            
            // Sort by priority (higher numbers first)
            return matchingRules.OrderByDescending(r => r.Priority);
        }
        
        /// <summary>
        /// Generates a set of actions to be performed based on matching rules
        /// </summary>
        /// <param name="filePath">The full path to the file</param>
        /// <param name="rules">Collection of rules to evaluate</param>
        /// <returns>Collection of actions to be performed on the file</returns>
        public async Task<IEnumerable<FolderAction>> GenerateActionsAsync(string filePath, IEnumerable<RuleDefinition> rules)
        {
            var matchingRules = await GetMatchingRulesAsync(filePath, rules);
            var allActions = new List<FolderAction>();
            
            foreach (var rule in matchingRules)
            {
                allActions.AddRange(rule.Actions);
                
                // Update rule execution statistics
                rule.LastExecutedDate = DateTime.Now;
                rule.ExecutionCount++;
            }
            
            // Resolve any conflicts between actions
            return await ResolveActionConflictsAsync(allActions);
        }
        
        /// <summary>
        /// Resolves conflicts between multiple actions
        /// </summary>
        /// <param name="actions">Collection of potentially conflicting actions</param>
        /// <returns>Collection of resolved actions</returns>
        public async Task<IEnumerable<FolderAction>> ResolveActionConflictsAsync(IEnumerable<FolderAction> actions)
        {
            var resolvedActions = new List<FolderAction>();
            var moveActions = actions.Where(a => a.Type == ActionType.Move).ToList();
            var copyActions = actions.Where(a => a.Type == ActionType.Copy).ToList();
            var otherActions = actions.Where(a => a.Type != ActionType.Move && a.Type != ActionType.Copy).ToList();
            
            // Only one move action can be executed (first by priority)
            if (moveActions.Any())
            {
                resolvedActions.Add(moveActions.First());
            }
            
            // Multiple copy actions can be executed
            resolvedActions.AddRange(copyActions);
            
            // Other actions can be executed
            resolvedActions.AddRange(otherActions);
            
            return resolvedActions;
        }
        
        /// <summary>
        /// Evaluates a single condition against a file
        /// </summary>
        private async Task<bool> EvaluateConditionAsync(string filePath, FileCondition condition)
        {
            if (!File.Exists(filePath))
                return false;
                
            var fileInfo = new FileInfo(filePath);
            
            switch (condition.Type)
            {
                case ConditionType.FileName:
                    return EvaluateFileNameCondition(fileInfo.Name, condition);
                    
                case ConditionType.FileExtension:
                    return EvaluateFileExtensionCondition(fileInfo.Extension, condition);
                    
                case ConditionType.FileSize:
                    return EvaluateFileSizeCondition(fileInfo.Length, condition);
                    
                case ConditionType.CreationDate:
                    return EvaluateDateCondition(fileInfo.CreationTime, condition);
                    
                case ConditionType.ModifiedDate:
                    return EvaluateDateCondition(fileInfo.LastWriteTime, condition);
                    
                case ConditionType.AccessDate:
                    return EvaluateDateCondition(fileInfo.LastAccessTime, condition);
                    
                case ConditionType.FileContent:
                    return await EvaluateFileContentConditionAsync(filePath, condition);
                    
                case ConditionType.ParentFolder:
                    return EvaluateFileNameCondition(fileInfo.Directory?.Name ?? string.Empty, condition);
                    
                case ConditionType.FileAttributes:
                    return EvaluateFileAttributesCondition(fileInfo.Attributes, condition);
                    
                default:
                    return false;
            }
        }
        
        private bool EvaluateFileNameCondition(string fileName, FileCondition condition)
        {
            switch (condition.Operator)
            {
                case ConditionOperator.Equals:
                    return string.Equals(fileName, condition.Value, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.NotEquals:
                    return !string.Equals(fileName, condition.Value, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.Contains:
                    return fileName.Contains(condition.Value, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.StartsWith:
                    return fileName.StartsWith(condition.Value, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.EndsWith:
                    return fileName.EndsWith(condition.Value, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.Matches:
                    if (condition.Pattern == null)
                        condition.CompilePattern();
                    return condition.Pattern?.IsMatch(fileName) ?? false;
                    
                case ConditionOperator.InList:
                    var values = condition.Value.Split(',', ';').Select(v => v.Trim());
                    return values.Any(v => string.Equals(fileName, v, StringComparison.OrdinalIgnoreCase));
                    
                default:
                    return false;
            }
        }
        
        private bool EvaluateFileExtensionCondition(string extension, FileCondition condition)
        {
            // Normalize extension for comparison
            string normalizedExtension = extension.ToLowerInvariant();
            string conditionValue = condition.Value.ToLowerInvariant();
            
            if (!normalizedExtension.StartsWith(".") && !string.IsNullOrEmpty(normalizedExtension))
                normalizedExtension = "." + normalizedExtension;
                
            if (!conditionValue.StartsWith(".") && !string.IsNullOrEmpty(conditionValue))
                conditionValue = "." + conditionValue;
                
            switch (condition.Operator)
            {
                case ConditionOperator.Equals:
                    return string.Equals(normalizedExtension, conditionValue, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.NotEquals:
                    return !string.Equals(normalizedExtension, conditionValue, StringComparison.OrdinalIgnoreCase);
                    
                case ConditionOperator.InList:
                    var values = condition.Value.Split(',', ';').Select(v => v.Trim().ToLowerInvariant())
                        .Select(v => v.StartsWith(".") ? v : "." + v);
                    return values.Any(v => string.Equals(normalizedExtension, v, StringComparison.OrdinalIgnoreCase));
                    
                default:
                    return false;
            }
        }
        
        private bool EvaluateFileSizeCondition(long fileSize, FileCondition condition)
        {
            switch (condition.Operator)
            {
                case ConditionOperator.Equals:
                    return fileSize == condition.NumericValueLower;
                    
                case ConditionOperator.NotEquals:
                    return fileSize != condition.NumericValueLower;
                    
                case ConditionOperator.GreaterThan:
                    return fileSize > condition.NumericValueLower;
                    
                case ConditionOperator.LessThan:
                    return fileSize < condition.NumericValueLower;
                    
                case ConditionOperator.Between:
                    return fileSize >= condition.NumericValueLower && 
                           fileSize <= condition.NumericValueUpper;
                    
                default:
                    return false;
            }
        }
        
        private bool EvaluateDateCondition(DateTime fileDate, FileCondition condition)
        {
            switch (condition.Operator)
            {
                case ConditionOperator.Equals:
                    return fileDate.Date == condition.DateValueLower?.Date;
                    
                case ConditionOperator.NotEquals:
                    return fileDate.Date != condition.DateValueLower?.Date;
                    
                case ConditionOperator.Before:
                    return fileDate < condition.DateValueLower;
                    
                case ConditionOperator.After:
                    return fileDate > condition.DateValueLower;
                    
                case ConditionOperator.Within:
                    return fileDate >= condition.DateValueLower && 
                           fileDate <= condition.DateValueUpper;
                    
                default:
                    return false;
            }
        }
        
        private async Task<bool> EvaluateFileContentConditionAsync(string filePath, FileCondition condition)
        {
            try
            {
                // Only evaluate if the file is text-based and not too large
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 10 * 1024 * 1024) // Skip files larger than 10MB
                    return false;
                    
                string content = await File.ReadAllTextAsync(filePath);
                
                switch (condition.Operator)
                {
                    case ConditionOperator.Contains:
                        return content.Contains(condition.Value, StringComparison.OrdinalIgnoreCase);
                        
                    case ConditionOperator.Matches:
                        if (condition.Pattern == null)
                            condition.CompilePattern();
                        return condition.Pattern?.IsMatch(content) ?? false;
                        
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                // If we can't read the file or it's not text, condition doesn't match
                return false;
            }
        }
        
        private bool EvaluateFileAttributesCondition(FileAttributes attributes, FileCondition condition)
        {
            try
            {
                if (condition.Operator != ConditionOperator.HasAttribute)
                    return false;
                    
                var attributeValues = condition.Value.Split(',', ';')
                    .Select(a => a.Trim())
                    .Select(a => (FileAttributes)Enum.Parse(typeof(FileAttributes), a, true));
                    
                foreach (var attr in attributeValues)
                {
                    if ((attributes & attr) != attr)
                        return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 