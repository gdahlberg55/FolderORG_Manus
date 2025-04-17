# Cline's Memory Bank Guidelines

## Overview
This document outlines the Memory Bank methodology used for the FolderORG Manus project. The Memory Bank system ensures perfect continuity between development sessions by maintaining comprehensive documentation that can be referenced at the start of each new task.

## Memory Bank Structure

The Memory Bank consists of core files and optional context files, all in Markdown format. Files build upon each other in a clear hierarchy:

```
flowchart TD
    PB[projectbrief.md] --> PC[productContext.md]
    PB --> SP[systemPatterns.md]
    PB --> TC[techContext.md]
    
    PC --> AC[activeContext.md]
    SP --> AC
    TC --> AC
    
    AC --> P[progress.md]
```

### Core Files (Required)
1. `projectbrief.md`
   - Foundation document that shapes all other files
   - Defines core requirements and goals
   - Source of truth for project scope

2. `productContext.md`
   - Why this project exists
   - Problems it solves
   - How it should work
   - User experience goals

3. `activeContext.md`
   - Current work focus
   - Recent changes
   - Next steps
   - Active decisions and considerations
   - Important patterns and preferences
   - Learnings and project insights

4. `systemPatterns.md`
   - System architecture
   - Key technical decisions
   - Design patterns in use
   - Component relationships
   - Critical implementation paths

5. `techContext.md`
   - Technologies used
   - Development setup
   - Technical constraints
   - Dependencies
   - Tool usage patterns

6. `progress.md`
   - What works
   - What's left to build
   - Current status
   - Known issues
   - Evolution of project decisions

7. `project_context.md`
   - Comprehensive overview file for quick context restoration
   - Summarizes project status, organization, and focus

### Additional Context Files
Feature-specific documentation is stored in the memory-bank directory:
- `backup_restore_progress.md` - Details on the Backup/Restore system implementation
- `path_validation_system.md` - Documentation on the Path Validation System

## Core Workflows

### Plan Mode
```
flowchart TD
    Start[Start] --> ReadFiles[Read Memory Bank]
    ReadFiles --> CheckFiles{Files Complete?}
    
    CheckFiles -->|No| Plan[Create Plan]
    Plan --> Document[Document in Chat]
    
    CheckFiles -->|Yes| Verify[Verify Context]
    Verify --> Strategy[Develop Strategy]
    Strategy --> Present[Present Approach]
```

### Act Mode
```
flowchart TD
    Start[Start] --> Context[Check Memory Bank]
    Context --> Update[Update Documentation]
    Update --> Execute[Execute Task]
    Execute --> Document[Document Changes]
```

## Documentation Updates

Memory Bank updates occur when:
1. Discovering new project patterns
2. After implementing significant changes
3. When requested with **update memory bank** (MUST review ALL files)
4. When context needs clarification

```
flowchart TD
    Start[Update Process]
    
    subgraph Process
        P1[Review ALL Files]
        P2[Document Current State]
        P3[Clarify Next Steps]
        P4[Document Insights & Patterns]
        
        P1 --> P2 --> P3 --> P4
    end
    
    Start --> Process
```

## Guidelines for FolderORG Manus

For the FolderORG Manus project:
1. ALWAYS read all memory bank files at the start of each session
2. Focus particularly on activeContext.md, progress.md, and project_context.md as they track current state
3. Update documentation after implementing significant changes
4. Ensure all technical decisions are documented in systemPatterns.md
5. Keep progress.md up-to-date with completion percentages
6. Document new patterns and insights as they emerge
7. When updating memory bank files, review ALL files even if some don't require updates

## Relation to Project Structure

The Memory Bank complements the project's organized directory structure:
- `docs/` - Contains project documentation and architecture details
- `deliverables/` - Contains project deliverables and specifications
- `testing/` - Contains test plans and results
- `memory-bank/` - Contains the project memory and context for continuity

The Memory Bank provides the "why" and current status, while the other directories contain the "what" and "how" of the project. 