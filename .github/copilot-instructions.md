# Project Requirements

## Technology Stack
- WINUI3 Application
- XAML for UI

## Coding Guidelines

### Code Style
- Use PascalCase for public members
- Use camelCase for private fields with _ prefix
- Always use explicit types instead of var

### XAML Conventions
- Resource dictionaries should be organized by type
- Use consistent spacing and indentation
- Prefer styles over inline properties

### Architecture
- Follow MVVM pattern
- Use MVVM Toolkit
- Keep code-behind minimal
- ViewModels should implement INotifyPropertyChanged

## Naming Conventions
- Views end with "Page" or "Window"
- ViewModels end with "ViewModel"
- Styles organized in separate resource files