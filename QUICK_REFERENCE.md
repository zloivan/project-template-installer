# Template Quick Reference Guide

This guide provides quick access to essential information for working with each template.

---

## 🎮 Single-Scene Prototype

### When to Use
- Hypercasual games
- Rapid prototyping (1-3 weeks)
- Team: 1-2 developers
- Simple game mechanics

### Key Locations
```
_Project/
├── Bootstrap/          # BootstrapInstaller
├── Core/
│   ├── Content/       # ContentService, ContentConfig
│   ├── Localization/  # LocalizationService
│   └── StateMachine/  # Game states
├── Game/
│   ├── Levels/        # Level prefabs (Addressable)
│   └── UI/            # UI screens (Addressable)
└── Scenes/            # Game.unity
```

### Essential Services
- **ContentService**: Load all assets via Addressables
- **LocalizationService**: Manage translations
- **GameStateMachine**: Control game flow
- **UIManager**: Load and manage UI screens
- **LevelLoader**: Load level prefabs

### Quick Actions

#### Create a Level
1. Create prefab in `_Project/Game/Levels/Prefabs/`
2. Name it `Level_1`, `Level_2`, etc.
3. Mark as Addressable → Address: `Level_1` → Group: `02_Levels_Local`

#### Create UI Screen
1. Create Canvas prefab in `_Project/Game/UI/Screens/`
2. Name it descriptively (e.g., `MainMenuScreen`)
3. Mark as Addressable → Address: `UI_MainMenu` → Group: `04_UI`

#### Add Localized Text
1. Add `LocalizedText` component to TextMeshProUGUI
2. Set Table: `UI`, Key: `your_key`
3. Add key to Localization Tables (Window → Asset Management → Localization Tables)

### Code Snippets

**Load Level:**
```csharp
var level = await _levelLoader.LoadCurrentLevelAsync();
```

**Show UI Screen:**
```csharp
var screen = await _uiManager.ShowScreenAsync<MainMenuScreen>("UI_MainMenu");
```

**Get Localized String:**
```csharp
string text = _localization.GetString("UI", "play_button");
```

### Addressable Groups
- `00_Static` - Core configs (Local)
- `01_Localization` - String/Asset tables (Local)
- `02_Levels_Local` - Levels 1-10 (Local)
- `04_UI` - All UI screens (Local)

---

## 🏗️ Multi-Scene Modular

### When to Use
- Hybrid-casual games
- LiveOps integration (3-12 months)
- Team: 3-5 developers
- A/B testing and remote balance

### Key Locations
```
_Project/
├── 00_Bootstrap/       # ProjectInstaller, Bootstrap.unity
├── 01_Core/           # Core services, Persistent.unity
├── 02_Features/       # Feature modules (Shop, Progression)
├── 03_Content/        # Configs and prefabs
├── 04_SDK/            # Analytics, Ads, IAP
└── 05_LiveOps/        # RemoteConfig, FeatureFlags
```

### Scene Flow
```
Bootstrap.unity (loads once, then unloads)
    ↓
Persistent.unity (loads additively, never unloads)
    ↓
Shell.unity (meta UI - main menu, shop)
    ↓
Gameplay.unity (loads when playing, unloads when done)
    ↓
Back to Shell.unity
```

### Essential Services
- **ContentService**: Load local + remote content
- **LocalizationService**: Multi-language support
- **RemoteConfigService**: Remote configuration
- **FeatureFlagService**: Enable/disable features
- **SceneLoader**: Manage scene loading
- **AppStateMachine**: Control app flow

### Quick Actions

#### Create Feature Module
1. Create folder: `_Project/02_Features/YourFeature/`
2. Add `YourFeature.cs` implementing `IFeature`
3. Add `YourFeatureInstaller.cs` for DI
4. Register in `PersistentInstaller`

#### Setup Remote Level
1. Create level prefab
2. Mark as Addressable → Address: `Level_11` → Group: `03_Levels_Remote`
3. Configure group: Build Path: `ServerData/[BuildTarget]`
4. Set Load Path: `https://your-cdn.com/[BuildTarget]`

#### Use Feature Flag
```csharp
bool shopEnabled = _featureFlags.IsEnabled("shop");
menu.SetShopButtonVisible(shopEnabled);
```

### Code Snippets

**Load Remote Level with Download:**
```csharp
var size = await _content.GetDownloadSizeAsync("Level_11");
if (size > 0)
{
    await _content.DownloadAsync("Level_11", progress);
}
var level = await _content.InstantiateAsync("Level_11");
```

**Load Scene:**
```csharp
await _sceneLoader.LoadAdditiveAsync("Gameplay");
await _sceneLoader.UnloadAsync("Shell");
```

**Get RemoteConfig Value:**
```csharp
int timer = _remoteConfig.GetValue<int>("level_timer", 60);
```

### Addressable Groups
- `00_Static` - Core configs (Local)
- `01_Localization` - Translations (Local)
- `02_Levels_Local` - Levels 1-10 (Local)
- `03_Levels_Remote` - Levels 11+ (Remote) ⭐
- `04_UI` - UI screens (Local)
- `05_Audio` - Music/SFX (Mixed)

### RemoteConfig Keys
```json
{
  "shop_items": "[...]",
  "level_timer": 60,
  "ads_frequency": 3,
  "feature_shop": true,
  "feature_daily_reward": true
}
```

---

## 🏛️ Clean Architecture

### When to Use
- Midcore games (RPG, Strategy)
- Complex business logic
- Team: 5-10+ developers
- Long-term project (1+ years)
- High testability requirements

### Key Locations
```
_Project/
├── 00_Bootstrap/           # ProjectInstaller
├── 01_Core/
│   ├── Domain/            # Entities, UseCases, Interfaces
│   ├── Application/       # Services, Events
│   ├── Infrastructure/    # Repositories, Network, Persistence
│   └── Presentation/      # ViewModels, Views
├── 02_Features/           # Feature modules (Battle, Inventory, Social)
├── 03_Shared/             # Shared utilities
└── 04_Content/            # Configs and prefabs
```

### Architecture Layers

**Domain (Inner Layer)**
- Entities: `Player`, `Item`, `Currency`
- UseCases: `PurchaseItemUseCase`, `EquipItemUseCase`
- Interfaces: `IContentRepository`, `IInventoryRepository`

**Application Layer**
- Services: `ContentService`, `LocalizationService`
- Events: `EventBus`, domain events

**Infrastructure Layer**
- Repositories: `ContentRepository` (wraps Addressables)
- Network: `BackendClient`
- Persistence: `SaveSystem`

**Presentation Layer**
- ViewModels: Presentation logic
- Views: MonoBehaviour UI

### Essential Patterns

#### Repository Pattern
```csharp
// Domain Interface
public interface IContentRepository
{
    Task<T> LoadAssetAsync<T>(string key);
    Task<GameObject> InstantiateAsync(string key);
}

// Infrastructure Implementation
public class ContentRepository : IContentRepository
{
    private readonly ContentService _contentService;

    public async Task<T> LoadAssetAsync<T>(string key)
    {
        return await _contentService.LoadAsync<T>(key);
    }
}
```

#### UseCase Pattern
```csharp
public class PurchaseItemUseCase
{
    private readonly IInventoryRepository _inventory;
    private readonly IWalletRepository _wallet;
    private readonly IContentRepository _content;

    public async Task<Result<Item>> ExecuteAsync(string itemId)
    {
        // 1. Load item definition
        var itemDef = await _inventory.GetItemDefinitionAsync(itemId);

        // 2. Check funds
        if (!_wallet.CanAfford(itemDef.Price))
            return Result<Item>.Failure("insufficient_funds");

        // 3. Process purchase
        _wallet.Spend(itemDef.Price);
        var item = itemDef.CreateInstance();
        await _inventory.AddItemAsync(item);

        return Result<Item>.Success(item);
    }
}
```

#### EventBus Pattern
```csharp
// Publish
_eventBus.Publish(new ItemPurchasedEvent(item));

// Subscribe
_eventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased);

// Unsubscribe
_eventBus.Unsubscribe<ItemPurchasedEvent>(OnItemPurchased);
```

### Quick Actions

#### Create UseCase
1. Create in `_Project/01_Core/Domain/UseCases/`
2. Inject repository interfaces
3. Implement business logic
4. Register in `ProjectInstaller`: `builder.Register<YourUseCase>(Lifetime.Transient)`

#### Create Repository
1. Define interface in `Domain/Interfaces/`
2. Implement in `Infrastructure/Persistence/`
3. Use `IContentRepository` for Addressables
4. Register: `builder.Register<IYourRepository, YourRepository>(Lifetime.Singleton)`

#### Create ViewModel
1. Create in `Presentation/ViewModels/`
2. Inject UseCases and services
3. Handle UI logic and events
4. Register: `builder.Register<YourViewModel>(Lifetime.Transient)`

### Code Snippets

**Load Item Definition:**
```csharp
public async Task<ItemDefinition> GetItemDefinitionAsync(string itemId)
{
    string key = $"ItemDef_{itemId}";
    return await _content.LoadAssetAsync<ItemDefinition>(key);
}
```

**ViewModel with UseCase:**
```csharp
public class ShopViewModel
{
    private readonly PurchaseItemUseCase _purchaseUseCase;
    private readonly LocalizationService _localization;

    public async void OnItemClicked(string itemId)
    {
        var result = await _purchaseUseCase.ExecuteAsync(itemId);

        if (result.IsSuccess)
        {
            ShowSuccessPopup(result.Value);
        }
        else
        {
            string error = _localization.GetString("Errors", result.Error);
            ShowErrorPopup(error);
        }
    }
}
```

**Unit Test with Mocks:**
```csharp
[Test]
public async Task PurchaseItem_WithSufficientFunds_ShouldSucceed()
{
    // Arrange
    var contentMock = new Mock<IContentRepository>();
    var inventoryMock = new Mock<IInventoryRepository>();
    var walletMock = new Mock<IWalletRepository>();

    walletMock.Setup(w => w.CanAfford(100)).Returns(true);

    var useCase = new PurchaseItemUseCase(
        inventoryMock.Object,
        walletMock.Object,
        contentMock.Object
    );

    // Act
    var result = await useCase.ExecuteAsync("sword");

    // Assert
    Assert.IsTrue(result.IsSuccess);
}
```

### Addressable Groups
- `00_Static` - Item definitions (Local)
- `01_Localization` - Translations (Local + Remote)
- `02_Content_Local` - Common items (Local)
- `03_Content_Remote` - Premium items (Remote) ⭐
- `04_UI` - UI screens (Local)
- `05_Features` - Feature bundles (Remote) ⭐

### Testing Strategy
- **Unit Tests**: Test UseCases with mocked repositories
- **Integration Tests**: Test repositories with real Addressables
- **No Unity Dependencies**: Domain layer is pure C#

---

## 📋 Common Workflows

### Adding Localization

1. **Open Localization Tables:**
   ```
   Window → Asset Management → Localization Tables
   ```

2. **Add Entry:**
   - Select table (UI, Shop, Gameplay)
   - Add key and translations for all languages

3. **Use in Code:**
   ```csharp
   string text = _localization.GetString("TableName", "key");
   ```

4. **Use in UI:**
   - Add `LocalizedText` component to TextMeshProUGUI
   - Set Table and Key

### Building Addressables

1. **Build Content:**
   ```
   Window → Asset Management → Addressables → Build → Build Player Content
   ```

2. **Upload Remote Content:**
   - Find `ServerData` folder in project root
   - Upload to CDN
   - Update Load Path in Addressables Groups

3. **Test:**
   - Clear cache: `Addressables → Clear → All`
   - Play and verify downloads

### Dependency Injection

**Register Service:**
```csharp
builder.Register<YourService>(Lifetime.Singleton);
```

**Register with Interface:**
```csharp
builder.Register<IYourService, YourService>(Lifetime.Singleton);
```

**Inject into Class:**
```csharp
public class YourClass
{
    private readonly YourService _service;

    public YourClass(YourService service)
    {
        _service = service;
    }
}
```

---

## 🔧 Troubleshooting

### Addressables Not Loading
- Check asset is marked as Addressable
- Verify Address key matches code
- Check group is built
- Clear cache and rebuild

### Localization Not Working
- Verify table exists and has entries
- Check locale is set correctly
- Ensure LocalizationService is initialized
- Check key spelling

### Scene Not Loading
- Verify scene is in Build Settings
- Check scene name matches code
- Use SceneLoader service, not SceneManager
- Check for errors in Console

### DI Injection Failing
- Verify service is registered in installer
- Check lifetime scope is correct
- Ensure installer is attached to scene
- Check for circular dependencies

---

## 📚 Additional Resources

### Documentation
- **VContainer**: https://vcontainer.hadashikick.jp/
- **Addressables**: https://docs.unity3d.com/Packages/com.unity.addressables@latest
- **Localization**: https://docs.unity3d.com/Packages/com.unity.localization@latest
- **UniTask**: https://github.com/Cysharp/UniTask

### Architecture Guides
- `unity-arch-single-scene.md` - Single-Scene details
- `unity-arch-modular.md` - Modular architecture
- `unity-arch-clean.md` - Clean Architecture
- `ARCHITECTURE.md` - General architecture overview

### Menu Locations
- **Template Installer**: `Tools → Project Template Installer`
- **Tutorial Window**: `Tools → Template Installer → Show Tutorial`
- **Addressables**: `Window → Asset Management → Addressables`
- **Localization**: `Window → Asset Management → Localization Tables`

---

**💡 Pro Tip**: Keep this reference open while developing. It contains the most common patterns and solutions you'll need daily.
