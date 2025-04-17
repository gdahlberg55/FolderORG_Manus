using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents the result of a path validation operation.
    /// </summary>
    public class PathValidationResult
    {
        /// <summary>
        /// Gets or sets whether the path is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the normalized path. This will be filled even if validation failed.
        /// </summary>
        public string NormalizedPath { get; set; }

        /// <summary>
        /// Gets or sets the original path that was validated.
        /// </summary>
        public string OriginalPath { get; set; }

        /// <summary>
        /// Gets or sets the list of validation issues.
        /// </summary>
        public List<ValidationIssue> ValidationIssues { get; set; } = new List<ValidationIssue>();

        /// <summary>
        /// Gets whether the validation result contains warnings.
        /// </summary>
        public bool HasWarnings => ValidationIssues.Any(i => i.Severity == ValidationSeverity.Warning);

        /// <summary>
        /// Gets whether the validation result contains errors.
        /// </summary>
        public bool HasErrors => ValidationIssues.Any(i => i.Severity == ValidationSeverity.Error);

        /// <summary>
        /// Gets or sets the resolved path after variable substitution and normalization.
        /// </summary>
        public string ResolvedPath { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if the path is invalid.
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets a list of validation issues with the path.
        /// </summary>
        public List<string> ValidationIssuesList { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets suggested corrections for the path if available.
        /// </summary>
        public List<string> SuggestedCorrections { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets a value indicating whether the path was modified during validation.
        /// </summary>
        public bool WasModified => !string.IsNullOrEmpty(OriginalPath) && !string.IsNullOrEmpty(ResolvedPath) && OriginalPath != ResolvedPath;

        /// <summary>
        /// Gets or sets the fallback paths that could be used if the primary path is invalid.
        /// </summary>
        public List<string> FallbackPaths { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether a fallback path was used.
        /// </summary>
        public bool UsedFallback { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether directories were created during validation.
        /// </summary>
        public bool CreatedDirectories { get; set; }

        /// <summary>
        /// Gets or sets a collection of detailed validation issues with additional context.
        /// </summary>
        public List<ValidationIssue> DetailedIssues { get; set; } = new List<ValidationIssue>();

        /// <summary>
        /// Gets or sets any additional metadata about the validation result.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the severity level of the validation result.
        /// </summary>
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.None;

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <param name="originalPath">The original path that was validated.</param>
        /// <param name="normalizedPath">The normalized path.</param>
        /// <returns>A validation result indicating success.</returns>
        public static PathValidationResult Success(string originalPath, string normalizedPath)
        {
            return new PathValidationResult
            {
                IsValid = true,
                OriginalPath = originalPath,
                NormalizedPath = normalizedPath
            };
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        /// <param name="originalPath">The original path that was validated.</param>
        /// <param name="normalizedPath">The normalized path.</param>
        /// <param name="message">Error message describing why validation failed.</param>
        /// <returns>A validation result indicating failure.</returns>
        public static PathValidationResult Failure(string originalPath, string normalizedPath, string message)
        {
            return new PathValidationResult
            {
                IsValid = false,
                OriginalPath = originalPath,
                NormalizedPath = normalizedPath,
                ValidationIssues = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Message = message,
                        Severity = ValidationSeverity.Error
                    }
                }
            };
        }

        /// <summary>
        /// Adds a validation issue to the result.
        /// </summary>
        /// <param name="message">The validation issue message.</param>
        /// <param name="severity">The severity of the issue.</param>
        /// <param name="code">An optional error code.</param>
        public void AddIssue(string message, ValidationSeverity severity, string code = null)
        {
            ValidationIssues.Add(new ValidationIssue
            {
                Message = message,
                Severity = severity,
                Code = code
            });

            // If we add an error, the validation is automatically invalid
            if (severity == ValidationSeverity.Error)
            {
                IsValid = false;
            }
        }

        /// <summary>
        /// Returns a string representation of the validation result.
        /// </summary>
        public override string ToString()
        {
            if (IsValid && !HasWarnings)
                return $"Valid: {NormalizedPath}";

            if (IsValid)
                return $"Valid with warnings: {NormalizedPath}, Warnings: {ValidationIssues.Count}";

            return $"Invalid: {OriginalPath}, Errors: {ValidationIssues.Count(i => i.Severity == ValidationSeverity.Error)}";
        }

        /// <summary>
        /// Gets a formatted report of the validation result.
        /// </summary>
        /// <returns>A formatted string containing validation details.</returns>
        public string GetFormattedReport()
        {
            var report = new StringBuilder();
            
            report.AppendLine($"Path Validation Report for: {OriginalPath}");
            report.AppendLine($"Status: {(IsValid ? "Valid" : "Invalid")}");
            
            if (WasModified)
            {
                report.AppendLine($"Modified: Yes (Original: {OriginalPath})");
                report.AppendLine($"Resolved: {ResolvedPath}");
            }
            
            if (CreatedDirectories)
            {
                report.AppendLine("Directories were created during validation.");
            }
            
            if (UsedFallback)
            {
                report.AppendLine($"Used Fallback: Yes (Primary path was invalid)");
                report.AppendLine($"Active Path: {ResolvedPath}");
            }
            
            if (ValidationIssuesList.Count > 0)
            {
                report.AppendLine("\nValidation Issues:");
                foreach (var issue in ValidationIssuesList)
                {
                    report.AppendLine($"- {issue}");
                }
            }
            
            if (DetailedIssues.Count > 0)
            {
                report.AppendLine("\nDetailed Issues:");
                foreach (var issue in DetailedIssues)
                {
                    report.AppendLine($"- [{issue.Severity}] {issue.Message}");
                    if (!string.IsNullOrEmpty(issue.Context))
                    {
                        report.AppendLine($"  Context: {issue.Context}");
                    }
                    if (!string.IsNullOrEmpty(issue.Recommendation))
                    {
                        report.AppendLine($"  Recommendation: {issue.Recommendation}");
                    }
                }
            }
            
            if (SuggestedCorrections.Count > 0)
            {
                report.AppendLine("\nSuggested Corrections:");
                foreach (var correction in SuggestedCorrections)
                {
                    report.AppendLine($"- {correction}");
                }
            }
            
            if (FallbackPaths.Count > 0)
            {
                report.AppendLine("\nFallback Paths:");
                foreach (var fallback in FallbackPaths)
                {
                    report.AppendLine($"- {fallback}");
                }
            }
            
            return report.ToString();
        }

        /// <summary>
        /// Creates a result that uses a fallback path.
        /// </summary>
        /// <param name="originalPath">The original path that was validated.</param>
        /// <param name="fallbackPath">The fallback path that will be used.</param>
        /// <param name="reason">The reason why the fallback path is used.</param>
        /// <returns>A validation result with a fallback path.</returns>
        public static PathValidationResult WithFallback(string originalPath, string fallbackPath, string reason)
        {
            return new PathValidationResult
            {
                IsValid = true,
                OriginalPath = originalPath,
                ResolvedPath = fallbackPath,
                UsedFallback = true,
                ValidationIssues = new List<ValidationIssue> { new ValidationIssue { Message = reason, Severity = ValidationSeverity.Warning } },
                FallbackPaths = new List<string> { fallbackPath },
                Severity = ValidationSeverity.Warning
            };
        }
    }

    /// <summary>
    /// Represents the severity of a validation issue.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// No issues.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Informational message, not affecting validation.
        /// </summary>
        Information = 1,
        
        /// <summary>
        /// Warning that should be considered but doesn't invalidate the path.
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Error that invalidates the path.
        /// </summary>
        Error = 3,
        
        /// <summary>
        /// Critical error that could lead to data loss or security issues.
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// Represents an issue found during path validation.
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Gets or sets the validation issue message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the severity of the issue.
        /// </summary>
        public ValidationSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets an optional error code for the issue.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets additional context information about the issue.
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets a recommendation on how to fix the issue.
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// Gets or sets the part of the path that caused the issue.
        /// </summary>
        public string ProblematicPathPart { get; set; }

        /// <summary>
        /// Returns a string representation of the validation issue.
        /// </summary>
        public override string ToString()
        {
            return $"[{Severity}] {(Code != null ? $"({Code}) " : "")}{Message}";
        }
    }
} 