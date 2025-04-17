using System;
using System.Collections.Generic;

namespace FolderORG.Manus.Domain.Rules.Models
{
    /// <summary>
    /// Represents an action to be performed on a file when a rule is triggered
    /// </summary>
    public class FolderAction
    {
        /// <summary>
        /// Unique identifier for the action
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The type of action to perform
        /// </summary>
        public ActionType Type { get; set; }
        
        /// <summary>
        /// The target path for the action (move/copy destination)
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
        
        /// <summary>
        /// The file name format for rename operations
        /// </summary>
        public string NameFormat { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether to create the target directory if it doesn't exist
        /// </summary>
        public bool CreateDirectory { get; set; } = true;
        
        /// <summary>
        /// How to handle naming conflicts
        /// </summary>
        public ConflictHandlingStrategy ConflictHandling { get; set; } = ConflictHandlingStrategy.AppendNumber;
        
        /// <summary>
        /// Whether to preserve the original file's creation date
        /// </summary>
        public bool PreserveCreationDate { get; set; } = true;
        
        /// <summary>
        /// Whether to preserve the original file's modification date
        /// </summary>
        public bool PreserveModificationDate { get; set; } = true;
        
        /// <summary>
        /// Additional options for action execution
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// A list of file attributes to set on the destination file
        /// </summary>
        public List<string> SetAttributes { get; set; } = new List<string>();
        
        /// <summary>
        /// Additional tags to apply to the file
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Creates a new action with a generated ID
        /// </summary>
        public FolderAction()
        {
            Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// Resolves variables in the target path using the provided context
        /// </summary>
        /// <param name="variables">Dictionary of variables and their values</param>
        /// <returns>Resolved path with variables replaced</returns>
        public string ResolveTargetPath(Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(TargetPath))
                return string.Empty;
                
            string result = TargetPath;
            
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }
            
            return result;
        }
    }
} 