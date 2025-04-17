# System Patterns: FolderORG Manus

## System Architecture
FolderORG Manus follows a modular architecture with distinct components that handle specific aspects of the file organization process:

```
┌───────────────────┐     ┌───────────────────┐     ┌───────────────────┐
│                   │     │                   │     │                   │
│   User Interface  │────▶│   Core Engine     │────▶│   Classification  │
│                   │     │                   │     │      Engine       │
└───────────────────┘     └───────────────────┘     └───────────────────┘
                                   │                          │
                                   ▼                          ▼
                          ┌───────────────────┐     ┌───────────────────┐
                          │                   │     │                   │
                          │   Rules Engine    │     │ Folder Structure  │
                          │                   │     │      System       │
                          └───────────────────┘     └───────────────────┘
                                   │                          │
                                   └──────────────┬───────────┘
                                                  ▼
                                        ┌───────────────────┐
                                        │                   │
                                        │  File Operation   │
                                        │      System       │
                                        └───────────────────┘
                                                  │
                                                  ▼
                                        ┌───────────────────┐
                                        │                   │
                                        │  Backup/Restore   │
                                        │      System       │
                                        └───────────────────┘
                                                  │
                                                  ▼
                                        ┌───────────────────┐
                                        │                   │
                                        │   Memory Bank     │
                                        │      System       │
                                        └───────────────────┘
```

## Key Components

### 1. User Interface
- **Pattern**: MVC (Model-View-Controller) with MVVM (Model-View-ViewModel)
- **Responsibility**: Provides user interaction for configuration, monitoring, and control
- **Design Notes**: Clean separation between UI and business logic for maintainability
- **Key Features**: 
  - Material Design styling for modern appearance
  - Responsive layout for different screen sizes
  - Progress visualization for lengthy operations
  - Statistics visualization from Memory Bank
  - Rule creation and management interfaces

### 2. Core Engine
- **Pattern**: Mediator/Coordinator with Façade
- **Responsibility**: Coordinates workflow between system components
- **Design Notes**: Central orchestration point that delegates specialized tasks
- **Key Features**:
  - Workflow coordination for file operations
  - Component communication through events
  - Configuration management
  - Exception handling and logging
  - Performance monitoring
  - Background task management

### 3. Classification Engine
- **Pattern**: Strategy Pattern with Factory Method
- **Responsibility**: Analyzes files to determine appropriate categorization
- **Design Notes**: Pluggable classifiers for different file types and analysis methods
- **Key Features**:
  - Multiple classifier implementations
  - Extension-based classification
  - Size-based classification
  - Date-based classification
  - Content-based classification
  - Metadata extraction and analysis
  - Classification caching and optimization

### 4. Rules Engine
- **Pattern**: Rule-based System with Command Pattern
- **Responsibility**: Applies user-defined and system rules to determine organization actions
- **Design Notes**: Uses declarative rule definitions with priority ordering
- **Key Features**:
  - Condition-based rule evaluation
  - Flexible action framework
  - Conflict resolution strategies
  - JSON-based rule storage
  - Rule templates for common scenarios
  - Fluent builder API for rule construction
  - Path validation and variable resolution

### 5. Folder Structure System
- **Pattern**: Template Method/Builder with Prototype
- **Responsibility**: Creates and manages folder hierarchy templates
- **Design Notes**: Supports both predefined and custom folder structures
- **Key Features**:
  - Template-based folder creation
  - Dynamic path resolution
  - Variable substitution
  - Permission validation
  - Structure verification
  - User-defined templates
  - Path normalization and optimization

### 6. File Operation System
- **Pattern**: Command Pattern with Memento
- **Responsibility**: Performs actual file system operations (move, copy, rename)
- **Design Notes**: Atomic operations with rollback capability for safety
- **Key Features**:
  - Safe file operations with validation
  - Transactional operations for atomicity
  - Progress reporting for lengthy operations
  - Conflict handling for file name collisions
  - Error recovery strategies
  - Operation batching for performance
  - Parallel processing for large operations

### 7. Backup/Restore System
- **Pattern**: Memento Pattern with Strategy
- **Responsibility**: Tracks file operations and provides restoration capability
- **Design Notes**: Maintains operation history with serializable state
- **Key Features**:
  - Operation logging for all file changes
  - Restoration capability for undo operations
  - State snapshots for critical points
  - User interface for restoration
  - Optimized storage for large histories
  - Incremental backup of operation history
  - Different restoration strategies

### 8. Memory Bank System
- **Pattern**: Repository Pattern with Observer
- **Responsibility**: Tracks organization history and provides statistics
- **Design Notes**: Maintains comprehensive metadata for insights
- **Key Features**:
  - JSON-based storage for organization history
  - File metadata tracking
  - Organization statistics generation
  - Performance metrics collection
  - Thread-safe operations for concurrent access
  - Data visualization support
  - Import/export functionality

## Data Flow

1. **Scan Phase**: System scans target directories for files
2. **Analysis Phase**: Classification Engine analyzes file properties and content
3. **Rule Application**: Rules Engine determines appropriate actions
4. **Structure Creation**: Folder Structure System creates necessary folders
5. **Execution Phase**: File Operation System performs required file operations
6. **Backup Recording**: Backup/Restore System records all changes
7. **Memory Bank Update**: Memory Bank records organization metadata and statistics
8. **User Interface Update**: UI displays progress and results to the user

## Critical Implementation Paths

### File Analysis Pipeline
- File metadata extraction → Content analysis → Pattern matching → Classification
- **Optimization**: Parallel processing for large file sets
- **Error Handling**: Graceful degradation for unsupported file types
- **Extension**: Pluggable analyzers for specialized file formats

### Rules Processing
- Rule loading → Priority sorting → Conflict resolution → Rule application
- **Performance**: Rule indexing and caching for quick evaluation
- **Safety**: Validation of rule conditions and actions before execution
- **Flexibility**: Extensible condition and action framework

### File Operation Safety
- Operation planning → Pre-operation validation → Atomic execution → Rollback capability
- **Error Recovery**: Transaction-like operations with rollback
- **Performance**: Batch operations for efficiency
- **Monitoring**: Progress reporting and cancellation support

### Path Validation and Resolution
- Variable identification → Token resolution → Path validation → Path normalization
- **Security**: Input validation and sanitization
- **Flexibility**: Extensible variable providers
- **Performance**: Caching of resolution results

## Design Principles

1. **Safety First**: Prevent data loss through validation, backup, and atomic operations
2. **User Control**: Provide visibility and override capability for all automated actions
3. **Extensibility**: Component-based design allowing for easy feature additions
4. **Performance**: Efficient processing to handle large file collections
5. **Adaptability**: Learning from user actions to improve organization over time
6. **Consistency**: Predictable behavior across different file types and operations
7. **Transparency**: Clear visibility into system actions and decisions
8. **Resilience**: Robust error handling and recovery mechanisms

## Architecture Layers

### 1. Presentation Layer
- **WPF UI Components**: Material Design-based interface elements
- **ViewModels**: MVVM pattern for UI logic separation
- **Commands**: Command pattern for UI actions
- **Visualization**: Statistics and progress visualization

### 2. Application Layer
- **Application Services**: Workflow orchestration and coordination
- **Use Case Implementations**: Implementation of user stories
- **Event Handling**: System-wide event management
- **Configuration Management**: User settings and preferences

### 3. Domain Layer
- **Business Logic**: Core rules and organization logic
- **Domain Models**: Entity definitions and relationships
- **Domain Services**: Specialized processing (classification, rules)
- **Validators**: Business rule validation components

### 4. Infrastructure Layer
- **File System Access**: Safe file operation wrappers
- **Data Persistence**: Storage mechanisms for rules and history
- **Logging**: Structured logging for system activities
- **External Services**: Integration with external components

## Performance Optimization Strategies

1. **Parallel Processing**: Multi-threaded operations for CPU-intensive tasks
2. **Batch Operations**: Grouping related file operations for efficiency
3. **Memory Management**: Careful control of memory usage for large file sets
4. **Caching**: Strategic caching of frequently used data
5. **Lazy Evaluation**: Deferred processing for expensive operations
6. **Indexing**: Fast lookup structures for rules and classifications
7. **Stream Processing**: Efficient handling of large data sets

## Error Handling and Recovery

1. **Exception Hierarchies**: Specialized exceptions for different error types
2. **Graceful Degradation**: Continued operation with limited functionality when possible
3. **Transaction-like Operations**: Atomic operations with rollback capability
4. **Error Logging**: Comprehensive logging for diagnostic purposes
5. **User Feedback**: Clear error messages and recovery suggestions
6. **Retry Mechanisms**: Automatic retry for transient failures
7. **State Preservation**: Saving system state to prevent data loss

## Extensibility Points

1. **Classifier Plugins**: New file classification strategies
2. **Rule Condition Types**: Additional condition types for rules
3. **Rule Action Types**: New action implementations for organization
4. **Template Providers**: Custom folder structure templates
5. **Variable Resolvers**: New variable types for path resolution
6. **UI Themes**: Customizable visual styling
5. **Adaptability**: Learning from user actions to improve organization over time 