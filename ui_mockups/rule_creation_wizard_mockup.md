# Rule Creation Wizard UI Mockup

## Overview
The Rule Creation Wizard provides a step-by-step interface for users to create and customize organization rules. The wizard simplifies complex rule creation through a guided process with visual feedback and previews.

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Create New Rule                   [_] [□] [X]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------------------------------------------------+|
|  | CREATE NEW ORGANIZATION RULE                      Step 1 of 4    ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | Rule Type:                                                       ||
|  |                                                                  ||
|  | [●] File Organization Rule   [ ] Cleanup Rule   [ ] Custom Rule  ||
|  |                                                                  ||
|  | Rule Name:                                                       ||
|  | [Document Organization Rule                                   ]  ||
|  |                                                                  ||
|  | Description: (Optional)                                          ||
|  | [Organize documents by type and date                          ]  ||
|  |                                                                  ||
|  | Apply to:                                                        ||
|  | [✓] Documents folder                                             ||
|  | [✓] Downloads folder                                             ||
|  | [ ] Desktop                                                      ||
|  | [ ] Custom location... [Browse]                                  ||
|  |                                                                  ||
|  | Include subfolders: [✓]                                          ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
|  [Cancel]                                           [Back] [Next >]  |
|                                                                      |
+----------------------------------------------------------------------+
```

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Create New Rule                   [_] [□] [X]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------------------------------------------------+|
|  | DEFINE CONDITIONS                                 Step 2 of 4    ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | Match files that meet: [●] All conditions  [ ] Any condition     ||
|  |                                                                  ||
|  | Conditions:                                                      ||
|  | +--------------------------------------------------------------+ ||
|  | | File Type    | is            | Document                    [+] ||
|  | | Created Date | is newer than | 30 days                     [+] ||
|  | | File Size    | is less than  | 10 MB                       [+] ||
|  | |              |               |                             [+] ||
|  | +--------------------------------------------------------------+ ||
|  |                                                                  ||
|  | Available Condition Types:                                       ||
|  | [File Type ▼]  [File Name ▼]  [Content ▼]  [Metadata ▼]         ||
|  |                                                                  ||
|  | Preview Matching Files:                                          ||
|  | +--------------------------------------------------------------+ ||
|  | | 📄 Project Report.docx      | 📄 Meeting Notes.pdf           | ||
|  | | 📄 Financial Summary.xlsx   | 📄 Presentation.pptx           | ||
|  | | 📄 Contract Draft.docx      | 📄 Research Paper.pdf          | ||
|  | +--------------------------------------------------------------+ ||
|  |                                                                  ||
|  | Matching: 127 files                                              ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
|  [Cancel]                                           [< Back] [Next >]|
|                                                                      |
+----------------------------------------------------------------------+
```

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Create New Rule                   [_] [□] [X]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------------------------------------------------+|
|  | DEFINE ACTIONS                                    Step 3 of 4    ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | Actions to perform on matching files:                            ||
|  |                                                                  ||
|  | [✓] Move files to folder:                                        ||
|  |     [Documents\Organized\{FileType}\{Year}\{Month}           ]   ||
|  |     [Browse]  [Template Variables ▼]                             ||
|  |                                                                  ||
|  | [✓] Rename files using pattern:                                  ||
|  |     [{Date}_{OriginalName}                                   ]   ||
|  |     [Template Variables ▼]                                        ||
|  |                                                                  ||
|  | [ ] Apply tags:                                                  ||
|  |     [Document] [Work] [Add New Tag +]                            ||
|  |                                                                  ||
|  | [ ] Convert file format:                                         ||
|  |     Convert [           ] to [           ]                       ||
|  |                                                                  ||
|  | Preview Result:                                                  ||
|  | +--------------------------------------------------------------+ ||
|  | | Original: C:\Users\User\Downloads\Project Report.docx         | ||
|  | | Result:   C:\Users\User\Documents\Organized\Word\2025\April\  | ||
|  | |           2025-04-14_Project Report.docx                      | ||
|  | +--------------------------------------------------------------+ ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
|  [Cancel]                                           [< Back] [Next >]|
|                                                                      |
+----------------------------------------------------------------------+
```

```
+----------------------------------------------------------------------+
|  File Organizer Pro - Create New Rule                   [_] [□] [X]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------------------------------------------------+|
|  | SCHEDULE AND FINALIZE                              Step 4 of 4   ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | When to run this rule:                                           ||
|  |                                                                  ||
|  | [●] Run with regular organization (Default schedule)             ||
|  | [ ] Run on specific schedule:                                    ||
|  |     Every [      ] at [      ]                                   ||
|  | [ ] Run manually only                                            ||
|  |                                                                  ||
|  | Rule priority:                                                   ||
|  |                                                                  ||
|  | Low [○───○───●───○───○] High                                     ||
|  |                                                                  ||
|  | Additional options:                                              ||
|  |                                                                  ||
|  | [✓] Create backup before organizing                              ||
|  | [✓] Skip files in use                                            ||
|  | [ ] Apply to new files only                                      ||
|  | [ ] Notify when rule runs                                        ||
|  |                                                                  ||
|  | Summary:                                                         ||
|  | This rule will organize document files from Documents and        ||
|  | Downloads folders into categorized folders by type and date,     ||
|  | and rename them with date prefix.                                ||
|  |                                                                  ||
|  +------------------------------------------------------------------+|
|                                                                      |
|  [Cancel]                                    [< Back] [Save & Apply] |
|                                                                      |
+----------------------------------------------------------------------+
```

## Key Components

### Step 1: Rule Basics
- **Rule Type Selection**: Radio buttons for different rule types
- **Rule Name Field**: Text input for naming the rule
- **Description Field**: Optional text area for rule description
- **Target Location Selection**: Checkboxes for common locations and browse option
- **Subfolder Option**: Checkbox to include subfolders in the rule scope

### Step 2: Condition Definition
- **Condition Logic Selector**: Radio buttons for "All" or "Any" conditions
- **Condition Builder**: Table with condition type, operator, and value columns
- **Condition Type Selectors**: Dropdown menus for different condition categories
- **Preview Panel**: Shows files that match the current conditions
- **Match Counter**: Displays the number of files matching the conditions

### Step 3: Action Definition
- **Move Action**: Checkbox with template-based path builder
- **Rename Action**: Checkbox with pattern-based name builder
- **Tag Action**: Checkbox with tag selection interface
- **Format Conversion**: Checkbox with format selection dropdowns
- **Preview Panel**: Shows example of original and resulting file paths

### Step 4: Schedule and Finalize
- **Schedule Options**: Radio buttons for different scheduling approaches
- **Priority Slider**: Visual slider for setting rule priority
- **Additional Options**: Checkboxes for various rule behaviors
- **Summary**: Text area showing a human-readable summary of the rule
- **Save & Apply Button**: Finalizes and activates the rule

## Color Scheme
- **Primary Color**: #2D7DD2 (Blue) - Used for headers, buttons, and highlights
- **Secondary Color**: #97CC04 (Green) - Used for success indicators and actions
- **Accent Color**: #F45D01 (Orange) - Used for warnings and important notifications
- **Background**: #F8F9FA (Light Gray) - Main background color
- **Text**: #212529 (Dark Gray) - Primary text color
- **Light Text**: #6C757D (Medium Gray) - Secondary text color

## Typography
- **Primary Font**: Segoe UI (Windows), SF Pro Text (macOS), Roboto (Fallback)
- **Header Size**: 16px for panel headers, 14px for section headers
- **Body Text**: 12px for general content
- **Input Fields**: 12px for text input

## Interaction Design
- **Progressive Disclosure**: Information revealed step by step
- **Immediate Feedback**: Preview updates as conditions and actions are defined
- **Validation**: Input validation with clear error messages
- **Help System**: Context-sensitive help available at each step
- **Navigation**: Clear back/next buttons with visual progress indicator

## Accessibility Features
- **Keyboard Navigation**: Full keyboard support with tab order
- **Screen Reader Support**: ARIA labels and role attributes
- **Focus Indicators**: Visible focus states for all interactive elements
- **Color Independence**: Information conveyed by more than just color
- **Text Alternatives**: Icons accompanied by text or tooltips
