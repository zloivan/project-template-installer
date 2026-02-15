# Multi-Scene Modular (Hybrid-Casual)

## Применение
- Жизненный цикл 3-12 месяцев
- A/B тестирование фич
- Remote Config для баланса
- Множество игровых режимов
- **Addressables + Localization с первого дня**
- **Готовность к LiveOps**

---

## Структура папок

```
Assets/
├── _Project/
│   ├── 00_Bootstrap/
│   │   ├── Scenes/Bootstrap.unity
│   │   └── BootstrapInstaller.cs
│   ├── 01_Core/
│   │   ├── Content/
│   │   │   ├── ContentService.cs
│   │   │   └── ContentConfig.asset
│   │   ├── Localization/
│   │   │   ├── LocalizationService.cs
│   │   │   └── LocalizedText.cs
│   │   ├── Infrastructure/
│   │   │   ├── StateMachine/
│   │   │   ├── Services/
│   │   │   └── CoreInstaller.cs
│   │   └── Scenes/Persistent.unity
│   ├── 02_Features/
│   │   ├── Meta/
│   │   │   ├── Shop/
│   │   │   ├── Progression/
│   │   │   └── MetaInstaller.cs
│   │   ├── Gameplay/
│   │   │   ├── Core/
│   │   │   ├── Levels/
│   │   │   └── GameplayInstaller.cs
│   │   └── Scenes/
│   │       ├── Shell.unity (мета UI)
│   │       └── Gameplay.unity (геймплей)
│   ├── 03_Content/
│   │   ├── Configs/ (ScriptableObjects)
│   │   └── Prefabs/ (Addressable)
│   ├── 04_SDK/
│   │   ├── Analytics/
│   │   ├── Ads/
│   │   ├── IAP/
│   │   └── SDKInstaller.cs
│   └── 05_LiveOps/
│       ├── RemoteConfig/
│       ├── FeatureFlags/
│       └── LiveOpsInstaller.cs
├── Localization/
│   ├── StringTables/
│   └── AssetTables/
└── AddressableAssets/
    └── Groups/
        ├── 00_Static/
        ├── 01_Localization/
        ├── 02_Levels_Local/ (уровни 1-10)
        ├── 03_Levels_Remote/ (уровни 11+)
        ├── 04_UI/
        └── 05_Audio/
```

---

## Addressables Groups (Modular)

```
00_Static (Local)
  - GameConfig
  - CorePrefabs
  
01_Localization (Local)
  - StringTables_EN
  - StringTables_RU
  - StringTables_ZH
  - AssetTables (локализованные спрайты)

02_Levels_Local (Local)
  - Level_1 до Level_10
  Keys: "Level_1", "Level_2", ...

03_Levels_Remote (Remote) ← Новое для Modular
  - Level_11+
  Keys: "Level_11", "Level_12", ...
  
04_UI (Local)
  - Screen_MainMenu
  - Screen_Shop
  - Screen_Progression
  - Screen_Gameplay
  - Screen_Results

05_Audio (Mixed)
  - SFX_Local (частые звуки - Local)
  - Music_Remote (музыка - Remote)
```

---

## Сцены (4 сцены)

### 1. Bootstrap.unity
- **ProjectContext** (глобальный DI scope)
- **Содержит**: Только инсталлеры
- **Жизненный цикл**: Выгружается после инициализации

### 2. Persistent.unity
- **SceneContext** (core services)
- **Содержит**: Audio, Analytics, SaveSystem
- **Жизненный цикл**: Никогда не выгружается

### 3. Shell.unity
- **Мета UI**: магазин, карта, прогрессия
- **Жизненный цикл**: Выгружается при входе в уровень

### 4. Gameplay.unity
- **Игровой процесс**
- **Жизненный цикл**: Выгружается при выходе из уровня

---

## Bootstrap Flow

```
1. App Launch
   ↓
2. Bootstrap.unity loads
   ↓
3. ProjectContext.Awake() → ProjectInstaller.Configure()
   Регистрирует:
   - ContentService
   - LocalizationService
   - AppStateMachine
   - SceneLoader
   - SDK
   - LiveOps
   ↓
4. AppEntryPoint.Start()
   ↓
5. ContentBootstrap:
   - Initialize Localization
   - Preload core assets
   - Check Addressables catalog updates
   ↓
6. AppStateMachine.Enter<BootstrapState>()
   ↓
7. BootstrapState:
   - Initialize SDK
   - Fetch RemoteConfig
   - Load Persistent.unity (additive)
   ↓
8. PersistentState:
   - Register feature installers
   - Load Shell.unity (additive)
   - Unload Bootstrap.unity
   ↓
9. ShellState (meta UI)
   ↓
10. User clicks "Play Level 5"
   ↓
11. LoadLevelState:
    - Check if remote (Level > 10)
    - Download if needed
    - Unload Shell.unity
    - Load level via ContentService
    - Load Gameplay.unity
   ↓
12. GameplayState
   ↓
13. ResultsState → Back to Shell
```

---

## Код: ProjectInstaller (Bootstrap.unity)

```csharp
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectInstaller : LifetimeScope
{
    [SerializeField] private ContentConfig _contentConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Content & Localization (базовые)
        builder.RegisterInstance(_contentConfig);
        builder.Register<ContentService>(Lifetime.Singleton);
        builder.Register<LocalizationService>(Lifetime.Singleton);
        
        // Infrastructure
        builder.Register<AppStateMachine>(Lifetime.Singleton);
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<SaveSystem>(Lifetime.Singleton);
        
        // SDK
        builder.Register<AnalyticsService>(Lifetime.Singleton)
            .AsImplementedInterfaces();
        builder.Register<AdsService>(Lifetime.Singleton)
            .AsImplementedInterfaces();
        builder.Register<IAPService>(Lifetime.Singleton)
            .AsImplementedInterfaces();
        
        // LiveOps
        builder.Register<RemoteConfigService>(Lifetime.Singleton);
        builder.Register<FeatureFlagService>(Lifetime.Singleton);
        
        // States
        builder.Register<BootstrapState>(Lifetime.Transient);
        builder.Register<PersistentState>(Lifetime.Transient);
        builder.Register<ShellState>(Lifetime.Transient);
        builder.Register<LoadLevelState>(Lifetime.Transient);
        builder.Register<GameplayState>(Lifetime.Transient);
        builder.Register<ResultsState>(Lifetime.Transient);
        
        // Entry points
        builder.RegisterEntryPoint<ContentBootstrap>();
        builder.RegisterEntryPoint<AppEntryPoint>();
    }
}
```

---

## Код: ContentBootstrap (инициализация)

```csharp
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class ContentBootstrap : IStartable
{
    private readonly ContentService _content;
    private readonly LocalizationService _localization;
    private readonly ContentConfig _config;
    
    public ContentBootstrap(
        ContentService content,
        LocalizationService localization,
        ContentConfig config)
    {
        _content = content;
        _localization = localization;
        _config = config;
    }
    
    public async void Start()
    {
        // 1. Проверка обновлений каталога Addressables
        await CheckCatalogUpdatesAsync();
        
        // 2. Инициализация локализации
        await _localization.InitializeAsync();
        
        // 3. Предзагрузка ключевых ассетов
        await _content.PreloadAsync(_config.PreloadKeys);
        
        UnityEngine.Debug.Log("[ContentBootstrap] Ready");
    }
    
    private async UniTask CheckCatalogUpdatesAsync()
    {
        var catalogs = await Addressables.CheckForCatalogUpdates().Task;
        
        if (catalogs != null && catalogs.Count > 0)
        {
            UnityEngine.Debug.Log($"[ContentBootstrap] Updating {catalogs.Count} catalogs");
            await Addressables.UpdateCatalogs(catalogs).Task;
        }
    }
}
```

---

## Код: BootstrapState

```csharp
using Cysharp.Threading.Tasks;

public class BootstrapState : IState
{
    private readonly IAnalyticsService _analytics;
    private readonly RemoteConfigService _remoteConfig;
    private readonly SceneLoader _sceneLoader;
    private readonly AppStateMachine _stateMachine;
    
    public BootstrapState(
        IAnalyticsService analytics,
        RemoteConfigService remoteConfig,
        SceneLoader sceneLoader,
        AppStateMachine stateMachine)
    {
        _analytics = analytics;
        _remoteConfig = remoteConfig;
        _sceneLoader = sceneLoader;
        _stateMachine = stateMachine;
    }
    
    public async void Enter()
    {
        // 1. Initialize SDK
        _analytics.Initialize();
        
        // 2. Fetch RemoteConfig
        await _remoteConfig.FetchAsync();
        
        // 3. Load Persistent scene
        await _sceneLoader.LoadAdditiveAsync("Persistent");
        
        // 4. Transition
        _stateMachine.Enter<PersistentState>();
    }
    
    public void Exit() { }
}
```

---

## Код: LoadLevelState (с Remote Content)

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadLevelState : IPayloadedState<int>
{
    private readonly SceneLoader _sceneLoader;
    private readonly LevelContentFeature _levelContent;
    private readonly AppStateMachine _stateMachine;
    private readonly UIManager _ui;
    
    public LoadLevelState(
        SceneLoader sceneLoader,
        LevelContentFeature levelContent,
        AppStateMachine stateMachine,
        UIManager ui)
    {
        _sceneLoader = sceneLoader;
        _levelContent = levelContent;
        _stateMachine = stateMachine;
        _ui = ui;
    }
    
    public async void Enter(int levelIndex)
    {
        // Показать loading screen
        await _ui.ShowScreenAsync<LoadingScreen>("UI_Loading");
        
        // Unload Shell
        await _sceneLoader.UnloadAsync("Shell");
        
        // Загрузить уровень (с auto-download если remote)
        var level = await _levelContent.LoadLevelWithProgressAsync(
            levelIndex,
            new Progress<float>(OnDownloadProgress)
        );
        
        if (level == null)
        {
            Debug.LogError($"Failed to load level {levelIndex}");
            // Fallback: вернуться в Shell
            await _sceneLoader.LoadAdditiveAsync("Shell");
            _stateMachine.Enter<ShellState>();
            return;
        }
        
        // Load Gameplay scene
        await _sceneLoader.LoadAdditiveAsync("Gameplay");
        
        _stateMachine.Enter<GameplayState>();
    }
    
    private void OnDownloadProgress(float progress)
    {
        // Update loading screen progress bar
        _ui.UpdateLoadingProgress(progress);
    }
    
    public void Exit()
    {
        _ui.HideScreen("UI_Loading");
    }
}
```

---

## Код: LevelContentFeature (с Remote)

```csharp
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class LevelContentFeature : IFeature
{
    private readonly ContentService _content;
    private readonly Dictionary<int, GameObject> _loadedLevels = new();
    
    private const int LOCAL_LEVELS_COUNT = 10; // Уровни 1-10 local
    
    public LevelContentFeature(ContentService content)
    {
        _content = content;
    }
    
    public void Initialize()
    {
        // Опционально: предзагрузить Level_1
    }
    
    /// <summary>
    /// Загрузка уровня с автоматическим скачиванием
    /// </summary>
    public async UniTask<GameObject> LoadLevelWithProgressAsync(
        int levelIndex,
        System.IProgress<float> progress = null)
    {
        // Проверка кеша
        if (_loadedLevels.TryGetValue(levelIndex, out var cached))
        {
            return cached;
        }
        
        string key = $"Level_{levelIndex}";
        
        // Проверка: local или remote
        bool isRemote = levelIndex > LOCAL_LEVELS_COUNT;
        
        GameObject level;
        
        if (isRemote)
        {
            // Remote уровень: загрузка с download
            level = await _content.LoadWithDownloadAsync<GameObject>(key, progress);
        }
        else
        {
            // Local уровень: просто загрузка
            level = await _content.LoadAsync<GameObject>(key);
        }
        
        if (level != null)
        {
            // Инстанцировать
            var instance = Object.Instantiate(level);
            _loadedLevels[levelIndex] = instance;
            return instance;
        }
        
        return null;
    }
    
    /// <summary>
    /// Выгрузка уровня
    /// </summary>
    public void UnloadLevel(int levelIndex)
    {
        if (_loadedLevels.TryGetValue(levelIndex, out var level))
        {
            Object.Destroy(level);
            _loadedLevels.Remove(levelIndex);
        }
    }
    
    /// <summary>
    /// Получение размера скачивания
    /// </summary>
    public async UniTask<long> GetDownloadSizeAsync(int levelIndex)
    {
        string key = $"Level_{levelIndex}";
        return await _content.GetDownloadSizeAsync(key);
    }
    
    /// <summary>
    /// Предзагрузка следующего уровня (в background)
    /// </summary>
    public async UniTask PreloadNextLevelAsync(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        string key = $"Level_{nextLevel}";
        
        // Проверка доступности
        bool isAvailable = await _content.IsAvailableAsync(key);
        
        if (isAvailable)
        {
            // Preload в background
            await _content.LoadAsync<GameObject>(key);
        }
    }
}
```

---

## Код: PersistentInstaller (Persistent.unity)

```csharp
using VContainer;
using VContainer.Unity;

public class PersistentInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Feature modules
        builder.Register<ShopFeature>(Lifetime.Singleton);
        builder.Register<ProgressionFeature>(Lifetime.Singleton);
        builder.Register<LevelContentFeature>(Lifetime.Singleton);
        
        // UI
        builder.Register<UIManager>(Lifetime.Singleton);
        
        // Entry point для feature initialization
        builder.RegisterEntryPoint<FeaturesBootstrap>();
    }
}

public class FeaturesBootstrap : IStartable
{
    private readonly ShopFeature _shop;
    private readonly ProgressionFeature _progression;
    private readonly LevelContentFeature _levels;
    
    public FeaturesBootstrap(
        ShopFeature shop,
        ProgressionFeature progression,
        LevelContentFeature levels)
    {
        _shop = shop;
        _progression = progression;
        _levels = levels;
    }
    
    public void Start()
    {
        _shop.Initialize();
        _progression.Initialize();
        _levels.Initialize();
        
        UnityEngine.Debug.Log("[FeaturesBootstrap] Ready");
    }
}
```

---

## Код: ShopFeature (с RemoteConfig + Localization)

```csharp
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class ShopFeature : IFeature
{
    private readonly RemoteConfigService _remoteConfig;
    private readonly IAPService _iap;
    private readonly ContentService _content;
    private readonly LocalizationService _localization;
    
    private List<ShopItem> _items;
    
    public ShopFeature(
        RemoteConfigService remoteConfig,
        IAPService iap,
        ContentService content,
        LocalizationService localization)
    {
        _remoteConfig = remoteConfig;
        _iap = iap;
        _content = content;
        _localization = localization;
    }
    
    public void Initialize()
    {
        // Загрузка конфига магазина из RemoteConfig
        var itemsJson = _remoteConfig.GetValue<string>("shop_items", "[]");
        _items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShopItem>>(itemsJson);
        
        // Регистрация IAP продуктов
        var productIds = _items.Select(i => i.ProductId).ToArray();
        _iap.RegisterProducts(productIds);
    }
    
    public List<ShopItem> GetAvailableItems()
    {
        return _items;
    }
    
    public async UniTask<ShopItemView> CreateShopItemViewAsync(ShopItem item, Transform parent)
    {
        // Загрузка UI префаба
        var view = await _content.InstantiateAsync("UI_ShopItem", parent);
        
        // Получение локализованного названия
        string localizedName = _localization.GetString("Shop", item.NameKey);
        
        // Загрузка локализованной иконки
        var icon = await _localization.GetAssetAsync<Sprite>("Shop_Assets", item.IconKey);
        
        var shopItemView = view.GetComponent<ShopItemView>();
        shopItemView.Initialize(item, localizedName, icon);
        
        return shopItemView;
    }
}

[System.Serializable]
public class ShopItem
{
    public string Id;
    public string ProductId;
    public string NameKey; // Ключ в локализации
    public string IconKey; // Ключ в AssetTable
    public float Price;
}
```

---

## Localization Setup (Modular)

### String Tables

```
Table: UI
Keys:
- main_menu_title
- play_button
- shop_button
- settings_button
- level_complete
- level_failed

Table: Shop
Keys:
- coins_100_name
- coins_500_name
- remove_ads_name
- vip_pass_name

Table: Gameplay
Keys:
- score_label
- time_label
- pause_button
```

### Asset Tables

```
Table: Shop_Assets
Keys:
- icon_coins_100 (Sprite)
- icon_coins_500 (Sprite)
- icon_remove_ads (Sprite)

Table: UI_Assets
Keys:
- flag_en (Sprite)
- flag_ru (Sprite)
- flag_zh (Sprite)
```

---

## RemoteConfig Integration

### RemoteConfig Keys

```json
{
  "shop_items": "[{\"id\":\"coins_100\",\"productId\":\"com.game.coins100\",\"nameKey\":\"coins_100_name\",\"iconKey\":\"icon_coins_100\",\"price\":0.99}]",
  
  "level_timer": 60,
  "ads_frequency": 3,
  
  "feature_shop": true,
  "feature_daily_reward": true,
  "feature_leaderboard": false,
  
  "levels_local_count": 10,
  "levels_remote_start": 11
}
```

### FeatureFlagService Usage

```csharp
public class ShellState : IState
{
    private readonly FeatureFlagService _features;
    private readonly UIManager _ui;
    
    public async void Enter()
    {
        var menu = await _ui.ShowScreenAsync<MainMenuScreen>("UI_MainMenu");
        
        // Показать кнопку магазина только если фича включена
        bool shopEnabled = _features.IsEnabled("shop");
        menu.SetShopButtonVisible(shopEnabled);
        
        // Показать кнопку daily reward
        bool dailyRewardEnabled = _features.IsEnabled("daily_reward");
        menu.SetDailyRewardButtonVisible(dailyRewardEnabled);
    }
}
```

---

## Content Download UI

```csharp
public class DownloadPromptUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _titleText;
    [SerializeField] private TMPro.TextMeshProUGUI _sizeText;
    [SerializeField] private Button _downloadButton;
    [SerializeField] private Button _cancelButton;
    
    private LocalizationService _localization;
    private System.Action<bool> _callback;
    
    public void Initialize(
        LocalizationService localization,
        long downloadSizeBytes,
        System.Action<bool> callback)
    {
        _localization = localization;
        _callback = callback;
        
        // Локализованный текст
        _titleText.text = _localization.GetString("UI", "download_required");
        
        // Размер в MB
        float sizeMB = downloadSizeBytes / (1024f * 1024f);
        _sizeText.text = $"{sizeMB:F2} MB";
        
        _downloadButton.onClick.AddListener(() => OnAnswer(true));
        _cancelButton.onClick.AddListener(() => OnAnswer(false));
    }
    
    private void OnAnswer(bool download)
    {
        _callback?.Invoke(download);
        Destroy(gameObject);
    }
}
```

---

## Setup Instructions

### 1. Addressables Groups (добавить Remote)

```
Create Group: 03_Levels_Remote
- Build Path: ServerData/[BuildTarget]
- Load Path: https://cdn.yourgame.com/[BuildTarget]

Move Level_11+ to this group
```

### 2. Build Addressables Content

```
Window → Asset Management → Addressables → Build → Build Player Content
```

### 3. Upload to CDN

```bash
# Upload ServerData folder to CDN
aws s3 sync ./ServerData s3://yourgame-cdn/ --acl public-read

# Or use your CDN provider CLI
```

### 4. Test Remote Loading

```
Play Mode → Check catalog updates
Download Level_11 → Verify download UI
Load Level_11 → Verify gameplay
```

---

## ContentConfig (Modular)

```
Preload Keys:
- "GameConfig"
- "UI_MainMenu"
- "UI_Shop"
- "SFX_Common"
- "Level_1" (первый уровень предзагружен)

Settings:
- Use Memory Cache: ✅
- Max Cache Size: 100
- Max Concurrent Downloads: 3
- Download Timeout: 30s

Retry Settings:
- Max Retries: 3
- Retry Delay: 2s
```

---

## Performance Optimization

### 1. Preload Next Level
```csharp
// В GameplayState при старте уровня
await _levelContent.PreloadNextLevelAsync(_currentLevel);
```

### 2. Unload Previous Level
```csharp
// В LoadLevelState перед загрузкой нового
_levelContent.UnloadLevel(_currentLevel - 1);
```

### 3. Bundle Sizes
```
Level_1-10 (Local): 50MB total
Level_11+ (Remote): 5MB per level
UI (Local): 10MB total
Audio (Mixed): SFX 5MB (Local), Music 20MB (Remote)
```

---

## Testing Recommendations

### Unit Tests
```csharp
[Test]
public async Task LevelContentFeature_LoadRemoteLevel_Success()
{
    var feature = new LevelContentFeature(_content);
    
    var level = await feature.LoadLevelWithProgressAsync(11);
    
    Assert.IsNotNull(level);
}

[Test]
public async Task ShopFeature_LoadLocalizedItems_Success()
{
    await _localization.SetLocaleAsync("ru");
    var shop = new ShopFeature(_config, _iap, _content, _localization);
    
    var items = shop.GetAvailableItems();
    
    Assert.Greater(items.Count, 0);
}
```

### Integration Tests
```
1. Bootstrap flow → Persistent → Shell
2. Remote level download with progress UI
3. Language switch → UI updates
4. Feature flag toggle → UI changes
```

---

## Migration Path

### From Prototype → Modular (Week 4)

```
1. Add Persistent.unity, Shell.unity, Gameplay.unity
2. Create 03_Levels_Remote group
3. Move Level_11+ to remote
4. Add RemoteConfig
5. Add FeatureFlagService

КОД ContentService, LocalizationService: НЕ МЕНЯЕТСЯ
```

### To LiveOps (Month 6)

```
1. Add Event/Season groups
2. Add EventSystem, SeasonSystem
3. Add backend API integration

КОД ContentService, LocalizationService: НЕ МЕНЯЕТСЯ
```

---

## Когда использовать

### ✅ ДА:
- Hybrid-casual игры
- Merge, idle, puzzle жанры
- A/B тестирование
- Remote balance
- Команда 3-5 человек
- Жизненный цикл 3-12 месяцев
- **Готовность к масштабированию в LiveOps**

### ❌ НЕТ:
- Hypercasual прототипы (используй Single-Scene)
- Games-as-service (используй LiveOps Platform)

---

## Итого

**Modular подход даёт:**
- Remote контент без переписывания системы
- Локализация интегрирована с Addressables
- Feature Flags для A/B тестов
- RemoteConfig для баланса
- Готовность к эволюции в LiveOps
- Один и тот же ContentService + LocalizationService