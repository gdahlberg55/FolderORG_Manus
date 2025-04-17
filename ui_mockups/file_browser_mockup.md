# File Browser UI Mockup

## Overview
The File Browser provides an intuitive interface for users to navigate, view, and manage their files. It combines familiar file explorer elements with enhanced organization features and visual indicators for file status.

```
+----------------------------------------------------------------------+
|  File Organizer Pro - File Browser                      [_] [□] [X]  |
+----------------------------------------------------------------------+
| [≡] | Dashboard | Rules | Files | Reports | Settings |    [🔍] [👤]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------+  +--------------------------------+      |
|  | LOCATIONS              |  | C:\Users\Username\Documents    |      |
|  +------------------------+  +--------------------------------+      |
|  |                        |  |                                |      |
|  | ▼ This PC              |  | Name           Type    Status  |      |
|  |   ▶ Desktop            |  | +------------------------------+      |
|  |   ▼ Documents          |  | | 📁 Projects     Folder  ✓     |      |
|  |     ▶ Projects         |  | | 📁 Personal     Folder  ✓     |      |
|  |     ▶ Work             |  | | 📁 Work         Folder  ✓     |      |
|  |     ▶ Personal         |  | | 📁 Financial    Folder  ⚠️     |      |
|  |   ▶ Downloads          |  | | 📄 Resume.docx  Word    ✓     |      |
|  |   ▶ Pictures           |  | | 📄 Budget.xlsx  Excel   ✓     |      |
|  |   ▶ Music              |  | | 📄 Notes.txt    Text    ⚠️     |      |
|  |   ▶ Videos             |  | | 📄 Scan001.pdf  PDF     🔄     |      |
|  | ▶ Network              |  | | 📄 Old File.doc Word    ⏱️     |      |
|  | ▶ Cloud Storage        |  | | 📄 Backup.zip   ZIP     ✓     |      |
|  |                        |  |                                |      |
|  | [+ Add Location]       |  | 10 items (6 files, 4 folders)  |      |
|  +------------------------+  +--------------------------------+      |
|                                                                      |
|  +------------------------------------------------------------------+|
|  | FILE DETAILS                                                     ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | Name: Budget.xlsx                                                ||
|  | Type: Microsoft Excel Spreadsheet                                ||
|  | Size: 2.4 MB                                                     ||
|  | Created: April 10, 2025 10:23 AM                                 ||
|  | Modified: April 14, 2025 09:15 AM                                ||
|  | Last accessed: Today, 11:30 AM                                   ||
|  |                                                                  ||
|  | Organization Status: ✓ Organized                                 ||
|  | Applied Rules: "Financial Documents", "Excel Files"              ||
|  | Tags: Financial, 2025, Budget                                    ||
|  |                                                                  ||
|  | [Open] [Organize] [Move To ▼] [More Actions ▼]                   ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
+----------------------------------------------------------------------+
```

## Key Components

### 1. Navigation Bar
- **Main Menu Toggle (≡)**: Expands/collapses the side navigation panel
- **Section Tabs**: Dashboard, Rules, Files, Reports, Settings
- **Search (🔍)**: Global search functionality
- **User Profile (👤)**: Access to user settings and preferences

### 2. Locations Panel
- **Hierarchical Tree View**: Expandable/collapsible folder structure
- **Standard Locations**: This PC, Desktop, Documents, Downloads, etc.
- **Custom Locations**: Network, Cloud Storage, User-defined locations
- **Add Location Button**: Allows adding new locations to monitor

### 3. File List Panel
- **Path Bar**: Shows current location path with navigation capabilities
- **Column Headers**: Name, Type, Status with sorting functionality
- **File/Folder List**: Icons, names, and metadata for items in current location
- **Status Icons**:
  - ✓ (Green checkmark): Properly organized
  - ⚠️ (Warning): Needs organization
  - 🔄 (Sync): Currently being processed
  - ⏱️ (Clock): Unused/old file
- **Item Count**: Summary of files and folders in current view

### 4. File Details Panel
- **Basic Properties**: Name, type, size, dates
- **Organization Information**: Status, applied rules, tags
- **Action Buttons**: Open, Organize, Move To, More Actions

## Status Indicators

### Organization Status
- **Organized (✓)**: File is in the correct location with proper naming
- **Needs Organization (⚠️)**: File could be better organized
- **Processing (🔄)**: File is currently being analyzed or moved
- **Unused (⏱️)**: File hasn't been accessed in a long time
- **Protected (🔒)**: System or application file that shouldn't be moved
- **Duplicate (🔄)**: File is a duplicate of another file

### Visual Cues
- **Folder Coloring**: Optional color-coding for different folder types
- **File Type Icons**: Distinctive icons for different file types
- **Selection Highlighting**: Clear visual indication of selected items
- **Drag Target Highlighting**: Visual feedback during drag operations

## Interaction Features

### Selection and Multi-Select
- **Click**: Select single item
- **Ctrl+Click**: Add to selection
- **Shift+Click**: Range selection
- **Drag Selection**: Marquee/lasso selection

### Drag and Drop
- **Move Operations**: Drag files to new locations
- **Organization Targets**: Special drop zones for quick organization
- **Rule Application**: Drop zones for applying specific rules

### Context Menu
- **Right-Click Menu**: Context-sensitive options
- **Common Operations**: Open, Copy, Move, Delete
- **Organization Options**: Organize, Apply Rule, Tag
- **Custom Actions**: User-defined operations

## View Options

### List Views
- **Details View**: Columns with comprehensive information
- **List View**: Simple list with basic details
- **Tiles View**: Larger icons with preview
- **Content View**: Preview-focused with content snippets

### Sorting and Filtering
- **Sort Options**: Name, Date, Size, Type, Status
- **Filter Bar**: Quick filtering by type, date, status
- **Advanced Filters**: Complex criteria matching
- **Saved Filters**: User-defined filter presets

## Color Scheme
- **Primary Color**: #2D7DD2 (Blue) - Used for headers, selection, and highlights
- **Secondary Color**: #97CC04 (Green) - Used for "organized" status
- **Warning Color**: #F45D01 (Orange) - Used for "needs organization" status
- **Background**: #F8F9FA (Light Gray) - Main background color
- **Alternate Row**: #E9ECEF (Lighter Gray) - Alternate row background
- **Text**: #212529 (Dark Gray) - Primary text color
- **Light Text**: #6C757D (Medium Gray) - Secondary text color

## Typography
- **Primary Font**: Segoe UI (Windows), SF Pro Text (macOS), Roboto (Fallback)
- **File Names**: 12px, regular
- **Metadata**: 11px, light
- **Headers**: 13px, semi-bold
- **Details**: 12px, regular

## Accessibility Features
- **Keyboard Navigation**: Full keyboard support for all operations
- **Screen Reader Support**: ARIA labels and proper semantic structure
- **High Contrast Mode**: Alternative color scheme for visibility
- **Text Scaling**: Interface adjusts to system text size settings
- **Focus Indicators**: Clear visual focus states for keyboard navigation
