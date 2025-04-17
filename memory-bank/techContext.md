# Technical Context: FolderORG Manus

## Technology Stack

### Core Platform
- **Primary Language**: C# (.NET 6.0)
- **UI Framework**: Windows Presentation Foundation (WPF)
- **Packaging**: MSIX for Windows distribution

### Key Libraries and Dependencies
- **File Operations**: System.IO.FileSystem, System.IO.FileSystem.Watcher
- **Metadata Extraction**: TagLib# (media files), DocumentFormat.OpenXml (Office docs), ExifLib (images)
- **Text Analysis**: ML.NET for content classification
- **Database**: SQLite for configuration and rule storage
- **Serialization**: System.Text.Json for configuration, memory bank, and rules storage
- **UI Components**: MaterialDesignThemes.Wpf for modern UI styling
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog for structured logging
- **Rule Engine**: Custom implementation with comprehensive condition evaluation
- **Unit Testing**: xUnit, Moq for mocking
- **Performance Benchmarking**: BenchmarkDotNet

### Development Tools
- **IDE**: Visual Studio 2022
- **Version Control**: Git
- **Build System**: MSBuild with Azure DevOps pipelines
- **Testing Framework**: xUnit, Moq for mocking
- **Code Quality**: StyleCop, FxCop
- **Documentation**: DocFX for API documentation
- **Continuous Integration**: Azure DevOps Pipelines

## Technical Architecture

### Application Layers
1. **Presentation Layer**: WPF UI components with Material Design styling
2. **Application Layer**: Workflow coordination, feature implementations
3. **Domain Layer**: Core business logic and rule processing
4. **Infrastructure Layer**: File system operations, database access, memory bank services

### Core Components
- **Classification Engine**: Manages multiple classifier implementations for categorizing files
- **Memory Bank System**: Tracks file organization history and provides statistics
- **File Operation Service**: Handles file movement, copying, and conflict resolution
- **UI Shell**: Material Design-based interface with modular components
- **Rules Engine**: Evaluates files against condition-based rules and generates actions
- **Path Validation System**: Validates rule target paths with variable resolution
- **Performance Optimization System**: Manages performance for large file operations

### Data Storage
- **User Settings**: JSON configuration files in AppData
- **Rule Definitions**: JSON files for rule storage and retrieval
- **Memory Bank**: JSON-based storage of file organization history
- **Operation History**: Transaction log files for backup/restore
- **File Metadata Cache**: In-memory caching with JSON serialization for persistence
- **Performance Metrics**: Statistics collection for optimization

## Development Patterns

### Dependency Injection
Utilizing Microsoft.Extensions.DependencyInjection for service resolution and testability. All core services are registered in the application startup.

### Asynchronous Operations
Heavy use of Task-based Asynchronous Pattern (TAP) for responsive UI during file operations. All file system, rules evaluation, and memory bank operations are asynchronous.

### Repository Pattern
Memory Bank and Rules Engine implementations use repository pattern with interface-based design for flexibility in storage mechanisms.

### Event-Driven Architecture
System components communicate through event publication/subscription for loose coupling.

### MVVM Pattern
Model-View-ViewModel pattern for clean separation of UI and business logic.

### Command Pattern
File operations implemented using command pattern to enable tracking, undoing, and batching.

### Strategy Pattern
Classification and rule condition evaluation utilize strategy pattern for pluggable implementations.

### Builder Pattern
Fluent API implemented for rule creation using the builder pattern.

### Factory Pattern
Classifier creation and rule action instantiation use factory pattern for flexibility.

### Observer Pattern
Progress reporting and status updates implemented using observer pattern.

## Technical Constraints

### Performance Considerations
- File scanning must be optimized for large directories (100,000+ files)
- Memory usage must remain reasonable during large operations
- Background processing should have minimal impact on system performance
- Memory Bank operations should be thread-safe for concurrent access
- Rule evaluation should be efficient for large rule sets
- Path validation must be optimized for frequent resolution
- Batch operations should utilize parallel processing where appropriate

### Security Requirements
- Limited to user's permissions on the file system
- No transmission of file data outside the local system
- Protection against recursive operations that could damage system files
- Secure storage of organization history with permission-based access
- Validation of rule actions before execution
- Sanitization of user input for path generation

### Compatibility
- Windows 10 and newer (primary target)
- Support for NTFS and ReFS file systems
- Handle extended character sets and long file paths
- Graceful handling of file locks and access restrictions
- Support for network file systems with latency considerations

## Memory Bank Implementation

### Storage Format
JSON-based serialization of organization history with the following structure:
- Entries collection with operation metadata
- File categorization history
- Path information (original and destination)
- Timestamps and operation types
- Category and classification details
- Operation performance metrics

### Threading Model
SemaphoreSlim for synchronized access to storage with async/await pattern.
Read-write lock pattern for optimized concurrent read access.

### Performance Optimizations
- Lazy loading of entries
- In-memory caching
- Batch operations for multi-file processing
- Directory existence validation caching
- Stream-based JSON processing for large datasets
- Compression for large history files
- Incremental serialization for continuous updates

## Rules Engine Implementation

### Rule Structure
- **RuleDefinition**: Core rule entity with conditions and actions
- **FileCondition**: Condition with type, operator, and value to match
- **FolderAction**: Action to perform when conditions are met
- **RuleGroup**: Logical grouping of rules with AND/OR relationships

### Condition Types
- File name, extension, size, dates, content, attributes
- Parent folder, MIME type, classification
- Custom condition extensions possible
- Content-based patterns with regular expressions

### Action Types
- Move, copy, rename, delete
- Tag, set attributes
- Create directory structures
- Custom action extensions
- Multi-step action sequences

### Evaluation Process
1. Load applicable rules from repository
2. Evaluate file against rule conditions
3. Generate actions for matching rules
4. Resolve conflicts between actions
5. Execute final action set
6. Record results in Memory Bank

### Conflict Resolution
- Priority-based (higher priority wins)
- Type-based (one move action, multiple other actions)
- Configurable resolution strategies
- User-definable conflict handlers
- Chain-of-responsibility pattern for multi-step resolution

### Template System
Pre-defined rule templates for common organization scenarios:
- Documents organization
- Image sorting by date
- Music organization by artist/album
- Video categorization
- Downloads management
- Project file management
- Source code organization

## Path Validation System

### Validation Approach
- Variable resolution using token replacement
- Path existence verification
- Permission checking
- Path normalization
- Redundancy elimination
- Directory nesting validation

### Variable Implementation
- Environment variables
- User-defined variables
- Date/time-based tokens
- File metadata tokens
- Expression-based computed values

### Error Handling
- Detailed validation error reporting
- Fallback path strategies
- Just-in-time path creation
- Path correction suggestions

## Development Environment Setup

### Required Software
- Visual Studio 2022 (Community Edition or higher)
- .NET 6.0 SDK
- Git
- SQLite tooling
- DocFX (for documentation generation)
- Azure DevOps CLI (for CI/CD integration)

### Build Process
1. Clone repository
2. Restore NuGet packages
3. Build solution (Debug or Release configuration)
4. Run tests
5. Generate documentation
6. Package application (Release only)

### Developer Onboarding
- Run initial setup script (./setup-dev-environment.ps1)
- Review coding standards document
- Setup local test directories with sample data
- Configure test data generation tools
- Set up performance benchmarking environment 