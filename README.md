# FolderORG Manus

An intelligent file organization system that automatically categorizes and structures files based on customizable rules and patterns.

## Features

- Automated file organization based on customizable rules
- Multiple classification strategies (extension, content, size, date)
- Folder structure templates for common organization patterns
- Comprehensive backup and restore functionality
- Memory Bank system to track organization history
- Path validation with variable resolution
- Intuitive Material Design UI

## Project Structure

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

## Development Status

Current development focus: Backup/Restore System - Selective Restoration (90% complete)

- Path Validation System: 100% Complete
- Backup/Restore System: 90% Complete
- Performance Optimization: 70% Complete
- Content-based Classification: In Progress
- Testing: In Progress
- UI Enhancement: In Progress

See the [todo.md](todo.md) file for detailed task status.

## Getting Started

### Prerequisites

- Windows 10 or later
- .NET 6.0 or later
- Visual Studio 2022

### Building the Project

1. Clone the repository
2. Open `FolderORG.Manus.sln` in Visual Studio
3. Build the solution

## Documentation

- Project specifications can be found in `deliverables/specifications/`
- User and technical documentation is available in `deliverables/documentation/`
- Component-specific documentation is located in `docs/components/`
- Architecture documentation is available in `docs/architecture/`

## Project Roadmap

1. **Complete Backup/Restore System (Week 4)**: Finish selective restoration capability
2. **UI Statistics Enhancement (Week 6)**: Add Memory Bank statistics visualization
3. **Content Classification (Week 8)**: Improve content-based classification
4. **Beta Release (Week 10)**: Feature-complete with initial testing

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 