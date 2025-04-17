# WPF Material Design Implementation Guide

This guide provides practical implementation steps for applying our UI Design Guidelines to the WPF application using Material Design.

## Setup and Dependencies

### Required Packages

```xml
<!-- In your .csproj file -->
<PackageReference Include="MaterialDesignThemes" Version="4.8.0" />
<PackageReference Include="MaterialDesignColors" Version="2.1.2" />
<PackageReference Include="MaterialDesignExtensions" Version="3.3.0" />
```

### App.xaml Configuration

```xml
<Application x:Class="FolderORG.Manus.UI.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design Theme -->
                <materialDesign:BundledTheme BaseTheme="Light" PrimaryColor="Indigo" SecondaryColor="DeepPurple" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />

                <!-- Additional Resources -->
                <ResourceDictionary Source="Resources/Typography.xaml" />
                <ResourceDictionary Source="Resources/Colors.xaml" />
                <ResourceDictionary Source="Resources/ControlStyles.xaml" />
            </ResourceDictionary.MergedDictionaries>
            
            <!-- Global Settings -->
            <Style TargetType="{x:Type Control}" x:Key="BaseStyle">
                <Setter Property="Margin" Value="8" />
            </Style>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## Theme Configuration

### Colors.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <!-- Primary and Secondary colors will be defined in branding -->
    
    <!-- Functional Colors -->
    <Color x:Key="SuccessColor">#4CAF50</Color>
    <Color x:Key="WarningColor">#FF9800</Color>
    <Color x:Key="ErrorColor">#F44336</Color>
    <Color x:Key="InfoColor">#2196F3</Color>
    
    <!-- Surface Colors -->
    <Color x:Key="SurfaceColor">#FFFFFF</Color>
    <Color x:Key="BackgroundColor">#F5F5F5</Color>
    <Color x:Key="OnSurfaceColor">#212121</Color>
    
    <!-- Brush Resources -->
    <SolidColorBrush x:Key="SuccessBrush" Color="{StaticResource SuccessColor}" />
    <SolidColorBrush x:Key="WarningBrush" Color="{StaticResource WarningColor}" />
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}" />
    <SolidColorBrush x:Key="InfoBrush" Color="{StaticResource InfoColor}" />
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}" />
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}" />
    <SolidColorBrush x:Key="OnSurfaceBrush" Color="{StaticResource OnSurfaceColor}" />
</ResourceDictionary>
```

### Typography.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <!-- Typography -->
    <Style x:Key="DisplayLarge" TargetType="TextBlock" BasedOn="{StaticResource MaterialDesignTextBlock}">
        <Setter Property="FontSize" Value="34" />
        <Setter Property="FontWeight" Value="Regular" />
        <Setter Property="LineHeight" Value="51" />
    </Style>
    
    <Style x:Key="DisplayMedium" TargetType="TextBlock" BasedOn="{StaticResource MaterialDesignTextBlock}">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontWeight" Value="Regular" />
        <Setter Property="LineHeight" Value="36" />
    </Style>
    
    <Style x:Key="Title" TargetType="TextBlock" BasedOn="{StaticResource MaterialDesignTextBlock}">
        <Setter Property="FontSize" Value="20" />
        <Setter Property="FontWeight" Value="Medium" />
        <Setter Property="LineHeight" Value="30" />
    </Style>
    
    <Style x:Key="Subtitle" TargetType="TextBlock" BasedOn="{StaticResource MaterialDesignTextBlock}">
        <Setter Property="FontSize" Value="16" />
        <Setter Property="FontWeight" Value="Regular" />
        <Setter Property="LineHeight" Value="24" />
    </Style>
    
    <Style x:Key="Body" TargetType="TextBlock" BasedOn="{StaticResource MaterialDesignTextBlock}">
        <Setter Property="FontSize" Value="14" />
        <Setter Property="FontWeight" Value="Regular" />
        <Setter Property="LineHeight" Value="21" />
    </Style>
    
    <Style x:Key="Caption" TargetType="TextBlock" BasedOn="{StaticResource MaterialDesignTextBlock}">
        <Setter Property="FontSize" Value="12" />
        <Setter Property="FontWeight" Value="Regular" />
        <Setter Property="LineHeight" Value="18" />
    </Style>
</ResourceDictionary>
```

## Layout Implementation

### Grid Structure

Use Grid with appropriate column and row definitions:

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <!-- 12-column grid, each column is * width -->
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <!-- Repeat for 12 columns -->
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <!-- Define rows with appropriate heights -->
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Content with grid positioning -->
</Grid>
```

### Spacing and Margins

Apply consistent spacing:

```xml
<StackPanel Margin="24">
    <TextBlock Style="{StaticResource Title}" Margin="0,0,0,16" Text="Section Title"/>
    <TextBlock Style="{StaticResource Body}" Margin="0,0,0,8" Text="Content description..."/>
    <Button Style="{StaticResource MaterialDesignRaisedButton}" Margin="0,16,0,0" Content="Primary Action"/>
</StackPanel>
```

## Component Examples

### Card Layout

```xml
<materialDesign:Card Margin="8" Padding="16" materialDesign:ShadowAssist.ShadowDepth="Depth1">
    <StackPanel>
        <TextBlock Style="{StaticResource Title}" Text="Card Title"/>
        <Separator Margin="0,8,0,8"/>
        <TextBlock Style="{StaticResource Body}" Text="Card content goes here..." TextWrapping="Wrap"/>
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,16,0,0">
            <Button Style="{StaticResource MaterialDesignTextButton}" Content="Cancel" Margin="0,0,8,0"/>
            <Button Style="{StaticResource MaterialDesignRaisedButton}" Content="Submit"/>
        </StackPanel>
    </StackPanel>
</materialDesign:Card>
```

### Navigation Drawer

```xml
<materialDesign:DrawerHost IsLeftDrawerOpen="{Binding IsMenuOpen}">
    <materialDesign:DrawerHost.LeftDrawerContent>
        <DockPanel MinWidth="220">
            <StackPanel DockPanel.Dock="Top" Margin="0,0,0,16">
                <Image Source="Assets/logo.png" Height="60" Margin="16"/>
                <TextBlock Style="{StaticResource Title}" Margin="16,0" Text="Application Name"/>
            </StackPanel>
            
            <ListBox x:Name="NavigationItems" 
                     SelectedIndex="{Binding SelectedViewIndex}"
                     ItemContainerStyle="{StaticResource MaterialDesignListBoxItem}">
                <ListBoxItem>
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="ViewDashboard" Margin="0,0,8,0" VerticalAlignment="Center"/>
                        <TextBlock Text="Dashboard"/>
                    </StackPanel>
                </ListBoxItem>
                <!-- Add more navigation items -->
            </ListBox>
        </DockPanel>
    </materialDesign:DrawerHost.LeftDrawerContent>
    
    <DockPanel>
        <!-- App Bar -->
        <materialDesign:ColorZone Mode="PrimaryMid" DockPanel.Dock="Top" Padding="16" materialDesign:ShadowAssist.ShadowDepth="Depth1">
            <DockPanel>
                <ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}" 
                              IsChecked="{Binding IsMenuOpen, Mode=TwoWay}" 
                              DockPanel.Dock="Left"/>
                <TextBlock Style="{StaticResource Title}" VerticalAlignment="Center" Text="Current View"/>
            </DockPanel>
        </materialDesign:ColorZone>
        
        <!-- Main Content Area -->
        <Grid>
            <ContentControl Content="{Binding CurrentView}"/>
        </Grid>
    </DockPanel>
</materialDesign:DrawerHost>
```

### File Preview Card

```xml
<materialDesign:Card Margin="8" materialDesign:ShadowAssist.ShadowDepth="Depth1">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="140"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Preview Area -->
        <Border Grid.Row="0" Background="{DynamicResource PrimaryHueLightBrush}">
            <materialDesign:PackIcon Kind="File" Width="64" Height="64" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        
        <!-- File Info -->
        <StackPanel Grid.Row="1" Margin="16">
            <TextBlock Style="{StaticResource Subtitle}" Text="{Binding FileName}" TextTrimming="CharacterEllipsis"/>
            <TextBlock Style="{StaticResource Caption}" Text="{Binding FileType}" Opacity="0.6"/>
            <TextBlock Style="{StaticResource Caption}" Text="{Binding FileSize}" Opacity="0.6" Margin="0,4,0,0"/>
            <TextBlock Style="{StaticResource Caption}" Text="{Binding LastModified}" Opacity="0.6"/>
        </StackPanel>
        
        <!-- Actions -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" Margin="8" HorizontalAlignment="Right">
            <Button Style="{StaticResource MaterialDesignToolButton}" ToolTip="Preview">
                <materialDesign:PackIcon Kind="Eye"/>
            </Button>
            <Button Style="{StaticResource MaterialDesignToolButton}" ToolTip="Move">
                <materialDesign:PackIcon Kind="FolderMove"/>
            </Button>
            <Button Style="{StaticResource MaterialDesignToolButton}" ToolTip="More Options">
                <materialDesign:PackIcon Kind="DotsVertical"/>
            </Button>
        </StackPanel>
    </Grid>
</materialDesign:Card>
```

## Animation Implementation

### Transitions

For page transitions:

```csharp
// In App.xaml.cs or your navigation service
public void NavigateTo(UserControl view)
{
    // Fade out current view
    var currentView = CurrentView;
    var animation = new DoubleAnimation
    {
        From = 1.0,
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(150)
    };
    
    animation.Completed += (s, e) =>
    {
        // Switch to new view
        CurrentView = view;
        
        // Fade in new view
        var fadeIn = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(150)
        };
        
        view.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    };
    
    currentView.BeginAnimation(UIElement.OpacityProperty, animation);
}
```

### Loading States

```xml
<materialDesign:Card>
    <Grid>
        <StackPanel x:Name="Content">
            <!-- Card content -->
        </StackPanel>
        
        <materialDesign:Card x:Name="LoadingOverlay" Background="#80FFFFFF" Visibility="Collapsed">
            <ProgressBar Style="{StaticResource MaterialDesignCircularProgressBar}" 
                         IsIndeterminate="True" 
                         HorizontalAlignment="Center" 
                         VerticalAlignment="Center"/>
        </materialDesign:Card>
    </Grid>
</materialDesign:Card>
```

```csharp
// Show loading state
private void ShowLoading()
{
    Content.IsEnabled = false;
    
    var fadeIn = new DoubleAnimation
    {
        From = 0.0,
        To = 1.0,
        Duration = TimeSpan.FromMilliseconds(200)
    };
    
    LoadingOverlay.Visibility = Visibility.Visible;
    LoadingOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
}

// Hide loading state
private void HideLoading()
{
    var fadeOut = new DoubleAnimation
    {
        From = 1.0,
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(200)
    };
    
    fadeOut.Completed += (s, e) =>
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        Content.IsEnabled = true;
    };
    
    LoadingOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
}
```

## Responsive Design Implementation

### Responsive Grid

```xml
<Grid>
    <!-- Responsive layout triggered by window width -->
    <Grid.Style>
        <Style TargetType="Grid">
            <Style.Triggers>
                <DataTrigger Binding="{Binding ActualWidth, RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Window}}}" Value="600">
                    <Setter Property="Tag" Value="Small"/>
                </DataTrigger>
                <DataTrigger Binding="{Binding ActualWidth, RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Window}}}" Value="960">
                    <Setter Property="Tag" Value="Medium"/>
                </DataTrigger>
                <DataTrigger Binding="{Binding ActualWidth, RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Window}}}" Value="1280">
                    <Setter Property="Tag" Value="Large"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Grid.Style>
    
    <!-- Content with responsive adjustments -->
    <ItemsControl ItemsSource="{Binding Items}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <WrapPanel />
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <!-- Card with responsive width -->
                <materialDesign:Card Margin="8">
                    <materialDesign:Card.Style>
                        <Style TargetType="materialDesign:Card" BasedOn="{StaticResource {x:Type materialDesign:Card}}">
                            <Setter Property="Width" Value="360"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Tag, RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Grid}}}" Value="Small">
                                    <Setter Property="Width" Value="280"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Tag, RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Grid}}}" Value="Medium">
                                    <Setter Property="Width" Value="320"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </materialDesign:Card.Style>
                    
                    <!-- Card content -->
                </materialDesign:Card>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Grid>
```

## Accessibility Implementation

### Focus Indicators

```xml
<Style TargetType="Button" BasedOn="{StaticResource MaterialDesignRaisedButton}">
    <Style.Triggers>
        <Trigger Property="IsKeyboardFocused" Value="True">
            <Setter Property="materialDesign:ButtonAssist.CornerRadius" Value="6"/>
            <Setter Property="BorderBrush" Value="{DynamicResource PrimaryHueDarkBrush}"/>
            <Setter Property="BorderThickness" Value="2"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### Screen Reader Support

```xml
<Button Content="Add File" 
        materialDesign:ButtonAssist.CornerRadius="4"
        ToolTip="Add new file to the project"
        AutomationProperties.Name="Add New File"
        AutomationProperties.HelpText="Click to add a new file to the current project">
    <Button.AccessKey>A</Button.AccessKey>
</Button>
```

## Theme Switching

```csharp
public void ToggleTheme()
{
    var paletteHelper = new PaletteHelper();
    ITheme theme = paletteHelper.GetTheme();
    
    theme.SetBaseTheme(theme.GetBaseTheme() == BaseTheme.Dark 
        ? Theme.Light 
        : Theme.Dark);
    
    paletteHelper.SetTheme(theme);
}
```

## UI Component Development Process

1. **Design**: Start with a mockup in the ui_mockups folder
2. **XAML Implementation**: 
   - Create XAML following Material Design guidelines
   - Use predefined styles and resources
   - Implement responsive behavior
3. **View Model Connection**:
   - Define proper MVVM bindings
   - Implement INotifyPropertyChanged
   - Use RelayCommand for actions
4. **Testing**:
   - Verify appearance in both light and dark themes
   - Test at different window sizes
   - Ensure keyboard accessibility
   - Check screen reader compatibility

## Common Problems and Solutions

### Card Layout Issues

Problem: Cards have inconsistent appearance or layout.
Solution: Always use the MaterialDesignCard and maintain consistent padding (16px) and margin (8px).

### Text Overflow

Problem: Text overflows containers.
Solution: Use TextTrimming="CharacterEllipsis" and TextWrapping="Wrap" appropriately.

### Animation Performance

Problem: Animations cause UI lag.
Solution: Keep animations short (under 300ms) and use hardware acceleration where possible.

```xml
<Button RenderTransformOrigin="0.5,0.5">
    <Button.RenderTransform>
        <ScaleTransform x:Name="AnimatedScaleTransform" ScaleX="1" ScaleY="1" />
    </Button.RenderTransform>
</Button>
```

### Theme Consistency

Problem: Some elements don't update when theme changes.
Solution: Always use DynamicResource instead of StaticResource for theme-related resources.

```xml
<TextBlock Foreground="{DynamicResource MaterialDesignBody}" />
```

### Icon Consistency

Problem: Icons appear at inconsistent sizes.
Solution: Always set both Width and Height on PackIcon elements.

```xml
<materialDesign:PackIcon Kind="FileDocument" Width="24" Height="24" />
```

## UI Testing Checklist

Before submitting UI changes:

- [ ] Appearance matches design mockups
- [ ] Works correctly in both light and dark themes
- [ ] Responsive layout adapts to different window sizes
- [ ] Animation is smooth and purposeful
- [ ] All text is readable with sufficient contrast
- [ ] Interactive elements have proper states (hover, pressed, disabled)
- [ ] Keyboard navigation works as expected
- [ ] Screen reader can access all elements with proper descriptions
- [ ] UI performance is smooth with no lag or flicker 