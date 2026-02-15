# VContainer Integration Guide

## ✅ VContainer is Now Fully Integrated!

All templates now come with VContainer dependency injection out of the box. No manual setup required!

---

## What's Included

### 1. Automatic Package Installation
When you install a template, VContainer (1.15.4) is automatically installed via Unity Package Manager.

### 2. Pre-configured Services
All core services are registered and ready to use:
- ✅ `ContentService` - Addressables asset loading
- ✅ `LocalizationService` - Unity Localization integration
- ✅ `GameStateMachine` / `AppStateMachine` - State management

### 3. Generated VContainer Code

#### BootstrapInstaller (LifetimeScope)
Every template generates a `LifetimeScope` that registers all services:

```csharp
public class BootstrapInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register core services as Singletons
        builder.Register<Core.Content.ContentService>(Lifetime.Singleton);
        builder.Register<Core.Localization.LocalizationService>(Lifetime.Singleton);
        builder.Register<Core.StateMachine.GameStateMachine>(Lifetime.Singleton);
    }
}
```

**Setup in Unity:**
1. Create an empty GameObject in your Bootstrap/Game scene
2. Name it "BootstrapInstaller" or "ProjectInstaller"
3. Add the generated installer component to it
4. This becomes your root DI container

---

## How to Use VContainer in Your Code

### Basic Injection Pattern

```csharp
using VContainer;
using UnityEngine;

public class MyGameplay : MonoBehaviour
{
    // Inject services via [Inject] attribute
    [Inject] private ContentService _contentService;
    [Inject] private LocalizationService _localizationService;

    private async void Start()
    {
        // Services are automatically injected and ready to use
        var prefab = await _contentService.LoadAsync<GameObject>("MyPrefab");
        string text = _localizationService.GetString("UI", "play_button");
    }
}
```

### Constructor Injection (Recommended for non-MonoBehaviours)

```csharp
public class GameController
{
    private readonly ContentService _contentService;
    private readonly LocalizationService _localizationService;

    // VContainer will automatically inject dependencies
    public GameController(
        ContentService contentService,
        LocalizationService localizationService)
    {
        _contentService = contentService;
        _localizationService = localizationService;
    }

    public async Task LoadLevel(string levelKey)
    {
        var level = await _contentService.LoadAsync<GameObject>(levelKey);
    }
}
```

---

## Template-Specific Setups

### Single-Scene Template

**Generated Files:**
- `BootstrapInstaller.cs` - Root LifetimeScope
- `GameEntryPoint.cs` - Entry point with injected services
- `GameStateMachine.cs` - State machine

**Scene Setup:**
```
Game Scene
└── BootstrapInstaller (GameObject)
    ├── BootstrapInstaller (Component)
    └── GameEntryPoint (Component)
```

### Modular Template

**Generated Files:**
- `ProjectInstaller.cs` - Root LifetimeScope
- `AppEntryPoint.cs` - Entry point with injected services
- `AppStateMachine.cs` - State machine

**Scene Setup:**
```
Bootstrap Scene
└── ProjectInstaller (GameObject)
    ├── ProjectInstaller (Component)
    └── AppEntryPoint (Component)
```

### Clean Architecture Template

Same as Modular, but with additional domain services you can register:

```csharp
protected override void Configure(IContainerBuilder builder)
{
    // Core services
    builder.Register<Core.Content.ContentService>(Lifetime.Singleton);
    builder.Register<Core.Localization.LocalizationService>(Lifetime.Singleton);

    // Domain layer
    builder.Register<Domain.IContentRepository, Infrastructure.ContentRepository>(Lifetime.Singleton);
    builder.Register<Application.Services.EventBus>(Lifetime.Singleton);

    // Use cases
    builder.Register<Domain.UseCases.LoadLevelUseCase>(Lifetime.Transient);
}
```

---

## Common Patterns

### 1. Registering Your Own Services

Edit the generated installer:

```csharp
public class BootstrapInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Auto-generated services
        builder.Register<Core.Content.ContentService>(Lifetime.Singleton);
        builder.Register<Core.Localization.LocalizationService>(Lifetime.Singleton);

        // YOUR CUSTOM SERVICES
        builder.Register<MyGameController>(Lifetime.Singleton);
        builder.Register<MyAudioManager>(Lifetime.Singleton);
        builder.Register<MyPlayerData>(Lifetime.Singleton);
    }
}
```

### 2. Registering MonoBehaviours

For MonoBehaviours that need injection:

```csharp
public class BootstrapInstaller : LifetimeScope
{
    [SerializeField] private MyUIManager _uiManagerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        // Register services
        builder.Register<ContentService>(Lifetime.Singleton);

        // Register MonoBehaviour from prefab
        builder.RegisterComponentInNewPrefab(_uiManagerPrefab, Lifetime.Singleton);
    }
}
```

### 3. Interface-Based Registration

```csharp
// Register interface with implementation
builder.Register<IContentService, ContentService>(Lifetime.Singleton);
builder.Register<ILocalizationService, LocalizationService>(Lifetime.Singleton);

// Usage
public class MyClass
{
    [Inject] private IContentService _contentService; // Injects ContentService
}
```

### 4. Factory Pattern

```csharp
public class BootstrapInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ContentService>(Lifetime.Singleton);

        // Register factory
        builder.RegisterFactory<string, Enemy>(container =>
        {
            var contentService = container.Resolve<ContentService>();
            return async (enemyKey) =>
            {
                var prefab = await contentService.LoadAsync<GameObject>(enemyKey);
                return prefab.GetComponent<Enemy>();
            };
        });
    }
}
```

---

## Scene Injection

VContainer automatically injects into MonoBehaviours in the scene:

```csharp
// This MonoBehaviour is in the scene
public class LevelController : MonoBehaviour
{
    [Inject] private ContentService _contentService;
    [Inject] private GameStateMachine _stateMachine;

    private void Start()
    {
        // Services are already injected!
        Debug.Log("Services ready!");
    }
}
```

**Important:** The GameObject with the LifetimeScope must be active before other scripts try to use injection.

---

## Lifetime Scopes

### Parent-Child Scopes

For multi-scene setups (Modular/Clean templates):

```csharp
// Bootstrap Scene - Root Scope
public class ProjectInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ContentService>(Lifetime.Singleton);
        builder.Register<LocalizationService>(Lifetime.Singleton);
    }
}

// Gameplay Scene - Child Scope
public class GameplayInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Can access parent services + register gameplay-specific services
        builder.Register<LevelController>(Lifetime.Scoped);
        builder.Register<EnemySpawner>(Lifetime.Scoped);
    }
}
```

---

## Best Practices

### ✅ DO

1. **Use Constructor Injection for non-MonoBehaviours**
   ```csharp
   public class GameController
   {
       public GameController(ContentService contentService) { }
   }
   ```

2. **Use [Inject] for MonoBehaviours**
   ```csharp
   public class UIController : MonoBehaviour
   {
       [Inject] private LocalizationService _localizationService;
   }
   ```

3. **Register services as Singleton for shared state**
   ```csharp
   builder.Register<PlayerData>(Lifetime.Singleton);
   ```

4. **Use interfaces for testability**
   ```csharp
   builder.Register<IContentService, ContentService>(Lifetime.Singleton);
   ```

### ❌ DON'T

1. **Don't use FindObjectOfType**
   ```csharp
   // BAD
   var service = FindObjectOfType<ContentService>();

   // GOOD
   [Inject] private ContentService _contentService;
   ```

2. **Don't create services manually**
   ```csharp
   // BAD
   var service = new ContentService();

   // GOOD - Let VContainer handle it
   [Inject] private ContentService _contentService;
   ```

3. **Don't register MonoBehaviours as Singleton if they're in scenes**
   ```csharp
   // BAD - Will create duplicate
   builder.Register<MySceneController>(Lifetime.Singleton);

   // GOOD - Let VContainer inject into existing scene object
   // Just add [Inject] to the MonoBehaviour in the scene
   ```

---

## Troubleshooting

### "VContainer: Type not registered"

**Problem:** Service not registered in LifetimeScope

**Solution:**
```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.Register<YourService>(Lifetime.Singleton); // Add this
}
```

### "NullReferenceException on injected field"

**Problem:** Injection happening after Awake/Start

**Solution:** Use injection in Start() or later, not in Awake():
```csharp
[Inject] private ContentService _contentService;

private void Start() // Not Awake()
{
    _contentService.LoadAsync(...); // Safe here
}
```

### "Multiple LifetimeScopes in scene"

**Problem:** More than one root scope

**Solution:** Only one LifetimeScope per scene (unless using parent-child pattern)

---

## Migration from Manual DI

If you have existing code without VContainer:

### Before (Manual):
```csharp
public class GameController : MonoBehaviour
{
    private ContentService _contentService;

    private void Awake()
    {
        _contentService = FindObjectOfType<ContentService>();
    }
}
```

### After (VContainer):
```csharp
public class GameController : MonoBehaviour
{
    [Inject] private ContentService _contentService;

    // No Awake needed - VContainer handles it!
}
```

---

## Advanced: Custom Scopes

For complex games with multiple scenes:

```csharp
// Global scope (DontDestroyOnLoad)
public class GlobalInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<PlayerData>(Lifetime.Singleton);
        builder.Register<SaveSystem>(Lifetime.Singleton);
    }
}

// Level scope (per-level services)
public class LevelInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<LevelController>(Lifetime.Scoped);
        builder.Register<EnemyManager>(Lifetime.Scoped);
    }
}
```

---

## Resources

- **VContainer Documentation:** https://vcontainer.hadashikick.jp/
- **Template Installer Docs:** See README.md
- **Unity Localization:** https://docs.unity3d.com/Packages/com.unity.localization@latest
- **Addressables:** https://docs.unity3d.com/Packages/com.unity.addressables@latest

---

**VContainer is production-ready and battle-tested. All templates are configured to use it from day one! 🚀**
