using System;

namespace FolderORG.Manus.Core.Models
{
    /// <summary>
    /// Represents an issue found during path validation.
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Gets the error code that uniquely identifies this issue type.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the message describing the issue.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the severity level of the issue.
        /// </summary>
        public ValidationSeverity Severity { get; }

        /// <summary>
        /// Gets the path segment or part that caused the issue.
        /// </summary>
        public string? AffectedSegment { get; }

        /// <summary>
        /// Gets the position in the path where the issue occurred.
        /// </summary>
        public int? Position { get; }

        /// <summary>
        /// Gets a suggested fix for this issue, if available.
        /// </summary>
        public string? SuggestedFix { get; }

        /// <summary>
        /// Indicates if this issue can be automatically fixed.
        /// </summary>
        public bool CanAutoFix { get; }

        /// <summary>
        /// Creates a new instance of ValidationIssue.
        /// </summary>
        /// <param name="code">Error code that uniquely identifies this issue type.</param>
        /// <param name="message">Message describing the issue.</param>
        /// <param name="severity">Severity level of the issue.</param>
        /// <param name="affectedSegment">Optional path segment that caused the issue.</param>
        /// <param name="position">Optional position in the path where the issue occurred.</param>
        /// <param name="suggestedFix">Optional suggested fix for this issue.</param>
        /// <param name="canAutoFix">Whether this issue can be automatically fixed.</param>
        public ValidationIssue(
            string code,
            string message,
            ValidationSeverity severity,
            string? affectedSegment = null,
            int? position = null,
            string? suggestedFix = null,
            bool canAutoFix = false)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Severity = severity;
            AffectedSegment = affectedSegment;
            Position = position;
            SuggestedFix = suggestedFix;
            CanAutoFix = canAutoFix;
        }

        /// <summary>
        /// Returns a string representation of the validation issue.
        /// </summary>
        /// <returns>A string representation of the validation issue.</returns>
        public override string ToString()
        {
            return $"[{Severity}] {Code}: {Message}" +
                   (AffectedSegment != null ? $" - Segment: '{AffectedSegment}'" : "") +
                   (Position.HasValue ? $" at position {Position}" : "") +
                   (SuggestedFix != null ? $" - Suggestion: {SuggestedFix}" : "");
        }
    }
} 