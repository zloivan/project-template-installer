# Template Installer Architecture

## Design Philosophy

The Template Installer is built on these core principles:

1. **No Code Generation Hacks**: Use pre-built templates and configuration, not string-based code generation
2. **Idempotent Operations**: Can be re-run safely without breaking existing setup
3. **Extensibility**: Easy to add new templates via ScriptableObjects
4. **Safety**: Validates environment, confirms destructive operations, supports rollback
5. **Editor-Only**: All installation code properly guarded and separated from runtime

## System Architecture

```
TemplateInstaller/
├── Runtime/                    (Data structures only, no logic)
│   ├── InstallCore/           (Core data models)
│   │   ├── TemplateDefinition.cs
│   │   ├── TemplateType.cs
│   │   ├── FolderStructure.cs
│   │   ├── AddressableGroupDefinition.cs
│   │   ├── LocalizationTableDefinition.cs
│   │   ├── SceneDefinition.cs
│   │   └── InstallationProgress.cs
│   └── InstallData/
│       └── Templates/         (Template definition assets)
│
└── Editor/                    (All installation logic)
    ├── TemplateInstaller.cs          (Main orchestrator)
    ├── TemplateInstallerWindow.cs    (UI)
    ├── InstallTrigger.cs             (Auto-open)
    ├── AddressablesConfigurator.cs   (Addressables setup)
    ├── LocalizationConfigurator.cs   (Localization setup)
    ├── SceneGenerator.cs             (Scene creation)
    ├── RuntimeCodeGenerator.cs       (Code generation)
    ├── CodeTemplates.cs              (Code templates)
    ├── BuildSettingsConfigurator.cs  (Build settings)
    ├── TemplateDefinitionFactory.cs  (Template creation)
    ├── PackageSelfRemover.cs         (Self-removal)
    └── EditorUtilities/
        ├── PackageValidator.cs
        └── PackageInstallationHelper.cs
```

## Component Responsibilities

### Runtime Layer (Data Only)

**TemplateDefinition (ScriptableObject)**
- Defines complete template configuration
- Folder structures
- Scene definitions
- Addressables groups
- Localization tables
- Required packages
- Code generation flags

**Data Structures**
- `FolderStructure`: Hierarchical folder definitions
- `AddressableGroupDefinition`: Addressables group configuration
- `LocalizationTableDefinition`: Localization table specs
- `SceneDefinition`: Scene creation parameters
- `InstallationProgress`: Progress tracking

### Editor Layer (Logic)

**TemplateInstaller (Orchestrator)**
- Main installation engine
- Validates environment
- Coordinates all configurators
- Reports progress
- Handles errors and rollback

**TemplateInstallerWindow (UI)**
- User interface for template selection
- Configuration options
- Preview of what will be created
- Installation trigger

**Module Configurators**

*AddressablesConfigurator*
- Creates Addressables groups
- Configures build/load paths
- Sets compression settings
- Manages labels

*LocalizationConfigurator*
- Creates string/asset table collections
- Adds locales
- Makes tables addressable
- Configures table settings

*SceneGenerator*
- Creates Unity scenes
- Adds template-specific GameObjects
- Configures scene hierarchy
- Adds placeholder components

*RuntimeCodeGenerator*
- Generates C# code from templates
- Injects namespace
- Creates folder structure
- Writes code files

**CodeTemplates (Static)**
- Contains all code generation templates
- Uses placeholder for namespace injection
- Templates for ContentService, LocalizationService, etc.
- State machine templates
- Bootstrap installer templates

**BuildSettingsConfigurator**
- Adds scenes to build settings
- Configures player settings
- Sets build order

**PackageSelfRemover**
- Safely removes installer package
- Confirms with user
- Preserves generated project

## Installation Flow

```
1. User Opens Installer Window
   ↓
2. Select Template Type
   ├→ Single-Scene
   ├→ Modular
   └→ Clean Architecture
   ↓
3. Configure Options
   ├→ Root Namespace
   ├→ Addressables (always on)
   ├→ Localization (always on)
   ├→ RemoteConfig (Modular/Clean only)
   ├→ Generate Samples
   └→ Remove Installer After
   ↓
4. Preview What Will Be Created
   ↓
5. Click "Install Template"
   ↓
6. TemplateInstaller.Install()
   ├→ Validate Environment
   │   ├→ Check Unity version
   │   ├→ Check play mode
   │   └→ Check write permissions
   ├→ Detect Existing Installation
   ├→ Create Folder Structure
   │   └→ CreateFolderHierarchy() for each structure
   ├→ Install Dependencies
   │   └→ PackageInstallationHelper for each package
   ├→ Configure Addressables (if enabled)
   │   ├→ Initialize Addressables
   │   └→ CreateGroup() for each group definition
   ├→ Configure Localization (if enabled)
   │   ├→ Create locales
   │   └→ CreateTable() for each table definition
   ├→ Create Scenes
   │   ├→ Generate scene files
   │   ├→ Add template-specific objects
   │   └→ Save scenes
   ├→ Generate Runtime Code
   │   ├→ ContentService
   │   ├→ LocalizationService
   │   ├→ StateMachine
   │   ├→ Bootstrap Installer
   │   └→ States (based on template)
   ├→ Configure Build Settings
   │   └→ Add scenes to build
   └→ Complete Installation
       ├→ AssetDatabase.Refresh()
       └→ Optional: RemoveInstallerPackage()
   ↓
7. Installation Result
   ├→ Success: Show completion dialog
   └→ Failure: Show error with details
```

## Code Generation Strategy

**Template-Based, Not String Building**

Instead of dynamically generating code with string concatenation:
```csharp
// ❌ DON'T DO THIS
string code = "public class " + className + " { ... }";
```

We use pre-written templates with placeholder injection:
```csharp
// ✅ DO THIS
string template = CodeTemplates.GetContentServiceTemplate(namespace);
// Template contains complete, validated code with {{NAMESPACE}} placeholder
```

**Benefits:**
- Templates are real C# that can be validated
- No syntax errors from string concatenation
- Easy to maintain and extend
- Safe namespace injection only

## Extensibility Points

### Adding a New Template

1. Create a new `TemplateDefinition` asset
2. Configure folder structures, scenes, groups, tables
3. Add to `TemplateDefinitionFactory` if needed
4. Update `TemplateInstallerWindow` UI
5. Add template-specific code templates to `CodeTemplates`
6. Update `RuntimeCodeGenerator` to handle new template

### Adding a New Configurator

1. Implement configurator class (e.g., `RemoteConfigConfigurator`)
2. Add configuration data structure to Runtime
3. Call from `TemplateInstaller.Install()`
4. Add progress reporting
5. Handle idempotent operations

### Adding a New Code Template

1. Add method to `CodeTemplates.cs`
2. Use `{{NAMESPACE}}` placeholder for injection
3. Call from `RuntimeCodeGenerator`
4. Test template output

## Safety Mechanisms

### Environment Validation
- Unity version check (2022.3+)
- Play mode check (cannot install in play mode)
- Write permissions check
- Package dependency check

### Idempotent Operations
- Check if folders exist before creating
- Check if groups exist before adding
- Check if tables exist before creating
- Check if scenes exist before generating
- Check if files exist before writing

### Error Handling
- Try-catch at top level
- Detailed error messages
- Error context in InstallationResult
- Rollback on critical failures

### User Confirmations
- Confirm installation
- Confirm overwrite if existing installation detected
- Confirm package removal
- Preview before installation

## Performance Considerations

### Batch Operations
- Create all folders in one pass
- Create all Addressables groups in one pass
- Generate all code files in one pass
- Single AssetDatabase.Refresh() at end

### Progress Reporting
- Report progress at major steps
- Update progress bar during long operations
- Allow UI to remain responsive

### Asset Database
- Minimize AssetDatabase operations
- Batch creates before refresh
- Use AssetDatabase.StartAssetEditing/StopAssetEditing for bulk operations

## Testing Strategy

### Manual Testing
- Test each template type
- Test with existing project
- Test with clean project
- Test all option combinations
- Test package removal

### Validation Testing
- Invalid Unity version
- Play mode detection
- Missing dependencies
- Write permission issues

### Edge Cases
- Re-running installation
- Partial installation failure
- Package already installed
- Corrupted template definition

## Future Enhancements

### Planned Features
1. VContainer integration (actual LifetimeScope generation)
2. UniTask integration (async/await templates)
3. Sample content generation (prefabs, ScriptableObjects)
4. Template preview system (visual preview)
5. Dry-run mode (preview without changes)
6. CLI support (for CI/CD)
7. Custom template wizard (create templates via UI)
8. Template versioning (upgrade path)

### Possible Extensions
- Remote template repository
- Template marketplace
- Template inheritance
- Template composition (mix features)
- Project migration tool (upgrade existing projects)

## Dependencies

### Required
- Unity 2022.3 LTS+
- Addressables 1.21.0+
- Localization 1.4.0+

### Optional
- VContainer (for DI templates)
- UniTask (for async/await templates)
- TextMeshPro (for LocalizedText component)

## Build and Distribution

### UPM Package Structure
```
package.json        (Package manifest)
README.md          (User documentation)
CHANGELOG.md       (Version history)
LICENSE.md         (MIT License)
ARCHITECTURE.md    (This file)
Runtime/           (Data structures)
Editor/            (Installation logic)
```

### Installation Methods
1. Via Package Manager (git URL)
2. Via local package (development)
3. Via package tarball

### Version Management
- Semantic versioning (MAJOR.MINOR.PATCH)
- Breaking changes = MAJOR bump
- New features = MINOR bump
- Bug fixes = PATCH bump

## Conclusion

The Template Installer is designed as a production-grade tooling system with:
- Clean separation of concerns
- Safety-first approach
- Extensibility at core
- No magic or hacks
- Comprehensive error handling

It generates production-ready projects that scale from prototype to production without rewrites, following Unity best practices and established architecture patterns.
