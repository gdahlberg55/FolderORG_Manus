# Component Style Guide

This guide provides detailed specifications for key UI components to ensure consistency throughout the application.

## Buttons

### Primary Button
- **Purpose**: For main actions on a screen
- **Style**: Filled button with primary color
- **States**:
  - Normal: Primary color (#TBD), white text, elevation 2dp
  - Hover: 10% darker primary color, elevation 4dp
  - Pressed: 20% darker primary color, elevation 8dp
  - Disabled: 70% opacity, no elevation
- **Sizing**: 
  - Height: 36px
  - Padding: 16px horizontal, 0px vertical
  - Corner radius: 4px
- **Typography**: 14px, medium weight, all caps, centered

```xml
<Button Style="{StaticResource MaterialDesignRaisedButton}"
        materialDesign:ButtonAssist.CornerRadius="4"
        Content="PRIMARY ACTION" />
```

### Secondary Button
- **Purpose**: For secondary actions within a flow
- **Style**: Outlined button with primary color border
- **States**:
  - Normal: Transparent fill, primary color border (1px), primary color text
  - Hover: 5% primary color fill, elevation 1dp
  - Pressed: 10% primary color fill, elevation 2dp
  - Disabled: 70% opacity, no elevation
- **Sizing**: Same as Primary Button
- **Typography**: Same as Primary Button

```xml
<Button Style="{StaticResource MaterialDesignOutlinedButton}"
        materialDesign:ButtonAssist.CornerRadius="4"
        Content="SECONDARY ACTION" />
```

### Text Button
- **Purpose**: For tertiary actions, often in dialogs
- **Style**: Text only (no border or background)
- **States**:
  - Normal: Primary color text
  - Hover: 5% primary color fill
  - Pressed: 10% primary color fill
  - Disabled: 50% opacity
- **Sizing**: 
  - Height: 36px
  - Padding: 8px horizontal, 0px vertical
- **Typography**: Same as Primary Button

```xml
<Button Style="{StaticResource MaterialDesignTextButton}"
        Content="TEXT ACTION" />
```

### Icon Button
- **Purpose**: For actions represented by icons
- **Style**: Icon only with transparent background
- **States**: Similar to Text Button
- **Sizing**: 
  - Touch target: 48x48px (minimum)
  - Icon size: 24x24px
- **Layout**: Centered icon with padding

```xml
<Button Style="{StaticResource MaterialDesignIconButton}">
    <materialDesign:PackIcon Kind="ContentSave" Width="24" Height="24" />
</Button>
```

## Cards

### Standard Card
- **Purpose**: Container for related content
- **Style**: 
  - Background: White (light theme) / #424242 (dark theme)
  - Elevation: 1dp (resting), 8dp (raised)
  - Corner radius: 4px
  - Border: None
- **Spacing**:
  - Padding: 16px
  - Content spacing: 8px between elements
- **Typography**: Follows typography hierarchy

```xml
<materialDesign:Card Margin="8" 
                     Padding="16" 
                     materialDesign:ShadowAssist.ShadowDepth="Depth1"
                     UniformCornerRadius="4">
    <!-- Card content -->
</materialDesign:Card>
```

### Action Card
- **Purpose**: Card with interactive elements
- **Style**: Similar to Standard Card
- **Layout**:
  - Title area: Top
  - Content area: Middle
  - Action buttons: Bottom-right aligned
- **Actions**: 1-3 buttons, using Text or Icon buttons

```xml
<materialDesign:Card Margin="8" 
                     materialDesign:ShadowAssist.ShadowDepth="Depth1"
                     UniformCornerRadius="4">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Title -->
        <TextBlock Grid.Row="0" 
                   Style="{StaticResource Title}" 
                   Margin="16,16,16,0" 
                   Text="Card Title"/>
        
        <!-- Content -->
        <StackPanel Grid.Row="1" Margin="16,8,16,8">
            <TextBlock Style="{StaticResource Body}" 
                       Text="Card content..." 
                       TextWrapping="Wrap"/>
        </StackPanel>
        
        <!-- Actions -->
        <StackPanel Grid.Row="2" 
                    Orientation="Horizontal" 
                    HorizontalAlignment="Right" 
                    Margin="8">
            <Button Style="{StaticResource MaterialDesignTextButton}" 
                    Content="CANCEL" 
                    Margin="0,0,8,0"/>
            <Button Style="{StaticResource MaterialDesignRaisedButton}" 
                    Content="SAVE"/>
        </StackPanel>
    </Grid>
</materialDesign:Card>
```

## Text Fields

### Standard Text Field
- **Purpose**: Single-line text input
- **Style**: 
  - Outlined box with label
  - 1px stroke in resting state
  - Corner radius: 4px
- **States**:
  - Resting: Light gray outline, floating label above
  - Focused: Primary color outline (2px), primary color label
  - Filled: Black text on white background
  - Error: Error color outline, error message below
  - Disabled: 50% opacity, no interaction
- **Sizing**:
  - Height: 56px
  - Label: 12px when floating, 16px when resting
  - Text: 16px
  - Helper text: 12px
- **Layout**:
  - Text: Left-aligned
  - Label: Left-aligned
  - Helper text: Left-aligned, 4px below input

```xml
<TextBox Style="{StaticResource MaterialDesignOutlinedTextBox}"
         materialDesign:HintAssist.Hint="Label text"
         materialDesign:TextFieldAssist.TextBoxViewMargin="1,0,1,0"
         materialDesign:HintAssist.HelperText="Helper text"
         Width="280" />
```

### Multiline Text Field
- **Purpose**: Multiple line text input
- **Style**: Same as Standard Text Field
- **Sizing**:
  - Min Height: 56px
  - Max Height: Variable or fixed based on context
- **Behavior**: 
  - Vertically expands to fit content up to max height
  - Shows scrollbar when content exceeds max height

```xml
<TextBox Style="{StaticResource MaterialDesignOutlinedTextBox}"
         materialDesign:HintAssist.Hint="Description"
         materialDesign:TextFieldAssist.TextBoxViewMargin="1,0,1,0"
         Width="280"
         MinHeight="56"
         MaxHeight="120"
         TextWrapping="Wrap"
         AcceptsReturn="True"
         VerticalScrollBarVisibility="Auto" />
```

## Selection Controls

### Checkbox
- **Purpose**: Multi-select option
- **Style**: 
  - Size: 18x18px
  - States:
    - Unchecked: Outline only
    - Checked: Filled with primary color, white checkmark
    - Disabled: 50% opacity
- **Layout**:
  - Label: Right of checkbox, 8px spacing
  - Touch target: 48x48px minimum

```xml
<CheckBox Content="Option label" 
          Margin="0,8"/>
```

### Radio Button
- **Purpose**: Single-select from a group
- **Style**: 
  - Size: 18px diameter
  - States:
    - Unselected: Outline circle
    - Selected: Outer ring with primary color filled center
    - Disabled: 50% opacity
- **Layout**: Same as Checkbox

```xml
<RadioButton Content="Option one" 
             GroupName="OptionGroup" 
             Margin="0,8"/>
```

### Switch
- **Purpose**: Toggle setting on/off
- **Style**: 
  - Track: 36px width, 14px height, rounded ends
  - Thumb: 20px diameter circle
  - Colors:
    - Off: Gray track, light gray thumb
    - On: Primary color (50% opacity) track, primary color thumb
- **States**:
  - Off: Thumb positioned left
  - On: Thumb positioned right
  - Disabled: 50% opacity
- **Animation**: Smooth slide between states (150ms)

```xml
<ToggleButton Style="{StaticResource MaterialDesignSwitchToggleButton}" 
              Content="Setting" 
              ToolTip="Enable/disable setting"/>
```

## Lists

### Standard List
- **Purpose**: Vertical list of items
- **Style**: 
  - Item height: 48px (single line), 64px (two lines), 88px (three lines)
  - Dividers: 1px light gray line (optional)
- **States**:
  - Normal: No background
  - Hover: Light gray background
  - Selected: Light primary color background
  - Active: Primary color text
- **Typography**:
  - Primary text: 16px, regular
  - Secondary text: 14px, medium opacity
- **Layout**:
  - Text padding: 16px left/right
  - Multiple lines: 4px spacing between lines

```xml
<ListView ItemsSource="{Binding Items}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <Grid Height="64">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <!-- Leading icon (optional) -->
                <materialDesign:PackIcon Kind="File" 
                                       Width="24" 
                                       Height="24" 
                                       VerticalAlignment="Center" 
                                       Margin="16,0,0,0"/>
                                       
                <StackPanel Grid.Column="1" Margin="16,0,16,0" VerticalAlignment="Center">
                    <TextBlock Text="{Binding PrimaryText}" Style="{StaticResource Body}"/>
                    <TextBlock Text="{Binding SecondaryText}" 
                              Style="{StaticResource Caption}" 
                              Opacity="0.6"/>
                </StackPanel>
            </Grid>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

## Navigation Components

### App Bar
- **Purpose**: Primary navigation and actions
- **Style**: 
  - Height: 64px
  - Background: Primary color
  - Elevation: 4dp
- **Layout**:
  - Left: Navigation menu button or app logo
  - Center: Title (optional)
  - Right: Action buttons
- **Typography**:
  - Title: 20px, medium weight, white

```xml
<materialDesign:ColorZone Mode="PrimaryMid" 
                         Padding="16" 
                         materialDesign:ShadowAssist.ShadowDepth="Depth2">
    <DockPanel>
        <ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}" 
                      DockPanel.Dock="Left"/>
        <TextBlock Style="{StaticResource Title}" 
                  VerticalAlignment="Center" 
                  Text="Application Title"/>
        <StackPanel Orientation="Horizontal" DockPanel.Dock="Right">
            <Button Style="{StaticResource MaterialDesignToolButton}" ToolTip="Settings">
                <materialDesign:PackIcon Kind="Cog"/>
            </Button>
            <Button Style="{StaticResource MaterialDesignToolButton}" ToolTip="Help">
                <materialDesign:PackIcon Kind="Help"/>
            </Button>
        </StackPanel>
    </DockPanel>
</materialDesign:ColorZone>
```

### Tabs
- **Purpose**: Switch between related views
- **Style**: 
  - Height: 48px
  - Indicator: 2px primary color line
  - Background: Transparent or surface color
- **States**:
  - Active: Primary color text, indicator visible
  - Inactive: Medium opacity text, no indicator
  - Disabled: Low opacity text
- **Layout**:
  - Equal width or content-based width
  - Center-aligned text
  - Icon above text (optional)

```xml
<materialDesign:ColorZone Mode="Standard" Padding="8" materialDesign:ShadowAssist.ShadowDepth="Depth1">
    <TabControl Style="{StaticResource MaterialDesignTabControl}" materialDesign:ColorZoneAssist.Mode="Standard">
        <TabItem Header="FILES">
            <TextBlock Margin="16" Text="Files content goes here"/>
        </TabItem>
        <TabItem Header="RULES">
            <TextBlock Margin="16" Text="Rules content goes here"/>
        </TabItem>
        <TabItem Header="HISTORY">
            <TextBlock Margin="16" Text="History content goes here"/>
        </TabItem>
    </TabControl>
</materialDesign:ColorZone>
```

## Dialogs

### Standard Dialog
- **Purpose**: User confirmation or input
- **Style**: 
  - Background: Surface color
  - Elevation: 24dp
  - Corner radius: 4px
  - Max width: 560px
  - Min width: 280px
- **Layout**:
  - Title: Top, 24px padding
  - Content: Middle, 24px padding sides, 16px padding top/bottom
  - Actions: Bottom, right-aligned, 8px padding
- **Animation**: 
  - Entry: Scale up (150ms)
  - Exit: Scale down (150ms)

```xml
<materialDesign:DialogHost IsOpen="{Binding IsDialogOpen}">
    <materialDesign:DialogHost.DialogContent>
        <StackPanel Margin="16">
            <TextBlock Style="{StaticResource Title}" Text="Dialog Title"/>
            <TextBlock Style="{StaticResource Body}" 
                      Text="Dialog content goes here." 
                      TextWrapping="Wrap" 
                      Margin="0,16"/>
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Style="{StaticResource MaterialDesignTextButton}" 
                        Command="{Binding CancelCommand}" 
                        Content="CANCEL" 
                        Margin="0,0,8,0"/>
                <Button Style="{StaticResource MaterialDesignRaisedButton}" 
                        Command="{Binding ConfirmCommand}" 
                        Content="CONFIRM"/>
            </StackPanel>
        </StackPanel>
    </materialDesign:DialogHost.DialogContent>
    
    <!-- Main content behind dialog -->
    <Grid>
        <!-- Main app content -->
    </Grid>
</materialDesign:DialogHost>
```

## Progress Indicators

### Linear Progress
- **Purpose**: Show determinate or indeterminate progress
- **Style**: 
  - Height: 4px
  - Color: Primary color
  - Background: Light primary color (10% opacity)
- **Types**:
  - Determinate: Filled portion represents progress percentage
  - Indeterminate: Animated bar that moves continuously

```xml
<!-- Determinate -->
<ProgressBar Value="70" Maximum="100" />

<!-- Indeterminate -->
<ProgressBar IsIndeterminate="True" />
```

### Circular Progress
- **Purpose**: Loading indicators, especially for smaller areas
- **Style**: 
  - Size: 40px diameter (standard)
  - Stroke width: 3.5px
  - Color: Primary color
- **Types**:
  - Determinate: Circle fills based on progress percentage
  - Indeterminate: Continuous spinning animation

```xml
<!-- Determinate -->
<ProgressBar Style="{StaticResource MaterialDesignCircularProgressBar}" 
             Value="70" 
             Maximum="100" />

<!-- Indeterminate -->
<ProgressBar Style="{StaticResource MaterialDesignCircularProgressBar}" 
             IsIndeterminate="True" />
```

## Data Visualization

### Data Tables
- **Purpose**: Display structured data in rows and columns
- **Style**: 
  - Row height: 48px
  - Dividers: 1px light gray horizontal lines
  - Header: Medium weight text, uppercase
- **States**:
  - Normal row: No background
  - Hover: Light gray background
  - Selected: Light primary color background
- **Typography**:
  - Header text: 12px, medium weight, uppercase
  - Row text: 14px, regular
- **Layout**:
  - Cell padding: 16px
  - Align text left for text, right for numbers

```xml
<DataGrid ItemsSource="{Binding Items}" 
          AutoGenerateColumns="False" 
          CanUserAddRows="False" 
          materialDesign:DataGridAssist.CellPadding="16" 
          materialDesign:DataGridAssist.ColumnHeaderPadding="8">
    <DataGrid.Columns>
        <DataGridTextColumn Header="NAME" 
                           Binding="{Binding Name}" 
                           Width="*"/>
        <DataGridTextColumn Header="TYPE" 
                           Binding="{Binding Type}" 
                           Width="*"/>
        <DataGridTextColumn Header="SIZE" 
                           Binding="{Binding Size}" 
                           Width="AUTO"/>
        <DataGridTextColumn Header="DATE MODIFIED" 
                           Binding="{Binding DateModified}" 
                           Width="*"/>
    </DataGrid.Columns>
</DataGrid>
```

## Component Usage Guidelines

1. **Consistency**: Use components consistently for the same purpose throughout the application
2. **Hierarchy**: Maintain proper visual hierarchy - use primary components for main actions
3. **Density**: Don't overcrowd interfaces - maintain proper spacing between components
4. **Simplicity**: Use the simplest component that meets the need
5. **Accessibility**: Ensure all components meet accessibility standards
6. **Responsive**: Components should adapt appropriately to different screen sizes
7. **Familiarity**: Follow established patterns for component usage to meet user expectations 