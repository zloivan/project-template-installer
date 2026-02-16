# Template Tutorial System

## Overview

The Template Tutorial System provides detailed, actionable guidance for developers after installing a template. It automatically opens after successful template installation and can be accessed anytime via the Unity menu.

## Features

### 📚 Comprehensive Guides
- **Overview**: What the template is designed for
- **What Was Created**: Complete list of folders, scenes, and configurations
- **Quick Start Steps**: Step-by-step instructions to get started immediately
- **How to Work With**: Detailed workflows for common development patterns
- **Common Tasks**: Practical examples with code snippets
- **Next Steps**: Recommended actions after initial setup
- **Additional Resources**: Links to documentation and guides

### 🎯 Template-Specific Content

Each template has its own tailored tutorial:

#### Single-Scene Prototype
- Focus on rapid prototyping
- Single scene architecture
- Local Addressables setup
- Basic state machine usage
- Simple UI and localization

#### Multi-Scene Modular
- Four-scene architecture (Bootstrap/Persistent/Shell/Gameplay)
- Remote content delivery
- Feature modules
- RemoteConfig integration
- A/B testing with feature flags

#### Clean Architecture
- Domain/Application/Infrastructure/Presentation layers
- UseCase pattern
- Repository abstraction
- EventBus system
- Unit testing with mocked repositories

## Usage

### Automatic Opening
The tutorial window automatically opens after successful template installation.

### Manual Access
Open the tutorial window anytime via:
```
Tools → Template Installer → Show Tutorial
```

### Navigation
- Scroll through the tutorial content
- Read step-by-step instructions
- Copy code examples
- Follow tips and best practices

## Tutorial Content Structure

### 1. Overview Section
Explains the template's purpose, use cases, and architecture philosophy.

### 2. What Was Created
Lists all generated:
- Folder structures
- Scenes
- Addressable groups
- Localization tables
- Core services

### 3. Quick Start Steps
Numbered steps to get started immediately:
- Opening scenes
- Creating first content
- Setting up Addressables
- Adding localization
- Testing the flow

Each step includes:
- **Title**: Clear action to take
- **Description**: Detailed explanation
- **Code Example**: Copy-paste ready code
- **Tip**: Best practices and gotchas

### 4. How to Work With
Workflow guides for common development patterns:
- Loading content via Addressables
- Adding new states
- Managing localization
- Organizing content
- Creating features

### 5. Common Tasks
Practical "How to" examples:
- How to: Load a Level
- How to: Show a UI Screen
- How to: Add a New Service
- How to: Create Feature Modules
- How to: Use RemoteConfig
- How to: Write Unit Tests

Each task includes:
- Step-by-step instructions
- Complete code examples
- Expected results

### 6. Next Steps
Recommended actions after completing the quick start:
- Creating more content
- Adding game-specific features
- Setting up remote content
- Building and deploying

### 7. Additional Resources
Links to:
- Architecture documentation
- Package documentation
- External resources
- Best practices guides

## Code Examples

All code examples are:
- ✅ Copy-paste ready
- ✅ Properly formatted
- ✅ Include necessary using statements
- ✅ Show complete context
- ✅ Follow best practices

## Tips and Best Practices

Throughout the tutorial, you'll find:
- 💡 **Tips**: Quick advice and shortcuts
- ⚠️ **Warnings**: Common pitfalls to avoid
- ✅ **Best Practices**: Recommended approaches
- 📝 **Notes**: Additional context

## Customization

### Adding New Tutorial Content

To add content to an existing tutorial:

1. Open `TemplateTutorialWindow.cs`
2. Find the appropriate `Create[Template]Tutorial()` method
3. Add new steps, guides, or tasks to the respective lists

Example - Adding a new Quick Start Step:
```csharp
QuickStartSteps = new List<TutorialStep>
{
    // ... existing steps
    new TutorialStep
    {
        Title = "Your New Step",
        Description = "Detailed description of what to do",
        CodeExample = "// Code example here\nvar example = new Example();",
        CodeHeight = 80,
        Tip = "Helpful tip for this step"
    }
}
```

### Creating Tutorial for New Template Type

1. Add new template type to `TemplateType` enum
2. Create new method: `CreateYourTemplateTutorial()`
3. Register in `InitializeTutorials()`:
```csharp
_tutorials[TemplateType.YourTemplate] = CreateYourTemplateTutorial();
```

## Integration with Installer

The tutorial system integrates seamlessly with the template installer:

1. User completes template installation
2. Installation success dialog appears
3. Tutorial window automatically opens
4. User can start working immediately with guidance

## Window Features

### Scrollable Content
All content is scrollable for easy navigation through long tutorials.

### Rich Text Formatting
- **Bold** for emphasis
- Code blocks with syntax highlighting
- Organized sections with headers
- Help boxes for important information

### Resizable Window
- Minimum size: 800x600
- Can be resized for comfortable reading
- Dockable in Unity Editor

## Best Practices for Tutorial Content

### Writing Steps
- ✅ Start with action verbs (Create, Open, Add, Configure)
- ✅ Be specific and actionable
- ✅ Include expected results
- ✅ Provide context for why

### Code Examples
- ✅ Show complete, working code
- ✅ Include necessary using statements
- ✅ Add comments for clarity
- ✅ Keep examples focused and concise

### Tips
- ✅ Highlight common mistakes
- ✅ Suggest shortcuts
- ✅ Explain non-obvious behavior
- ✅ Link to related concepts

## Maintenance

### Updating Content
When template structure changes:
1. Update "What Was Created" section
2. Revise Quick Start Steps if needed
3. Update code examples
4. Test all instructions

### Version Compatibility
- Keep tutorials in sync with template versions
- Note any Unity version-specific instructions
- Update package version requirements

## Troubleshooting

### Tutorial Window Doesn't Open
- Check console for errors
- Manually open: Tools → Template Installer → Show Tutorial
- Verify TemplateTutorialWindow.cs is in Editor folder

### Content Not Displaying
- Check template type is set correctly
- Verify tutorial data is initialized
- Check for compilation errors

### Code Examples Not Formatted
- Ensure GUIStyle initialization is working
- Check Unity Editor version compatibility

## Future Enhancements

Potential improvements:
- [ ] Interactive tutorials with validation
- [ ] Video tutorials embedded
- [ ] Search functionality
- [ ] Bookmarks for favorite sections
- [ ] Export tutorial as PDF
- [ ] Multi-language support for tutorials
- [ ] Progress tracking

## Contributing

When adding new tutorial content:
1. Follow existing structure
2. Test all code examples
3. Verify instructions are clear
4. Include tips for common issues
5. Update this documentation

## Support

For questions or issues:
- Check the architecture guides in `Assets/TempalateInstaller/`
- Review package documentation
- Open an issue in the repository

---

**Remember**: The tutorial system is designed to help developers get productive quickly. Keep content actionable, clear, and focused on practical tasks.
