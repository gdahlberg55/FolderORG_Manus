# FolderORG Manus - Project Structure

## Solution Organization

```
FolderORG.Manus.sln
│
├── src/
│   ├── FolderORG.Manus.Application/       # Application layer, workflow coordination
│   ├── FolderORG.Manus.Domain/            # Domain layer, business logic
│   ├── FolderORG.Manus.Infrastructure/    # Infrastructure layer, file operations, database
│   ├── FolderORG.Manus.UI/                # WPF UI application
│   └── FolderORG.Manus.Core/              # Shared core functionality
│
├── tests/
│   ├── FolderORG.Manus.UnitTests/         # Unit tests
│   ├── FolderORG.Manus.IntegrationTests/  # Integration tests
│   └── FolderORG.Manus.UITests/           # UI automation tests
│
├── tools/                                  # Development and deployment tools
├── docs/                                   # Documentation files
└── samples/                                # Sample data for testing
```

## Detailed Component Structure

### FolderORG.Manus.Application

```
FolderORG.Manus.Application/
├── Services/
│   ├── EngineCoordinator.cs               # Core engine implementation
│   ├── ScanningService.cs                 # File scanning service
│   ├── OrganizationService.cs             # Main organization orchestration
│   └── BackupService.cs                   # Backup and restore service
│
├── Workflows/
│   ├── InitialScanWorkflow.cs             # Initial scan workflow
│   ├── OrganizationWorkflow.cs            # Organization workflow
│   └── RestoreWorkflow.cs                 # Restore workflow
│
└── EventHandlers/                         # Application event handlers
```

### FolderORG.Manus.Domain

```
FolderORG.Manus.Domain/
├── Classification/
│   ├── Classifiers/                       # File classifiers by type
│   │   ├── ExtensionClassifier.cs
│   │   ├── ContentClassifier.cs
│   │   ├── SizeClassifier.cs
│   │   └── DateClassifier.cs
│   ├── Models/                            # Classification models
│   └── Services/                          # Classification services
│
├── Rules/
│   ├── Models/                            # Rule data models
│   │   ├── Rule.cs
│   │   ├── RuleCondition.cs
│   │   └── RuleAction.cs
│   ├── Services/                          # Rule processing services
│   │   ├── RuleParser.cs
│   │   ├── RuleValidator.cs
│   │   └── RuleExecutor.cs
│   └── Templates/                         # Rule templates
│
├── FolderStructure/
│   ├── Models/                            # Folder structure models
│   │   ├── FolderTemplate.cs
│   │   └── FolderNode.cs
│   ├── Services/                          # Folder structure services
│   │   ├── TemplateService.cs
│   │   └── StructureGenerator.cs
│   └── Templates/                         # Predefined templates
│
├── FileOperations/
│   ├── Models/                            # File operation models
│   │   ├── FileOperation.cs
│   │   └── OperationBatch.cs
│   ├── Services/                          # File operation services
│   │   ├── FileOperationService.cs
│   │   └── ConflictResolver.cs
│   └── Validators/                        # Operation validators
│
└── Common/                                # Shared domain models and interfaces
```

### FolderORG.Manus.Infrastructure

```
FolderORG.Manus.Infrastructure/
├── FileSystem/
│   ├── FileSystemAccess.cs                # File system access wrapper
│   ├── FileWatcher.cs                     # File system watcher implementation
│   └── FileOperationExecutor.cs           # File operation executor
│
├── Database/
│   ├── SQLiteContext.cs                   # SQLite database context
│   ├── Repositories/                      # Data repositories
│   └── Migrations/                        # Database migrations
│
├── Logging/
│   ├── LoggingService.cs                  # Logging implementation
│   └── ActivityLogger.cs                  # Activity logging
│
└── Configuration/
    ├── ConfigurationProvider.cs           # Configuration provider
    └── Settings/                          # Application settings
```

### FolderORG.Manus.UI

```
FolderORG.Manus.UI/
├── App.xaml                               # Application entry point
├── MainWindow.xaml                        # Main application window
│
├── Views/
│   ├── Dashboard/                         # Dashboard view
│   ├── Rules/                             # Rules management views
│   ├── Classification/                    # Classification views
│   ├── Structure/                         # Folder structure views
│   ├── Operations/                        # File operations views
│   └── Settings/                          # Settings views
│
├── ViewModels/                            # MVVM view models
│
├── Controls/                              # Custom UI controls
│
├── Converters/                            # WPF value converters
│
├── Resources/
│   ├── Icons/                             # Application icons
│   ├── Styles/                            # XAML styles
│   └── Themes/                            # Application themes
│
└── Services/                              # UI-specific services
```

### FolderORG.Manus.Core

```
FolderORG.Manus.Core/
├── Models/                                # Shared models
│
├── Interfaces/                            # Core interfaces
│
├── Extensions/                            # Extension methods
│
├── Constants/                             # Application constants
│
└── Utilities/                             # Helper utilities
```

## Key Interfaces

```csharp
// Classification Engine Interface
public interface IClassificationEngine
{
    Task<ClassificationResult> ClassifyFileAsync(FileInfo file);
    void RegisterClassifier(IFileClassifier classifier);
    Task<IEnumerable<ClassificationResult>> BatchClassifyAsync(IEnumerable<FileInfo> files);
}

// Rules Engine Interface
public interface IRulesEngine
{
    Task<IEnumerable<RuleMatch>> EvaluateRulesAsync(FileInfo file, ClassificationResult classification);
    Task<bool> ValidateRuleAsync(Rule rule);
    Task<Rule> SaveRuleAsync(Rule rule);
    Task<IEnumerable<Rule>> GetRulesAsync();
}

// Folder Structure Interface
public interface IFolderStructureService
{
    Task<FolderTemplate> CreateTemplateAsync(FolderTemplate template);
    Task<IEnumerable<FolderNode>> GenerateStructureAsync(FolderTemplate template, string basePath);
    Task<IEnumerable<FolderTemplate>> GetTemplatesAsync();
}

// File Operation Interface
public interface IFileOperationService
{
    Task<OperationResult> ExecuteOperationAsync(FileOperation operation);
    Task<OperationBatchResult> ExecuteBatchAsync(OperationBatch batch);
    Task<IEnumerable<FileOperation>> PlanOperationsAsync(IEnumerable<RuleMatch> matches);
}
```

## Implementation Approach

The project structure follows a clean architecture approach with:

1. **Domain-Centric Design**: All business logic contained in the Domain layer
2. **Interface Boundaries**: Clear interfaces between components
3. **Dependency Inversion**: Higher-level modules not dependent on lower-level modules
4. **Feature Organization**: Code organized by feature rather than technical type
5. **Separate UI Concerns**: UI isolated from business logic via MVVM pattern

This organization aligns with the system patterns documented in the Memory Bank, ensuring separation of concerns between the key components: Classification Engine, Rules Engine, Folder Structure System, and File Operation System, while providing clean integration points through well-defined interfaces. 