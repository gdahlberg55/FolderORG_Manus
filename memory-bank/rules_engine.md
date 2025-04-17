# Rules Engine: FolderORG Manus

## Overview
The Rules Engine is a core component of the FolderORG Manus system, providing powerful and flexible file organization capabilities based on customizable conditions and actions. It evaluates files against user-defined rules and automatically organizes them according to specified criteria, handling conflicts and ensuring safe operation.

## Key Features
- **Condition-Based Evaluation**: Rules match files based on various properties (name, extension, size, dates, content, etc.)
- **Flexible Actions**: Multiple action types for file organization (move, copy, rename, delete, etc.)
- **Rule Grouping**: Logical combinations of rules with AND/OR relationships
- **Conflict Resolution**: Automatic handling of conflicting actions
- **Templating System**: Pre-defined templates for common organization scenarios
- **JSON Storage**: Persistent storage of rules with efficient serialization
- **Asynchronous Operation**: Non-blocking execution for responsive UI
- **Builder API**: Fluent interface for programmatic rule creation
- **Performance Optimization**: Efficient evaluation for large rule sets
- **Path Validation**: Comprehensive validation of target paths
- **Variable Resolution**: Support for dynamic path components

## Architecture

### Domain Models
- **RuleDefinition**: Core entity representing a rule with conditions and actions
- **FileCondition**: Represents a condition to evaluate against a file
- **ConditionType**: Enum defining the types of conditions (FileName, FileExtension, FileSize, etc.)
- **ConditionOperator**: Enum defining operators for conditions (Equals, Contains, GreaterThan, etc.)
- **FolderAction**: Represents an action to perform when conditions are met
- **ActionType**: Enum defining the types of actions (Move, Copy, Rename, etc.)
- **RuleGroup**: Logical grouping of rules with AND/OR relationships
- **LogicalOperator**: Enum defining logical operators (And, Or, NotAnd, NotOr)
- **ConflictResolutionStrategy**: Enum defining strategies for resolving rule conflicts
- **ConflictHandlingStrategy**: Enum defining strategies for handling file naming conflicts
- **PathVariable**: Represents a variable for dynamic path resolution
- **RuleExecutionContext**: Context for rule execution with environment information

### Services
- **IRuleEvaluationService**: Interface for rule evaluation against files
- **RuleEvaluationService**: Implementation of rule evaluation logic
- **IRuleRepository**: Interface for rule storage and retrieval
- **JsonRuleRepository**: JSON-based implementation of rule repository
- **IPathValidationService**: Interface for path validation
- **PathValidationService**: Implementation of path validation logic
- **IRuleFactory**: Interface for rule creation
- **RuleFactory**: Factory implementation for rule creation

### Application Layer
- **RuleEngine**: Coordinates rule evaluation and action execution
- **RuleBuilder**: Fluent API for building rules programmatically
- **RuleTemplateManager**: Manages pre-defined rule templates
- **RuleValidationService**: Validates rule definitions for consistency

## Implementation Details

### Rule Definition Structure
```csharp
public class RuleDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; }
    public List<FileCondition> Conditions { get; set; }
    public List<FolderAction> Actions { get; set; }
    public ConflictResolutionStrategy ConflictStrategy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public DateTime? LastExecutedDate { get; set; }
    public int ExecutionCount { get; set; }
    public string Category { get; set; }
    public Dictionary<string, string> Variables { get; set; }
    public bool IsTemplate { get; set; }
    public ExecutionStatistics Statistics { get; set; }
}
```

### Condition Evaluation
Each condition is evaluated based on its type and operator. The RuleEvaluationService implements specialized evaluation methods for each condition type:
- File name/extension evaluation
- File size evaluation
- Date-based evaluation
- Content-based evaluation
- File attribute evaluation
- Regular expression pattern matching
- MIME type detection

All evaluations are performed asynchronously to maintain UI responsiveness. Optimizations include:
- Condition short-circuiting for logical operators
- Caching of repeated evaluations
- Parallel evaluation where appropriate
- Pre-filtering of rules based on common conditions

### Action Execution
Actions are executed after conditions are evaluated and conflicts are resolved. Key considerations:
- Path resolution with variable substitution
- Directory creation if needed
- Conflict handling strategies
- Safe file operations with error handling
- Rollback capability for failed operations
- Memory Bank recording for tracking
- Performance measurements for optimization

### Conflict Resolution
The Rules Engine implements conflict resolution in two areas:
1. **Rule Conflicts**: When multiple rules match the same file but require incompatible actions
2. **File Conflicts**: When file operations would result in naming conflicts

Resolution strategies include:
- Priority-based resolution (higher priority wins)
- Type-based resolution (one move action, multiple other actions)
- User-selectable conflict handling (overwrite, rename, skip)
- Chain-of-responsibility pattern for complex resolution
- Custom conflict handler registration
- User notification for manual resolution

### Storage Model
Rules are stored in JSON format with the following considerations:
- Thread safety with SemaphoreSlim
- Efficient serialization/deserialization
- Error handling and recovery
- In-memory caching for performance
- Incremental updates for large rule sets
- Versioning for backward compatibility
- Import/export functionality

### Path Validation
The Rules Engine integrates with the Path Validation System to ensure target paths are valid:
- Variable resolution using tokens
- Path existence verification
- Permission checking
- Path normalization
- Redundancy elimination
- Directory nesting validation
- Error reporting and suggestions

### Variable System
The Rules Engine supports a rich variable system for dynamic path resolution:
- Environment variables
- User-defined variables
- Date/time-based tokens
- File metadata tokens
- Expression-based computed values
- Custom variable providers
- Variable scope management

### Template System
Pre-defined templates provide common organization patterns:
- Document organization (by type)
- Image organization (by date)
- Music organization (by artist/album)
- Video organization (by date)
- Downloads organization (by type)
- Project file management
- Source code organization
- Custom template creation and sharing

### Performance Optimizations
The Rules Engine includes several optimizations for handling large rule sets:
- Rule indexing for quick lookup
- Condition evaluation caching
- Parallel rule evaluation
- Fast-path conditions for common cases
- Rule set partitioning
- Lazy action creation
- Memory usage optimizations

## Usage Flow
1. Rules are created through UI or programmatically using RuleBuilder
2. Rules are stored in the repository
3. Files are evaluated against rules when:
   - New files are detected
   - User initiates manual organization
   - Scheduled organization runs
4. Matching rules generate actions
5. Conflicts are resolved
6. Actions are executed
7. Results are recorded in the Memory Bank

## Integration Points
- **Classification Engine**: Uses classification results as rule conditions
- **Memory Bank**: Records rule executions and organization history
- **File Operation Service**: Executes actions generated by rules
- **UI Components**: Provides management interface for rules
- **Path Validation System**: Validates rule target paths

## Error Handling
- **Rule Validation Errors**: Inconsistent or invalid rule definitions
- **Condition Evaluation Errors**: Problems with condition evaluation
- **Path Resolution Errors**: Issues with resolving variable paths
- **File Operation Errors**: Problems during file operations
- **Conflict Resolution Errors**: Unresolvable conflicts
- **Repository Access Errors**: Issues with rule storage/retrieval

## Future Enhancements
- Advanced content-based conditions using ML.NET
- Script-based custom conditions and actions
- Cloud-based rule sharing
- Rule suggestions based on usage patterns
- Rule effectiveness analytics
- Performance profiling and optimization
- Enhanced conflict resolution strategies
- Improved variable system with expressions 