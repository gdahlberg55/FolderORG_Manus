# Dashboard UI Mockup

## Overview
The main dashboard provides users with a comprehensive overview of the file organization system status, recent activities, and quick access to key functions. The design follows a clean, modern aesthetic with a focus on clarity and usability.

```
+----------------------------------------------------------------------+
|  File Organizer Pro                                     [_] [□] [X]  |
+----------------------------------------------------------------------+
| [≡] | Dashboard | Rules | Files | Reports | Settings |    [🔍] [👤]  |
+----------------------------------------------------------------------+
|                                                                      |
|  +------------------------+  +--------------------------------+      |
|  | SYSTEM STATUS          |  | STORAGE OVERVIEW               |      |
|  +------------------------+  +--------------------------------+      |
|  |                        |  |                                |      |
|  | ● Active               |  | [███████████████████     ]     |      |
|  | Last Scan: Today 14:30 |  | 789.5 GB used of 1.2 TB (65%) |      |
|  | Files Managed: 145,782 |  |                                |      |
|  | Rules Active: 24       |  | C: [████████████] 65%          |      |
|  |                        |  | D: [██████] 30%                |      |
|  | [Scan Now]             |  | E: [█████████████████] 85%     |      |
|  +------------------------+  +--------------------------------+      |
|                                                                      |
|  +------------------------+  +--------------------------------+      |
|  | RECENT ACTIVITY        |  | ORGANIZATION OPPORTUNITIES    |      |
|  +------------------------+  +--------------------------------+      |
|  |                        |  |                                |      |
|  | ↓ 124 files organized  |  | 📁 Downloads folder (342 files)|      |
|  | ↑ 56 files moved       |  | 📁 Desktop (127 files)         |      |
|  | ✓ 89 files classified  |  | 📁 Documents (215 files)       |      |
|  | 🗑️ 45 files cleaned up  |  |                                |      |
|  |                        |  | [Organize] [View Details]      |      |
|  | [View Activity Log]    |  |                                |      |
|  +------------------------+  +--------------------------------+      |
|                                                                      |
|  +------------------------------------------------------------------+|
|  | QUICK ACTIONS                                                    ||
|  +------------------------------------------------------------------+|
|  |                                                                  ||
|  | [+ New Rule] [🔄 Run Organization] [🧹 Cleanup] [⚙️ Settings]     ||
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

### 2. System Status Panel
- **Status Indicator**: Shows current system state (Active, Idle, Scanning)
- **Last Scan Information**: Date and time of the most recent scan
- **Files Managed**: Total number of files under management
- **Rules Active**: Number of active organization rules
- **Scan Now Button**: Triggers an immediate system scan

### 3. Storage Overview Panel
- **Overall Storage Usage**: Visual bar showing total storage utilization
- **Drive-Specific Usage**: Individual bars for each drive/volume
- **Space Metrics**: Used space, total space, and percentage

### 4. Recent Activity Panel
- **Activity Summary**: Icons and counts for recent organization actions
- **Activity Types**: Files organized, moved, classified, cleaned up
- **View Activity Log Button**: Opens detailed activity history

### 5. Organization Opportunities Panel
- **Target Locations**: Folders that would benefit from organization
- **File Counts**: Number of files in each location
- **Action Buttons**: Quick access to organize or view details

### 6. Quick Actions Bar
- **New Rule Button**: Creates a new organization rule
- **Run Organization Button**: Triggers the organization process
- **Cleanup Button**: Initiates the cleanup process
- **Settings Button**: Quick access to system settings

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
- **Metrics**: 14px for numbers and statistics

## Responsive Behavior
- **Desktop**: Full layout as shown above
- **Laptop**: Panels reorganize into 2x2 grid
- **Tablet**: Panels stack vertically, full width
- **Mobile**: Simplified view with essential information only

## Accessibility Features
- **High Contrast Mode**: Alternative color scheme for visibility
- **Keyboard Navigation**: Full keyboard control with visible focus indicators
- **Screen Reader Support**: All elements properly labeled
- **Text Scaling**: Interface adjusts to system text size settings
