using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FolderORG.Manus.Domain.Rules.Models;
using FolderORG.Manus.Domain.Rules.Repository;

namespace FolderORG.Manus.Infrastructure.Rules.Repository
{
    /// <summary>
    /// JSON-based implementation of rule repository
    /// </summary>
    public class JsonRuleRepository : IRuleRepository
    {
        private readonly string _rulesFilePath;
        private readonly string _ruleGroupsFilePath;
        private readonly SemaphoreSlim _ruleLock = new(1, 1);
        private readonly SemaphoreSlim _groupLock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;
        
        private List<RuleDefinition> _rules = new();
        private List<RuleGroup> _ruleGroups = new();
        
        /// <summary>
        /// Creates a new JSON-based rule repository
        /// </summary>
        /// <param name="basePath">Base directory for storage</param>
        public JsonRuleRepository(string basePath)
        {
            _rulesFilePath = Path.Combine(basePath, "rules.json");
            _ruleGroupsFilePath = Path.Combine(basePath, "rule_groups.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            // Ensure the directory exists
            Directory.CreateDirectory(basePath);
            
            // Initialize the repository
            InitializeAsync().GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Gets all rules
        /// </summary>
        public async Task<IEnumerable<RuleDefinition>> GetAllRulesAsync()
        {
            await _ruleLock.WaitAsync();
            try
            {
                return _rules.ToList(); // Return a copy
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Gets a rule by its ID
        /// </summary>
        public async Task<RuleDefinition?> GetRuleByIdAsync(Guid id)
        {
            await _ruleLock.WaitAsync();
            try
            {
                return _rules.FirstOrDefault(r => r.Id == id);
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Gets rules by category
        /// </summary>
        public async Task<IEnumerable<RuleDefinition>> GetRulesByCategoryAsync(string category)
        {
            await _ruleLock.WaitAsync();
            try
            {
                return _rules.Where(r => r.Category == category).ToList();
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Gets enabled rules
        /// </summary>
        public async Task<IEnumerable<RuleDefinition>> GetEnabledRulesAsync()
        {
            await _ruleLock.WaitAsync();
            try
            {
                return _rules.Where(r => r.IsEnabled).ToList();
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Adds a new rule
        /// </summary>
        public async Task<RuleDefinition> AddRuleAsync(RuleDefinition rule)
        {
            await _ruleLock.WaitAsync();
            try
            {
                // Ensure ID is set
                if (rule.Id == Guid.Empty)
                {
                    rule.Id = Guid.NewGuid();
                }
                
                // Set timestamps
                rule.CreatedDate = DateTime.Now;
                rule.ModifiedDate = DateTime.Now;
                
                _rules.Add(rule);
                await SaveRulesAsync();
                
                return rule;
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Updates an existing rule
        /// </summary>
        public async Task<bool> UpdateRuleAsync(RuleDefinition rule)
        {
            await _ruleLock.WaitAsync();
            try
            {
                var existingRule = _rules.FirstOrDefault(r => r.Id == rule.Id);
                if (existingRule == null)
                    return false;
                
                // Update the rule
                var index = _rules.IndexOf(existingRule);
                rule.ModifiedDate = DateTime.Now;
                rule.CreatedDate = existingRule.CreatedDate; // Preserve creation date
                _rules[index] = rule;
                
                await SaveRulesAsync();
                return true;
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Deletes a rule by its ID
        /// </summary>
        public async Task<bool> DeleteRuleAsync(Guid id)
        {
            await _ruleLock.WaitAsync();
            try
            {
                var existingRule = _rules.FirstOrDefault(r => r.Id == id);
                if (existingRule == null)
                    return false;
                
                _rules.Remove(existingRule);
                await SaveRulesAsync();
                return true;
            }
            finally
            {
                _ruleLock.Release();
            }
        }
        
        /// <summary>
        /// Gets all rule groups
        /// </summary>
        public async Task<IEnumerable<RuleGroup>> GetAllRuleGroupsAsync()
        {
            await _groupLock.WaitAsync();
            try
            {
                return _ruleGroups.ToList(); // Return a copy
            }
            finally
            {
                _groupLock.Release();
            }
        }
        
        /// <summary>
        /// Gets a rule group by its ID
        /// </summary>
        public async Task<RuleGroup?> GetRuleGroupByIdAsync(Guid id)
        {
            await _groupLock.WaitAsync();
            try
            {
                return _ruleGroups.FirstOrDefault(g => g.Id == id);
            }
            finally
            {
                _groupLock.Release();
            }
        }
        
        /// <summary>
        /// Adds a new rule group
        /// </summary>
        public async Task<RuleGroup> AddRuleGroupAsync(RuleGroup group)
        {
            await _groupLock.WaitAsync();
            try
            {
                // Ensure ID is set
                if (group.Id == Guid.Empty)
                {
                    group.Id = Guid.NewGuid();
                }
                
                _ruleGroups.Add(group);
                await SaveRuleGroupsAsync();
                
                return group;
            }
            finally
            {
                _groupLock.Release();
            }
        }
        
        /// <summary>
        /// Updates an existing rule group
        /// </summary>
        public async Task<bool> UpdateRuleGroupAsync(RuleGroup group)
        {
            await _groupLock.WaitAsync();
            try
            {
                var existingGroup = _ruleGroups.FirstOrDefault(g => g.Id == group.Id);
                if (existingGroup == null)
                    return false;
                
                // Update the group
                var index = _ruleGroups.IndexOf(existingGroup);
                _ruleGroups[index] = group;
                
                await SaveRuleGroupsAsync();
                return true;
            }
            finally
            {
                _groupLock.Release();
            }
        }
        
        /// <summary>
        /// Deletes a rule group by its ID
        /// </summary>
        public async Task<bool> DeleteRuleGroupAsync(Guid id)
        {
            await _groupLock.WaitAsync();
            try
            {
                var existingGroup = _ruleGroups.FirstOrDefault(g => g.Id == id);
                if (existingGroup == null)
                    return false;
                
                _ruleGroups.Remove(existingGroup);
                await SaveRuleGroupsAsync();
                return true;
            }
            finally
            {
                _groupLock.Release();
            }
        }
        
        /// <summary>
        /// Initializes the repository, loading existing rules
        /// </summary>
        private async Task InitializeAsync()
        {
            // Load rules
            await _ruleLock.WaitAsync();
            try
            {
                if (File.Exists(_rulesFilePath))
                {
                    var json = await File.ReadAllTextAsync(_rulesFilePath);
                    var rules = JsonSerializer.Deserialize<List<RuleDefinition>>(json, _jsonOptions);
                    if (rules != null)
                    {
                        _rules = rules;
                    }
                }
            }
            catch (Exception)
            {
                // If loading fails, start with an empty set
                _rules = new List<RuleDefinition>();
            }
            finally
            {
                _ruleLock.Release();
            }
            
            // Load rule groups
            await _groupLock.WaitAsync();
            try
            {
                if (File.Exists(_ruleGroupsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_ruleGroupsFilePath);
                    var groups = JsonSerializer.Deserialize<List<RuleGroup>>(json, _jsonOptions);
                    if (groups != null)
                    {
                        _ruleGroups = groups;
                    }
                }
            }
            catch (Exception)
            {
                // If loading fails, start with an empty set
                _ruleGroups = new List<RuleGroup>();
            }
            finally
            {
                _groupLock.Release();
            }
        }
        
        /// <summary>
        /// Saves rules to the JSON file
        /// </summary>
        private async Task SaveRulesAsync()
        {
            var json = JsonSerializer.Serialize(_rules, _jsonOptions);
            await File.WriteAllTextAsync(_rulesFilePath, json);
        }
        
        /// <summary>
        /// Saves rule groups to the JSON file
        /// </summary>
        private async Task SaveRuleGroupsAsync()
        {
            var json = JsonSerializer.Serialize(_ruleGroups, _jsonOptions);
            await File.WriteAllTextAsync(_ruleGroupsFilePath, json);
        }
    }
} 