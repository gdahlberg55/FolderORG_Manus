# Path Validation System

## Overview
The Path Validation System provides comprehensive file and directory path validation with robust support for path normalization, variable resolution, and permission checking. It is a critical component for the Rules Engine and File Operations modules, ensuring safe and reliable file system operations.

## Architecture
The Path Validation System follows Clean Architecture principles with clear separation of concerns:

- **Core Layer**: Contains interfaces and models
  - `IPathValidator` interface
  - `PathValidationContext` class
  - `ValidationResult` class
  - `ValidationIssue` class
  - `ValidationSeverity` enum

- **Domain Layer**: Contains implementation and extensions
  - `PathValidator` class
  - `PathValidatorExtensions` static class

- **Infrastructure Layer**: Contains dependency registration
  - Service registration in DI container

## Features

### Path Validation
- Format validation (drive letters, UNC paths)
- Character validation (invalid characters detection)
- Length validation (standard and long path support)
- Reserved name validation (CON, PRN, etc.)
- Segment validation (periods, spaces at end)
- Existence validation
- Parent directory verification
- File/directory type checking

### Path Normalization
- Redundant segment removal (., ..)
- Path separator standardization
- Forward/backslash conversion
- Trailing separator handling
- Network share preservation
- Relative path resolution

### Variable Resolution
- Environment variable substitution (`%VARIABLE%`)
- Custom variable substitution (`${VARIABLE}`)
- Multiple variable handling
- Context-based variables

### Permission Checking
- Read permission validation
- Write permission validation
- ACL-based permission checking
- Directory permissions inheritance
- Parent directory permission fallback

### Directory Creation
- Just-in-time directory creation
- Parent directory creation
- Creation verification

## Implementation Details

### Key Components

#### IPathValidator Interface
Defines the contract for path validation operations:
```csharp
public interface IPathValidator
{
    Task<ValidationResult> ValidatePathAsync(string path, PathValidationContext context, CancellationToken cancellationToken = default);
    ValidationResult ValidatePath(string path, PathValidationContext context);
    string NormalizePath(string path, PathValidationContext context);
    string ResolveVariables(string path, PathValidationContext context);
    Task<ValidationResult> CheckPermissionsAsync(string path, PathValidationContext context, CancellationToken cancellationToken = default);
}
```

#### PathValidationContext
Contains settings and configuration for path validation:
```csharp
public class PathValidationContext
{
    public bool AllowLongPaths { get; set; } = true;
    public bool CheckExistence { get; set; } = true;
    public bool CheckWritePermissions { get; set; } = true;
    public bool CheckReadPermissions { get; set; } = true;
    public bool CreateDirectories { get; set; } = false;
    public string BaseDirectory { get; set; }
    public int MaxPathLength { get; set; } = 260;
    public bool ResolveEnvironmentVariables { get; set; } = true;
    public bool NormalizePath { get; set; } = true;
    public bool RequireExistingPath { get; set; } = false;
    public bool ExpectFile { get; set; } = false;
    public bool ExpectDirectory { get; set; } = false;
    public Dictionary<string, string> Variables { get; set; }
    
    public PathValidationContext Clone();
    public void SetVariable(string name, string value);
}
```

#### ValidationResult
Represents the outcome of validation operations:
```csharp
public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationIssue> Issues { get; }
    public string NormalizedPath { get; set; }
    
    public void AddIssue(string message, ValidationSeverity severity, string code = null);
    public static ValidationResult Success(string normalizedPath);
    public static ValidationResult Failure(string message, string code = null);
    public void MergeWith(ValidationResult other);
}
```

#### PathValidator
Implements the validation logic with robust handling of various path formats, Windows/Unix differences, and file system particularities.

#### PathValidatorExtensions
Provides convenience methods for common validation scenarios:
- ValidateFileExistsAsync
- ValidateDirectoryExistsAsync
- ValidateWritablePathAsync
- EnsureDirectoryExistsAsync
- NormalizeAndResolvePath
- IsValidPathFormat

### Error Handling
The Path Validation System provides detailed error reporting with:
- Error codes for categorization
- Severity levels (Information, Warning, Error)
- Contextual error messages
- Fallback strategies
- Exception safety

### Variable Resolution System
Supports two main formats:
1. Environment variables: `%VARIABLE%`
2. Custom variables: `${VARIABLE}`

Variables are resolved in a deterministic order, with custom variables taking precedence over environment variables.

### Platform-Specific Handling
The implementation handles platform-specific path requirements:
- Windows drive letters and UNC paths
- Path separators (forward/backslash)
- Reserved device names (CON, NUL, etc.)
- Maximum path lengths (260 characters limit on Windows)
- Long path support (via context configuration)

## Integration Points

### Rules Engine Integration
The Path Validation System is used by the Rules Engine to validate target paths for file operations, ensuring that rules don't attempt to move files to invalid or inaccessible locations.

### File Operations Integration
Before performing any file or directory operation, the Path Validation System verifies the operation's safety and feasibility.

### User Interface Integration
Path validation results are presented to users in a friendly format, with clear error messages and suggested corrections.

## Performance Considerations
- Efficient path normalization for minimizing redundant operations
- Lazy validation (only validating what's required)
- Caching of filesystem access results
- Asynchronous validation for responsive UI
- Cancellation support for long-running operations

## Testing
Comprehensive unit tests cover:
- Path normalization
- Variable resolution
- Format validation
- Character validation
- Length validation
- Path segment validation
- Permission checking
- Directory creation

## Future Improvements
- Implement transaction-based operations with rollback capability
- Add support for additional variable formats
- Enhance performance for batch validation
- Implement more detailed permission reporting
- Add support for custom validation rules 