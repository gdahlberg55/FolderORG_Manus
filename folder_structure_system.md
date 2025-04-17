# Folder Structure System Design

## Overview
The Folder Structure System is a core component of the File Organization Tool that provides predefined folder templates, supports custom hierarchies, enables nested categorization, and implements consistent naming conventions. This document outlines the design and implementation details for this component.

## 1. Predefined Folder Templates

### Default Templates
The system will include the following predefined folder templates:

#### Basic Template
```
Documents/
    Work/
    Personal/
    Financial/
    Educational/
Downloads/
    Applications/
    Documents/
    Media/
    Archives/
Media/
    Images/
        Photos/
        Screenshots/
        Artwork/
    Videos/
        Movies/
        TV Shows/
        Personal/
    Audio/
        Music/
        Podcasts/
        Audiobooks/
Projects/
    Active/
    Archived/
    Resources/
```

#### Developer Template
```
Projects/
    Active/
    Archived/
    Resources/
    Libraries/
Development/
    Repositories/
    Environments/
    Documentation/
    Tools/
    Builds/
    Logs/
Documents/
    Specifications/
    Designs/
    References/
    Contracts/
Resources/
    Assets/
    Templates/
    Samples/
```

#### Creative Professional Template
```
Projects/
    Client/
    Personal/
    Archived/
Assets/
    Images/
    Fonts/
    Templates/
    Audio/
    Video/
Media/
    Source Files/
    Renders/
    Exports/
    Raw/
References/
    Inspiration/
    Tutorials/
    Documentation/
```

### Template Management
- Templates will be stored as JSON configuration files
- Each template will include folder paths, descriptions, and suggested file types
- Users can modify existing templates or create new ones through the UI
- Templates can be exported and imported for sharing

## 2. Custom Folder Hierarchies

### Hierarchy Builder
- Interactive UI for creating and modifying folder structures
- Drag-and-drop interface for rearranging folders
- Right-click context menu for adding, renaming, and removing folders
- Preview pane showing the resulting folder structure

### User Preferences
- Save multiple custom hierarchies for different purposes
- Associate hierarchies with specific file types or projects
- Set default hierarchy for automatic organization
- Import/export functionality for sharing hierarchies

### Dynamic Hierarchy Adaptation
- System will suggest hierarchy modifications based on usage patterns
- Identify frequently accessed folders and suggest promoting them
- Detect unused folders and suggest archiving or removing them
- Recommend new folders based on file clustering analysis

## 3. Nested Categorization Support

### Depth Management
- Support for unlimited nesting depth (practical limit of 10 levels recommended)
- Visual indicators for deep nesting in UI
- Collapsible tree view for easy navigation
- Breadcrumb navigation for current location

### Category Relationships
- Parent-child relationships between categories
- Cross-category relationships (tags that span multiple categories)
- Category inheritance (subcategories inherit properties from parent)
- Category exclusivity rules (files that belong in only one category)

### Smart Path Resolution
- Automatic path shortening for deeply nested files
- Path aliases for frequently accessed deep folders
- Search functionality that understands nested relationships
- Quick navigation to any level in the hierarchy

## 4. Naming Convention Generator

### Convention Templates
- CamelCase: `documentName`
- Pascal Case: `DocumentName`
- Snake Case: `document_name`
- Kebab Case: `document-name`
- Title Case: `Document Name`
- Date Prefixed: `YYYY-MM-DD_DocumentName`
- Category Prefixed: `[Category]_DocumentName`
- Version Controlled: `DocumentName_v1.0`

### Custom Convention Builder
- Pattern-based naming with variables:
  - `{date}`: Current date in specified format
  - `{type}`: File type or extension
  - `{category}`: Parent folder or category
  - `{project}`: Associated project name
  - `{author}`: File creator
  - `{counter}`: Sequential number
- Regular expression support for advanced patterns
- Preview generator to show example results

### Automatic Renaming
- Batch rename files according to convention
- Preserve original name in metadata
- Option to create symbolic links with original names
- Conflict resolution for duplicate names (add counter, modify date, etc.)

### Convention Enforcement
- Rules for enforcing naming conventions
- Validation of file names against conventions
- Suggestions for correcting non-conforming names
- Exceptions list for files that should retain original names

## 5. Implementation Approach

### Data Structures
- Folder Template:
  ```json
  {
    "templateId": "basic",
    "name": "Basic Template",
    "description": "General purpose folder structure for home users",
    "structure": [
      {
        "path": "Documents",
        "children": [
          {"path": "Work"},
          {"path": "Personal"},
          {"path": "Financial"},
          {"path": "Educational"}
        ]
      },
      {
        "path": "Downloads",
        "children": [
          {"path": "Applications"},
          {"path": "Documents"},
          {"path": "Media"},
          {"path": "Archives"}
        ]
      }
      // Additional folders...
    ]
  }
  ```

- Naming Convention:
  ```json
  {
    "conventionId": "date-project",
    "name": "Date and Project",
    "pattern": "{date}_{project}_{name}",
    "dateFormat": "yyyy-MM-dd",
    "separator": "_",
    "caseFormat": "kebab-case",
    "examples": [
      "2025-04-14_project-alpha_design-document.docx"
    ]
  }
  ```

### Algorithms
- Folder Structure Generation:
  1. Load template or custom hierarchy
  2. Validate against existing file system
  3. Create missing directories
  4. Apply permissions and attributes
  5. Generate report of created structure

- Naming Convention Application:
  1. Parse file metadata and context
  2. Apply naming pattern with variables
  3. Check for naming conflicts
  4. Rename file or create symbolic link
  5. Update database with original and new names

### Storage
- Templates stored in JSON format in application data directory
- User preferences in local database
- Hierarchy configurations in user profile
- Naming conventions in configuration database

## 6. User Interface Components

### Template Browser
- Grid view of available templates with preview
- Search and filter options
- Import/export buttons
- Edit and duplicate functionality

### Hierarchy Editor
- Tree view of folder structure
- Drag-and-drop interface
- Context menu for operations
- Path editor for direct manipulation

### Naming Convention Editor
- Pattern builder with variable insertion
- Format selector for common conventions
- Live preview with example files
- Test functionality with user's actual files

## 7. Integration Points

### File Classification Engine
- Provide folder suggestions based on file type
- Receive classification data for folder assignment
- Share naming convention data for classification

### Automated Organization Process
- Receive folder structure for file organization
- Provide feedback on structure effectiveness
- Update structure based on organization results

### System Protection Mechanisms
- Receive protected paths to avoid modification
- Provide structure information for protection analysis
- Coordinate symbolic link creation for moved system files

## 8. Future Enhancements

### Cloud Integration
- Synchronize folder structures across devices
- Apply consistent organization to cloud storage
- Template sharing through cloud services

### AI-Driven Structure Optimization
- Analyze file access patterns to suggest structure improvements
- Learn from user behavior to predict optimal organization
- Generate personalized templates based on usage history

### Version Control Integration
- Special handling for version-controlled directories
- Integration with Git, SVN, and other VCS
- Structure templates optimized for different VCS workflows

### Multi-User Support
- Shared templates for team environments
- Role-based access to structure modification
- Conflict resolution for simultaneous changes
