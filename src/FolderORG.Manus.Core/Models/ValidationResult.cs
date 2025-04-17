using System;
using System.Collections.Generic;
using System.Linq;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid => !_issues.Any(i => i.Severity == ValidationSeverity.Error);

        /// <summary>
        /// Gets a list of validation issues.
        /// </summary>
        public IReadOnlyList<ValidationIssue> Issues => _issues.AsReadOnly();

        /// <summary>
        /// Gets or sets the normalized path after validation.
        /// </summary>
        public string NormalizedPath { get; set; }

        /// <summary>
        /// Adds a validation issue.
        /// </summary>
        /// <param name="message">The issue message.</param>
        /// <param name="severity">The issue severity.</param>
        /// <param name="code">Optional issue code for categorization.</param>
        public void AddIssue(string message, ValidationSeverity severity, string code = null)
        {
            _issues.Add(new ValidationIssue(message, severity, code));
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <param name="normalizedPath">The normalized path.</param>
        /// <returns>A successful validation result.</returns>
        public static ValidationResult Success(string normalizedPath)
        {
            return new ValidationResult { NormalizedPath = normalizedPath };
        }

        /// <summary>
        /// Creates a failure validation result.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed validation result.</returns>
        public static ValidationResult Failure(string message, string code = null)
        {
            var result = new ValidationResult();
            result.AddIssue(message, ValidationSeverity.Error, code);
            return result;
        }

        /// <summary>
        /// Merges another validation result into this one.
        /// </summary>
        /// <param name="other">The validation result to merge.</param>
        public void MergeWith(ValidationResult other)
        {
            if (other == null) return;

            foreach (var issue in other.Issues)
            {
                _issues.Add(issue);
            }

            // Only update normalized path if it's not already set
            if (string.IsNullOrEmpty(NormalizedPath) && !string.IsNullOrEmpty(other.NormalizedPath))
            {
                NormalizedPath = other.NormalizedPath;
            }
        }
    }

    /// <summary>
    /// Represents a validation issue.
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Gets the message describing the issue.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the severity of the issue.
        /// </summary>
        public ValidationSeverity Severity { get; }

        /// <summary>
        /// Gets the code identifying the type of issue.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationIssue class.
        /// </summary>
        /// <param name="message">The issue message.</param>
        /// <param name="severity">The issue severity.</param>
        /// <param name="code">The issue code.</param>
        public ValidationIssue(string message, ValidationSeverity severity, string code = null)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Severity = severity;
            Code = code;
        }
    }

    /// <summary>
    /// Defines the severity levels for validation issues.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Information that doesn't affect validity.
        /// </summary>
        Information,

        /// <summary>
        /// Warning that doesn't invalidate the path but suggests potential issues.
        /// </summary>
        Warning,

        /// <summary>
        /// Error that invalidates the path.
        /// </summary>
        Error
    }
} 