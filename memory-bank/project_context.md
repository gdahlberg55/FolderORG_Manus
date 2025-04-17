# FolderORG Manus - Project Context

## Project Overview
FolderORG Manus is an intelligent file organization system that automatically categorizes and structures files based on customizable rules and patterns. The system uses multiple classification strategies including extension-based, size-based, date-based, and content-based approaches to organize files according to user-defined rules.

## Current Development Focus
**Backup/Restore System - Selective Restoration (90% Complete)**

The current implementation focus is on completing the selective restoration capability of the Backup/Restore System. This feature allows users to choose specific files to restore rather than restoring entire transactions.

## Recent Project Cleanup
The project has undergone a significant file organization cleanup:
- Removed duplicate documentation files from the root directory
- Moved architecture documentation to `docs/architecture/`
- Moved component documentation to `docs/components/`
- Improved navigation with a cleaner root directory
- Maintained the same content while eliminating redundancy

## Project Organization
The project follows a clean architecture approach with clear separation of concerns:

```
FolderORG_Manus/
├── src/                           # Source code
│   ├── FolderORG.Manus.Core/      # Core entities and interfaces
│   ├── FolderORG.Manus.Domain/    # Domain logic and business rules
│   ├── FolderORG.Manus.Application/  # Application services
│   ├── FolderORG.Manus.Infrastructure/  # External concerns
│   ├── FolderORG.Manus.UI/        # WPF application
│   └── FolderORG.Manus.Tests/     # Unit and integration tests
├── docs/                          # Project documentation
│   ├── architecture/              # Architectural documentation
│   └── components/                # Component-specific documentation
├── deliverables/                  # Project deliverables
│   ├── documentation/             # User and technical documentation
│   └── specifications/            # Project specifications
├── testing/                       # Testing documentation
│   ├── plans/                     # Test plans and strategies
│   └── results/                   # Test results and reports
├── memory-bank/                   # Project context and progress tracking
└── ui_mockups/                    # UI design mockups
```

## Implementation Status

### Completed Components (100%)
- ✅ **Core Infrastructure**: Repository setup, solution structure, CI/CD pipeline
- ✅ **Documentation and Planning**: Project documentation, roadmap, specifications
- ✅ **File System Operations**: File scanning, metadata extraction, directory analysis
- ✅ **Classification Engine**: Extension, size, and date-based classification (content-based partial)
- ✅ **User Interface**: Application shell, navigation, visualization, and MVVM infrastructure
- ✅ **Memory Bank System**: Entry model, storage, history tracking, statistics generation
- ✅ **Rules Engine**: Rule definition, parsing, execution, conflict resolution
- ✅ **Path Validation System**: Format validation, normalization, variable resolution
- ✅ **Project Organization**: Structured directory layout and documentation organization

### In-Progress Components
- ✅ **Folder Structure System (100%)**: Template definition, path resolution, structure creation
- ✅ **File Operations**: Safe operations, batching, conflict resolution (Performance: 70%)
- 🔄 **Backup/Restore System (90%)**:
  - ✅ Transaction-based operations (100%)
  - ✅ State snapshots (100%)
  - ✅ Restore points (100%)
  - 🔄 User restoration interface (80%)
  - ✅ Test plan development (100%)
- 🔄 **Testing**: Unit tests (50%), Integration tests (25%)

### Not Started
- ⬜ **Advanced Features**: Machine learning classification, schedule-based organization, cloud integration

## Key Components

### Backup/Restore System
The system provides robust transaction-based file operations with rollback capability and user-friendly restoration interface. Current focus is on implementing selective restoration.

**Components:**
- Transaction-Based Operations: Model for tracking transaction details and execution
- State Snapshots: Creating and verifying snapshots of file system state
- Restore Points: Creation, retrieval, and filtering of restore points
- User Restoration Interface: UI for browsing, previewing, and executing restores

### Path Validation System
Provides comprehensive file and directory path validation with robust support for path normalization, variable resolution, and permission checking.

### Rules Engine
Implements condition-based rule system with action framework, JSON-based storage, and fluent API for defining organization rules.

### Memory Bank System
Tracks operation history, provides statistics, and maintains metadata for performed file operations.

## Recent Implementation
- Completed file cleanup for better organization and navigation
- Added **RelayCommand** and **RelayCommand<T>** classes to support the MVVM pattern
- Created structured directory layout for improved project organization
- Added comprehensive test plan for Backup/Restore System
- Created test result templates for standardized reporting
- Added component documentation structure with README files

## Upcoming Milestones
1. **Complete Backup/Restore System (Week 4)**: Finish selective restoration capability
2. **UI Statistics Enhancement (Week 6)**: Add Memory Bank statistics visualization
3. **Content Classification (Week 8)**: Improve content-based classification
4. **Beta Release (Week 10)**: Feature-complete with initial testing

## Known Issues
1. Performance bottlenecks when scanning extremely large directories (>500,000 files)
2. Metadata extraction fails for some specialized file formats
3. Memory usage peaks during heavy batch file operations
4. UI responsiveness issues during concurrent file operations
5. Backup data serialization performance for large histories

## Technical Stack
- C# (.NET 6.0)
- WPF with Material Design for UI
- MVVM architecture pattern
- JSON for configuration storage
- SQLite for rule definitions
- ML.NET for content classification

## Key Project Decisions
- JSON chosen over XML for configuration files (better library support)
- SQLite over file-based storage for rule definitions (complex queries)
- Clean architecture approach with feature-based organization
- Memory Bank system with JSON storage for tracking history
- Path Validation System with variable resolution and permission checking
- MVVM pattern with RelayCommand implementation
- Structured project organization with dedicated documentation directories
- Removal of duplicate files to maintain clean structure and better navigation 