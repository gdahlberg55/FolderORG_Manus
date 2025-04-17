using System;
using System.Collections.Generic;
using FolderORG.Manus.Domain.Rules.Models;

namespace FolderORG.Manus.Domain.Rules.Builder
{
    /// <summary>
    /// Fluent builder for creating rule definitions
    /// </summary>
    public class RuleBuilder
    {
        private readonly RuleDefinition _rule;
        
        /// <summary>
        /// Creates a new rule builder
        /// </summary>
        public RuleBuilder()
        {
            _rule = new RuleDefinition();
        }
        
        /// <summary>
        /// Sets the name of the rule
        /// </summary>
        /// <param name="name">Rule name</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithName(string name)
        {
            _rule.Name = name;
            return this;
        }
        
        /// <summary>
        /// Sets the description of the rule
        /// </summary>
        /// <param name="description">Rule description</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithDescription(string description)
        {
            _rule.Description = description;
            return this;
        }
        
        /// <summary>
        /// Sets the priority of the rule
        /// </summary>
        /// <param name="priority">Rule priority</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithPriority(int priority)
        {
            _rule.Priority = priority;
            return this;
        }
        
        /// <summary>
        /// Sets the category of the rule
        /// </summary>
        /// <param name="category">Rule category</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithCategory(string category)
        {
            _rule.Category = category;
            return this;
        }
        
        /// <summary>
        /// Sets the enabled state of the rule
        /// </summary>
        /// <param name="isEnabled">Whether the rule is enabled</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder IsEnabled(bool isEnabled)
        {
            _rule.IsEnabled = isEnabled;
            return this;
        }
        
        /// <summary>
        /// Sets the conflict resolution strategy
        /// </summary>
        /// <param name="strategy">Conflict resolution strategy</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithConflictStrategy(ConflictResolutionStrategy strategy)
        {
            _rule.ConflictStrategy = strategy;
            return this;
        }
        
        /// <summary>
        /// Adds a condition to the rule
        /// </summary>
        /// <param name="condition">The condition to add</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithCondition(FileCondition condition)
        {
            _rule.Conditions.Add(condition);
            return this;
        }
        
        /// <summary>
        /// Adds a file name condition
        /// </summary>
        /// <param name="pattern">The file name pattern</param>
        /// <param name="op">The comparison operator</param>
        /// <param name="isNegated">Whether to negate the condition</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithFileNameCondition(string pattern, ConditionOperator op = ConditionOperator.Matches, bool isNegated = false)
        {
            var condition = new FileCondition
            {
                Type = ConditionType.FileName,
                Operator = op,
                Value = pattern,
                IsNegated = isNegated
            };
            
            condition.CompilePattern();
            _rule.Conditions.Add(condition);
            return this;
        }
        
        /// <summary>
        /// Adds a file extension condition
        /// </summary>
        /// <param name="extension">The file extension to match</param>
        /// <param name="isNegated">Whether to negate the condition</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithFileExtensionCondition(string extension, bool isNegated = false)
        {
            var condition = new FileCondition
            {
                Type = ConditionType.FileExtension,
                Operator = ConditionOperator.Equals,
                Value = extension.StartsWith(".") ? extension : $".{extension}",
                IsNegated = isNegated
            };
            
            _rule.Conditions.Add(condition);
            return this;
        }
        
        /// <summary>
        /// Adds a file size condition
        /// </summary>
        /// <param name="size">The file size in bytes</param>
        /// <param name="op">The comparison operator</param>
        /// <param name="isNegated">Whether to negate the condition</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithFileSizeCondition(long size, ConditionOperator op = ConditionOperator.GreaterThan, bool isNegated = false)
        {
            var condition = new FileCondition
            {
                Type = ConditionType.FileSize,
                Operator = op,
                NumericValueLower = size,
                IsNegated = isNegated
            };
            
            _rule.Conditions.Add(condition);
            return this;
        }
        
        /// <summary>
        /// Adds a file size range condition
        /// </summary>
        /// <param name="minSize">Minimum file size in bytes</param>
        /// <param name="maxSize">Maximum file size in bytes</param>
        /// <param name="isNegated">Whether to negate the condition</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithFileSizeRangeCondition(long minSize, long maxSize, bool isNegated = false)
        {
            var condition = new FileCondition
            {
                Type = ConditionType.FileSize,
                Operator = ConditionOperator.Between,
                NumericValueLower = minSize,
                NumericValueUpper = maxSize,
                IsNegated = isNegated
            };
            
            _rule.Conditions.Add(condition);
            return this;
        }
        
        /// <summary>
        /// Adds an action to the rule
        /// </summary>
        /// <param name="action">The action to add</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithAction(FolderAction action)
        {
            _rule.Actions.Add(action);
            return this;
        }
        
        /// <summary>
        /// Adds a move action to the rule
        /// </summary>
        /// <param name="targetPath">The target path to move files to</param>
        /// <param name="createDirectory">Whether to create the directory if it doesn't exist</param>
        /// <param name="conflictHandling">How to handle naming conflicts</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithMoveAction(string targetPath, bool createDirectory = true, ConflictHandlingStrategy conflictHandling = ConflictHandlingStrategy.AppendNumber)
        {
            var action = new FolderAction
            {
                Type = ActionType.Move,
                TargetPath = targetPath,
                CreateDirectory = createDirectory,
                ConflictHandling = conflictHandling
            };
            
            _rule.Actions.Add(action);
            return this;
        }
        
        /// <summary>
        /// Adds a copy action to the rule
        /// </summary>
        /// <param name="targetPath">The target path to copy files to</param>
        /// <param name="createDirectory">Whether to create the directory if it doesn't exist</param>
        /// <param name="conflictHandling">How to handle naming conflicts</param>
        /// <returns>The builder for method chaining</returns>
        public RuleBuilder WithCopyAction(string targetPath, bool createDirectory = true, ConflictHandlingStrategy conflictHandling = ConflictHandlingStrategy.AppendNumber)
        {
            var action = new FolderAction
            {
                Type = ActionType.Copy,
                TargetPath = targetPath,
                CreateDirectory = createDirectory,
                ConflictHandling = conflictHandling
            };
            
            _rule.Actions.Add(action);
            return this;
        }
        
        /// <summary>
        /// Builds the rule definition
        /// </summary>
        /// <returns>The completed rule definition</returns>
        public RuleDefinition Build()
        {
            return _rule;
        }
    }
} 