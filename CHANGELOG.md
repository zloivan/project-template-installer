# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2025-02-16

### Fixed
- Unity 6 (6000.x) version validation - now supports Unity 2022.3 LTS and newer
- Removed package dependencies from package.json to prevent signature warnings
- Package installation now uses latest compatible versions (no hardcoded versions)
- Compilation errors when Addressables/Localization not installed - added conditional compilation

### Added
- `PackageAutoInstaller` - automatically prompts to install required packages on first import
- `ScriptingDefineManager` - automatically adds/removes define symbols based on installed packages
- Conditional compilation for Addressables/Localization dependent code
- Stub implementations when packages are not installed

### Changed
- Improved version parsing with `IsUnityVersionSupported()` method
- Updated package description for Unity 6 compatibility
- Removed VContainer from required packages (already in manifest.json)
- `AddressablesConfigurator` now uses `ADDRESSABLES_INSTALLED` define symbol

---

## [1.0.0] - 2025-02-16

### Added
- Initial release of Unity Project Template Installer
- Three architecture templates:
  - Single-Scene Prototype for hypercasual games
  - Multi-Scene Modular for hybrid-casual games
  - Clean Architecture for midcore games
- Automatic Addressables configuration with local/remote groups
- Unity Localization integration with multiple languages
- ContentService universal content loading system
- LocalizationService universal localization system
- Template Installer Window with interactive UI
- Auto-open installer on first project load
- Package self-removal feature
- Folder structure generation
- Scene creation and build settings configuration
- Runtime code generation with namespace injection
- Progress reporting during installation
- Environment validation
- Error handling and rollback capabilities

### Features
- **Production-Ready Templates**: No prototypes to rewrite
- **Addressables from Day 1**: Remote content ready
- **Localization Built-in**: 2+ languages out of the box
- **Migration-Friendly**: Scale without rewrites
- **VContainer Integration**: Dependency injection support
- **State Machine**: Implemented for each template
- **Editor Tools**: Complete editor workflow integration

### Dependencies
- Unity 2022.3 LTS or newer
- Addressables 1.21.0+
- Localization 1.4.0+
- VContainer (optional but recommended)
- UniTask (optional but recommended)

### Documentation
- Comprehensive README with usage instructions
- Architecture comparison guide
- Migration path documentation
- Best practices and troubleshooting

## [Unreleased]

### Planned
- VContainer template integration (requires package)
- UniTask async/await templates (requires package)
- Additional template variants (LiveOps, Shell+Streaming)
- Sample content generation
- Custom template creation wizard
- Template preview system
- Dry-run mode
- CLI support for CI/CD pipelines

---

## Template Versions

### Single-Scene Prototype v1.0.0
- Basic folder structure
- One scene architecture
- Simple state machine (3 states)
- Local Addressables only
- EN + RU localization

### Multi-Scene Modular v1.0.0
- Four-scene architecture
- Feature-based modules
- Local + Remote Addressables
- RemoteConfig scaffolding
- EN + RU + ZH localization

### Clean Architecture v1.0.0
- Domain/Application/Infrastructure/Presentation layers
- IContentRepository abstraction
- UseCase pattern
- EventBus system
- EN + RU + ZH + JA + KO localization

---

## Breaking Changes

None in initial release.

## Known Issues

- VContainer installer templates require manual VContainer package installation
- UniTask templates are not yet implemented
- Sample content generation is placeholder only

## Migration Guide

N/A - Initial release

---

For full documentation, see [README.md](README.md)
