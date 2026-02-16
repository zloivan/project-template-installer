# Unity Project Template Installer

Production-ready Unity project template installer that generates complete architecture setups with Addressables, Localization, and VContainer integration from day one.

## Features

✅ **Three Architecture Templates**
- **Single-Scene Prototype**: For hypercasual games and rapid prototyping (1-3 weeks)
- **Multi-Scene Modular**: For hybrid-casual games with LiveOps (3-12 months)
- **Clean Architecture**: For midcore games with complex business logic (1+ years)

✅ **Out-of-the-Box Integration**
- Addressables content management (local + remote ready)
- Unity Localization with 2+ languages
- VContainer dependency injection
- ContentService & LocalizationService (universal across templates)

✅ **Production-Ready Code**
- No code generation hacks
- Proper runtime/editor separation
- Extensible template system
- Migration-friendly (scale without rewriting)

## Installation

### Via Package Manager (Recommended)
1. Open Unity Package Manager
2. Add package from git URL: `git+https://github.com/yourname/project-template-installer.git`

### Via Manual Installation
1. Download the package
2. Place in `Packages/` folder of your Unity project

## Quick Start

### First Time Setup
1. After installation, the Template Installer window opens automatically
2. Or open via `Tools → Project Template Installer`
3. **After installation completes, a Tutorial Window will open automatically** with detailed step-by-step instructions
4. Access the tutorial anytime via `Tools → Template Installer → Show Tutorial`

### Choose Your Template
**Single-Scene Prototype** - Choose if:
- Building a hypercasual game
- Validating game mechanics quickly
- Team of 1-2 developers
- 1-3 week development cycle

**Modular** - Choose if:
- Building hybrid-casual (merge, idle, puzzle)
- Need A/B testing and remote config
- Team of 3-5 developers
- 3-12 month lifecycle

**Clean Architecture** - Choose if:
- Building midcore game (RPG, strategy)
- Complex business logic required
- Team of 5-10+ developers
- 1+ year lifecycle
- Backend dependency

### Configuration Options
- **Root Namespace**: Your project namespace (e.g., `MyGame`)
- **Include Addressables**: ✅ (Recommended - always enabled)
- **Include Localization**: ✅ (Recommended - always enabled)
- **Include RemoteConfig**: For Modular/Clean templates
- **Generate Sample Levels**: Creates example levels with proper structure
- **Remove Installer After Setup**: Cleans up the installer package post-installation

### What Gets Created

#### All Templates Include:
```
Assets/_Project/
├── Core/
│   ├── Content/
│   │   ├── ContentService.cs
│   │   └── ContentConfig.asset
│   └── Localization/
│       ├── LocalizationService.cs
│       └── LocalizedText.cs
├── Content/
│   ├── Configs/
│   ├── Levels/
│   └── UI/
└── Scenes/

Assets/AddressableAssets/Groups/
├── 00_Static/
├── 01_Localization/
├── 02_Levels_Local/
└── 04_UI/

Assets/Localization/
└── StringTables/
    ├── UI_EN
    └── UI_RU
```

#### Modular Template Adds:
- Bootstrap/Persistent/Shell/Gameplay scenes
- RemoteConfig integration
- Remote Addressables groups
- Feature-based installers

#### Clean Architecture Template Adds:
- Domain/Application/Infrastructure/Presentation layers
- IContentRepository abstraction
- UseCase pattern implementation
- EventBus system

## Architecture Comparison

| Feature | Single-Scene | Modular | Clean Architecture |
|---------|--------------|---------|-------------------|
| Scenes | 1 | 4 | 4 |
| DI Complexity | Simple | Medium | Complex |
| Addressables Groups | 4 Local | 4 Local + 2 Remote | 5 Local + 5 Remote |
| State Machine | 3 states | 5-7 states | 8+ states |
| Team Size | 1-2 | 3-5 | 5-10+ |
| Dev Time | 1-3 weeks | 3-12 months | 1+ years |

## Migration Path

The templates are designed for **zero-rewrite migration**:

```
Prototype → Modular → Clean Architecture → LiveOps
```

**What doesn't change during migration:**
- ContentService
- LocalizationService
- Addressables structure (only additions)
- UI components

**What gets added:**
- Week 4: Remote groups, additional scenes
- Month 3: Domain layer, UseCases
- Month 6: Backend integration, EventSystem

## Universal Services

### ContentService (VContainer-ready)
```csharp
public class ContentService
{
    public async Task<T> LoadAsync<T>(string key) where T : Object;
    public async Task<GameObject> InstantiateAsync(string key, Transform parent = null);
    public void Release<T>(T obj) where T : Object;
}

// Usage with VContainer injection:
public class MyGameplay : MonoBehaviour
{
    [Inject] private ContentService _contentService;

    private async void Start()
    {
        var prefab = await _contentService.LoadAsync<GameObject>("MyPrefab");
    }
}
```

### LocalizationService (VContainer-ready)
```csharp
public class LocalizationService
{
    public async Task InitializeAsync();
    public async Task SetLocaleAsync(string localeCode);
    public string GetString(string tableName, string key);
}

// Usage with VContainer injection:
public class MyUI : MonoBehaviour
{
    [Inject] private LocalizationService _localizationService;

    private void Start()
    {
        string text = _localizationService.GetString("UI", "play_button");
    }
}
```

## Best Practices

### ✅ DO
- Always use ContentService for asset loading
- Always use LocalizationService for UI text
- Mark assets as Addressable with meaningful keys
- Use AssetReference in inspector for type safety
- Set up at least 2 languages from day one

### ❌ DON'T
- Use Resources.Load (NEVER)
- Hardcode strings in UI
- Put everything in one Addressables group
- Skip localization setup
- Generate code dynamically

## Requirements

- Unity 2022.3 LTS or newer (including Unity 6)
- Addressables Package (latest - auto-installed)
- Localization Package (latest - auto-installed)
- VContainer 1.17.0+ (auto-installed via Git URL)
- UniTask (optional but recommended)

**Note:** Packages are installed automatically without version numbers to get the latest compatible version for your Unity.

## Documentation & Tutorials

### 📚 Built-in Tutorial System
After installing a template, an **interactive tutorial window** opens automatically with:
- **Overview**: What the template is designed for
- **What Was Created**: Complete list of generated files and configurations
- **Quick Start Steps**: Step-by-step instructions to get productive immediately
- **How to Work With**: Detailed workflows for common development patterns
- **Common Tasks**: Practical "How to" examples with code snippets
- **Next Steps**: Recommended actions after initial setup
- **Additional Resources**: Links to documentation and guides

Access the tutorial anytime: `Tools → Template Installer → Show Tutorial`

### 📖 Documentation Files
- **QUICK_REFERENCE.md**: Quick reference for all templates with code snippets
- **TUTORIAL_SYSTEM.md**: Complete guide to the tutorial system
- **unity-arch-single-scene.md**: Detailed Single-Scene architecture guide
- **unity-arch-modular.md**: Detailed Modular architecture guide
- **unity-arch-clean.md**: Detailed Clean Architecture guide
- **ARCHITECTURE.md**: General architecture overview

## Extensibility

You can create custom templates by:
1. Creating a new `TemplateDefinition` ScriptableObject
2. Defining folder structures, scenes, and configurations
3. Implementing custom `ITemplateModule` interfaces
4. Adding tutorial content in `TemplateTutorialWindow.cs`

See documentation for advanced customization.

## Troubleshooting

**"Package dependencies not installed"**
- Wait for Unity to resolve packages
- Check Package Manager for errors
- Manually install Addressables and Localization if needed

**"Template installation failed"**
- Check Unity console for detailed errors
- Ensure project is not in play mode
- Verify write permissions in Assets folder

**"Addressables groups not created"**
- Manually initialize Addressables: `Window → Asset Management → Addressables → Groups`
- Re-run template installer

## Support

- Documentation: [Link to full docs]
- Issues: [GitHub Issues]
- Discord: [Community server]

## License

MIT License - See LICENSE.md

## Credits

Based on production-proven architecture patterns used in:
- Hypercasual studios (Voodoo, Kwalee)
- Hybrid-casual studios (Playrix methodology)
- Midcore games (Clean Architecture patterns)

---

**Built for production. Scales without rewrites. Start right, stay right.**
