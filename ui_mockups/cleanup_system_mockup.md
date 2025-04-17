# Intelligent Cleanup UI Mockup

## Overview
The Intelligent Cleanup interface provides users with a comprehensive system for managing unused files, reviewing cleanup recommendations, and safely removing unnecessary data. The design emphasizes safety, clarity, and user control throughout the cleanup process.

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Intelligent Cleanup                [_] [□] [X]  |
+----------------------------------------------------------------------+
| [≡] | Dashboard | Rules | Files | Cleanup | Settings |    [🔍] [👤]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------+  +--------------------------------+      |
|  | CLEANUP STATUS         |  | CLEANUP RECOMMENDATIONS        |      |
|  +------------------------+  +--------------------------------+      |
|  |                        |  |                                |      |
|  | Last Cleanup:          |  | 🗑️ Temporary Files             |      |
|  | April 10, 2025         |  |   245 files, 1.2 GB            |      |
|  |                        |  |   [Review] [Clean Now]         |      |
|  | Space Reclaimed:       |  |                                |      |
|  | 4.7 GB this month      |  | 🗑️ Downloads Folder            |      |
|  |                        |  |   127 unused files, 3.4 GB      |      |
|  | Files in Staging:      |  |   [Review] [Clean Now]         |      |
|  | 89 files (2.1 GB)      |  |                                |      |
|  | [View Staged Files]    |  | 🗑️ Duplicate Files             |      |
|  |                        |  |   56 duplicates, 780 MB         |      |
|  | [Run Full Cleanup]     |  |   [Review] [Clean Now]         |      |
|  +------------------------+  +--------------------------------+      |
|                                                                      |
|  +------------------------------------------------------------------+|
|  | CLEANUP REVIEW: TEMPORARY FILES                                  ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | Select files to clean up:                                        ||
|  |                                                                  ||
|  | [✓] Select All (245 files, 1.2 GB)                              ||
|  |                                                                  ||
|  | [✓] Browser Cache Files (156 files, 820 MB)                     ||
|  |     Last accessed: Various dates                                 ||
|  |     [Details ▼]                                                  ||
|  |                                                                  ||
|  | [✓] Application Temp Files (67 files, 340 MB)                   ||
|  |     Last accessed: Various dates                                 ||
|  |     [Details ▼]                                                  ||
|  |                                                                  ||
|  | [✓] Windows Temp Files (22 files, 45 MB)                        ||
|  |     Last accessed: Various dates                                 ||
|  |     [Details ▼]                                                  ||
|  |                                                                  ||
|  | [Clean Selected (245 files)] [Move to Staging] [Cancel]          ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
+----------------------------------------------------------------------+
```

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Staged Files                      [_] [□] [X]  |
+----------------------------------------------------------------------+
| [≡] | Dashboard | Rules | Files | Cleanup | Settings |    [🔍] [👤]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------+  +--------------------------------+      |
|  | STAGING INFORMATION    |  | STAGED FILES                   |      |
|  +------------------------+  +--------------------------------+      |
|  |                        |  |                                |      |
|  | Total Files: 89        |  | Name           Date Staged  Size     |      |
|  | Total Size: 2.1 GB     |  | +------------------------------+      |
|  |                        |  | | 📄 Report.docx   Apr 1    2.4 MB   |      |
|  | Stage 1 (Flagged):     |  | | 📄 Backup.zip    Apr 1    450 MB   |      |
|  | 32 files (780 MB)      |  | | 📄 Old_Log.txt   Apr 5    15 MB    |      |
|  |                        |  | | 📄 Setup.exe     Apr 5    65 MB    |      |
|  | Stage 2 (Quarantine):  |  | | 📄 Photo001.jpg  Apr 10   8.2 MB   |      |
|  | 45 files (1.2 GB)      |  | | 📄 Video.mp4     Apr 10   720 MB   |      |
|  |                        |  | | 📄 Notes.pdf     Apr 14   1.5 MB   |      |
|  | Stage 3 (Archive):     |  | | 📄 Archive.rar   Apr 14   340 MB   |      |
|  | 12 files (120 MB)      |  |                                |      |
|  |                        |  | Showing 8 of 89 files          |      |
|  | [Cleanup Settings]     |  | [Load More]                    |      |
|  +------------------------+  +--------------------------------+      |
|                                                                      |
|  +------------------------------------------------------------------+|
|  | FILE DETAILS                                                     ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | File: Report.docx                                                ||
|  | Original Location: C:\Users\Username\Documents\Old Projects\     ||
|  | Date Created: January 15, 2024                                   ||
|  | Last Accessed: October 23, 2024 (173 days ago)                   ||
|  | Size: 2.4 MB                                                     ||
|  | Stage: 1 - Flagged (Will move to Quarantine on May 1, 2025)      ||
|  |                                                                  ||
|  | Reason for Staging: Unused file (not accessed in over 5 months)  ||
|  |                                                                  ||
|  | [Restore to Original Location] [Restore to...] [Delete Now]      ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
+----------------------------------------------------------------------+
```

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Cleanup Settings                  [_] [□] [X]  |
+----------------------------------------------------------------------+
| [≡] | Dashboard | Rules | Files | Cleanup | Settings |    [🔍] [👤]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------------------------------------------------+|
|  | CLEANUP SETTINGS                                                 ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | Unused File Parameters                                           ||
|  | +--------------------------------------------------------------+ ||
|  | | File Type       | Unused After | Action                      | ||
|  | +--------------------------------------------------------------+ ||
|  | | Documents       | 6 months     | Stage                       | ||
|  | | Images          | 1 year       | Stage                       | ||
|  | | Videos          | 1 year       | Stage                       | ||
|  | | Downloads       | 3 months     | Stage                       | ||
|  | | Executables     | 6 months     | Stage                       | ||
|  | | Temporary Files | 1 week       | Delete                      | ||
|  | | System Files    | Never        | Protect                     | ||
|  | +--------------------------------------------------------------+ ||
|  |                                                [Add] [Edit] [Remove] ||
|  |                                                                  ||
|  | Staging System                                                   ||
|  |                                                                  ||
|  | Stage 1 (Flagged):     Files remain in original location         ||
|  |                        Duration: [30] days                       ||
|  |                                                                  ||
|  | Stage 2 (Quarantine):  Files moved to quarantine folder          ||
|  |                        Duration: [60] days                       ||
|  |                                                                  ||
|  | Stage 3 (Archive):     Files compressed in archive               ||
|  |                        Duration: [90] days                       ||
|  |                                                                  ||
|  | After all stages:      [●] Delete permanently                    ||
|  |                        [ ] Keep in archive indefinitely          ||
|  |                                                                  ||
|  | Quarantine Location:   [C:\FileOrganizer\Quarantine         ]    ||
|  |                        [Browse]                                  ||
|  |                                                                  ||
|  | [Restore Defaults]                             [Cancel] [Save]   ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
+----------------------------------------------------------------------+
```

## Key Components

### 1. Cleanup Status Panel
- **Last Cleanup**: Date of the most recent cleanup operation
- **Space Reclaimed**: Amount of storage freed by cleanup operations
- **Files in Staging**: Count and size of files in the staging system
- **View Staged Files Button**: Access to the staging management interface
- **Run Full Cleanup Button**: Initiates a comprehensive cleanup process

### 2. Cleanup Recommendations Panel
- **Category Sections**: Groupings of cleanup opportunities (Temporary Files, Downloads, Duplicates)
- **Statistics**: File counts and potential space savings for each category
- **Action Buttons**: Review (examine files before cleaning) and Clean Now (immediate action)

### 3. Cleanup Review Interface
- **Category Header**: Identifies the type of files being reviewed
- **Selection Controls**: Checkboxes for selecting/deselecting items
- **File Groupings**: Logical groups of related files
- **Statistics**: File counts and sizes for each group
- **Action Buttons**: Clean Selected, Move to Staging, Cancel

### 4. Staged Files Interface
- **Staging Information Panel**: Overview of files in the staging system
- **Stage Breakdown**: Counts and sizes for each staging phase
- **Staged Files List**: Sortable list of files in staging with key metadata
- **File Details Panel**: Comprehensive information about selected file
- **Action Buttons**: Restore options and Delete Now

### 5. Cleanup Settings Interface
- **Unused File Parameters**: Table of file types with customizable thresholds
- **Staging System Configuration**: Duration settings for each stage
- **Final Disposition Options**: What happens after all staging phases
- **Location Settings**: Paths for quarantine and archive storage

## Status Indicators

### Staging Phases
- **Stage 1 (Flagged)**: Files marked as unused but still in original location
- **Stage 2 (Quarantine)**: Files moved to separate quarantine location
- **Stage 3 (Archive)**: Files compressed to save space before potential deletion

### File Status
- **Recently Added**: Files newly added to staging
- **Approaching Next Stage**: Files nearing transition to next stage
- **Ready for Deletion**: Files that have completed all staging phases

## Color Scheme
- **Primary Color**: #2D7DD2 (Blue) - Used for headers and highlights
- **Secondary Color**: #97CC04 (Green) - Used for restore actions
- **Warning Color**: #F45D01 (Orange) - Used for cleanup actions
- **Danger Color**: #D64045 (Red) - Used for permanent deletion
- **Background**: #F8F9FA (Light Gray) - Main background color
- **Text**: #212529 (Dark Gray) - Primary text color
- **Light Text**: #6C757D (Medium Gray) - Secondary text color

## Typography
- **Primary Font**: Segoe UI (Windows), SF Pro Text (macOS), Roboto (Fallback)
- **Header Size**: 16px for panel headers, 14px for section headers
- **Body Text**: 12px for general content
- **Metadata**: 11px for file details and statistics

## Interaction Design
- **Progressive Disclosure**: Details revealed on demand
- **Confirmation Dialogs**: Required for destructive actions
- **Undo Capability**: Available for recent cleanup actions
- **Batch Operations**: Ability to handle multiple files simultaneously
- **Search and Filter**: Tools to locate specific files in staging

## Accessibility Features
- **Keyboard Navigation**: Full keyboard support for all operations
- **Screen Reader Support**: ARIA labels and proper semantic structure
- **High Contrast Mode**: Alternative color scheme for visibility
- **Text Scaling**: Interface adjusts to system text size settings
- **Focus Indicators**: Clear visual focus states for keyboard navigation
