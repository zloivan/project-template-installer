# Content & Localization Core System

## Философия
**Правильная архитектура с первого дня.** Прототипы не выбрасывают - их масштабируют. Resources.Load = технический долг. Addressables + Localization с дня 1.

---

## Универсальная структура (для всех архитектур)

```
Assets/
├── _Project/
│   ├── Core/
│   │   ├── Content/
│   │   │   ├── ContentService.cs (единый для всех)
│   │   │   ├── ContentConfig.cs (ScriptableObject)
│   │   │   └── AssetReference/
│   │   │       ├── LevelReference.cs
│   │   │       ├── UIReference.cs
│   │   │       └── AudioReference.cs
│   │   └── Localization/
│   │       ├── LocalizationService.cs (единый для всех)
│   │       ├── LocalizedText.cs (UI компонент)
│   │       └── LocalizedAsset.cs (спрайты, аудио)
│   └── Content/
│       ├── Configs/ (ScriptableObjects)
│       ├── Levels/
│       ├── UI/
│       └── Audio/
└── AddressableAssets/
    ├── Settings/
    │   └── AddressableAssetSettings.asset
    └── Groups/
        ├── 00_Static/
        ├── 01_Localization/
        ├── 02_Levels_Local/
        ├── 03_Levels_Remote/
        └── 04_UI/
```

---

## Addressables Groups Setup (универсальная)

### Для Prototype (минимальная)
```
00_Static (Local)
  - GameConfig
  - CorePrefabs

01_Localization (Local)
  - StringTables_EN
  - StringTables_RU

02_Levels (Local)
  - Level_1 до Level_10

04_UI (Local)
  - Screens
  - Widgets
```

### Для Modular (расширенная)
```
+ 03_Levels_Remote (Remote)
  - Level_11+

+ 05_Audio_Remote (Remote)
  - Music

+ 06_Events (Remote)
  - Event content
```

### Для LiveOps (полная)
```
+ 07_Seasons (Remote)
+ 08_DLC (Remote)
+ 09_AssetBundles_PerCountry (Remote)
```

**Ключ: одна и та же структура, просто добавляем группы при масштабировании.**

---

## Код: ContentService (ядро, единое для всех)

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ContentService
{
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();
    private readonly Dictionary<string, object> _cache = new();
    private readonly ContentConfig _config;
    
    public ContentService(ContentConfig config)
    {
        _config = config;
    }
    
    /// <summary>
    /// Загрузка ассета с кешированием (основной метод)
    /// </summary>
    public async UniTask<T> LoadAsync<T>(string key) where T : Object
    {
        // Проверка кеша
        if (_cache.TryGetValue(key, out var cached))
        {
            return cached as T;
        }
        
        // Загрузка
        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _handles[key] = handle;
            _cache[key] = handle.Result;
            return handle.Result;
        }
        
        Debug.LogError($"[ContentService] Failed to load: {key}");
        return null;
    }
    
    /// <summary>
    /// Загрузка с автоматическим скачиванием (для Remote content)
    /// </summary>
    public async UniTask<T> LoadWithDownloadAsync<T>(
        string key, 
        System.IProgress<float> progress = null) where T : Object
    {
        // Проверка размера скачивания
        var downloadSize = await GetDownloadSizeAsync(key);
        
        if (downloadSize > 0)
        {
            // Скачивание зависимостей
            await DownloadAsync(key, progress);
        }
        
        // Загрузка ассета
        return await LoadAsync<T>(key);
    }
    
    /// <summary>
    /// Инстанцирование префаба (для уровней, UI)
    /// </summary>
    public async UniTask<GameObject> InstantiateAsync(
        string key, 
        Transform parent = null)
    {
        var prefab = await LoadAsync<GameObject>(key);
        
        if (prefab == null) return null;
        
        var instance = Object.Instantiate(prefab, parent);
        return instance;
    }
    
    /// <summary>
    /// Получение размера скачивания
    /// </summary>
    public async UniTask<long> GetDownloadSizeAsync(string key)
    {
        var handle = Addressables.GetDownloadSizeAsync(key);
        await handle;
        
        long size = handle.Result;
        Addressables.Release(handle);
        
        return size;
    }
    
    /// <summary>
    /// Скачивание контента с прогрессом
    /// </summary>
    public async UniTask DownloadAsync(
        string key, 
        System.IProgress<float> progress = null)
    {
        var handle = Addressables.DownloadDependenciesAsync(key);
        
        while (!handle.IsDone)
        {
            progress?.Report(handle.GetDownloadStatus().Percent);
            await UniTask.Yield();
        }
        
        await handle;
        Addressables.Release(handle);
    }
    
    /// <summary>
    /// Выгрузка ассета
    /// </summary>
    public void Unload(string key)
    {
        if (_handles.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _handles.Remove(key);
            _cache.Remove(key);
        }
    }
    
    /// <summary>
    /// Проверка доступности контента (локально или remote)
    /// </summary>
    public async UniTask<bool> IsAvailableAsync(string key)
    {
        var locations = await Addressables.LoadResourceLocationsAsync(key).Task;
        return locations.Count > 0;
    }
    
    /// <summary>
    /// Предзагрузка списка ассетов
    /// </summary>
    public async UniTask PreloadAsync(IEnumerable<string> keys)
    {
        var tasks = new List<UniTask>();
        
        foreach (var key in keys)
        {
            tasks.Add(LoadAsync<Object>(key));
        }
        
        await UniTask.WhenAll(tasks);
    }
    
    /// <summary>
    /// Очистка всего кеша
    /// </summary>
    public void ClearCache()
    {
        foreach (var handle in _handles.Values)
        {
            Addressables.Release(handle);
        }
        
        _handles.Clear();
        _cache.Clear();
    }
}
```

---

## Код: ContentConfig (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "ContentConfig", menuName = "Game/Content Config")]
public class ContentConfig : ScriptableObject
{
    [Header("Preload на старте")]
    public string[] PreloadKeys = new[]
    {
        "GameConfig",
        "CoreUI",
        "SFX_Common"
    };
    
    [Header("Настройки кеша")]
    public bool UseMemoryCache = true;
    public int MaxCacheSize = 100;
    
    [Header("Настройки загрузки")]
    public int MaxConcurrentDownloads = 3;
    public float DownloadTimeout = 30f;
    
    [Header("Retry политика")]
    public int MaxRetries = 3;
    public float RetryDelay = 1f;
}
```

---

## Код: LocalizationService (Unity Localization Package)

```csharp
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class LocalizationService
{
    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";
    
    private Locale _currentLocale;
    
    public Locale CurrentLocale => _currentLocale;
    public event System.Action<Locale> OnLocaleChanged;
    
    /// <summary>
    /// Инициализация (вызывается в Bootstrap)
    /// </summary>
    public async UniTask InitializeAsync()
    {
        // Дождаться инициализации Localization System
        await LocalizationSettings.InitializationOperation;
        
        // Загрузить сохранённый язык или использовать системный
        var savedLanguage = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, "");
        
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            await SetLocaleAsync(savedLanguage);
        }
        else
        {
            await SetSystemLocaleAsync();
        }
    }
    
    /// <summary>
    /// Установка языка
    /// </summary>
    public async UniTask SetLocaleAsync(string localeCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        
        if (locale == null)
        {
            UnityEngine.Debug.LogError($"[LocalizationService] Locale not found: {localeCode}");
            return;
        }
        
        LocalizationSettings.SelectedLocale = locale;
        _currentLocale = locale;
        
        // Сохранить выбор
        PlayerPrefs.SetString(LANGUAGE_PREF_KEY, localeCode);
        PlayerPrefs.Save();
        
        // Дождаться загрузки таблиц
        await UniTask.WaitUntil(() => !LocalizationSettings.SelectedLocaleAsync.IsDone);
        
        OnLocaleChanged?.Invoke(locale);
    }
    
    /// <summary>
    /// Установка системного языка
    /// </summary>
    public async UniTask SetSystemLocaleAsync()
    {
        var systemLocale = LocalizationSettings.AvailableLocales.GetLocale(
            UnityEngine.Application.systemLanguage
        );
        
        if (systemLocale != null)
        {
            await SetLocaleAsync(systemLocale.Identifier.Code);
        }
        else
        {
            // Fallback на английский
            await SetLocaleAsync("en");
        }
    }
    
    /// <summary>
    /// Получение переведённой строки
    /// </summary>
    public string GetString(string tableName, string key)
    {
        var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
        
        if (stringTable == null)
        {
            UnityEngine.Debug.LogError($"[LocalizationService] Table not found: {tableName}");
            return $"[{key}]";
        }
        
        var entry = stringTable.GetEntry(key);
        return entry?.GetLocalizedString() ?? $"[{key}]";
    }
    
    /// <summary>
    /// Получение переведённой строки async (для больших таблиц)
    /// </summary>
    public async UniTask<string> GetStringAsync(string tableName, string key)
    {
        var operation = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
        await operation;
        
        var table = operation.Result;
        var entry = table.GetEntry(key);
        
        return entry?.GetLocalizedString() ?? $"[{key}]";
    }
    
    /// <summary>
    /// Получение локализованного ассета (спрайт, аудио)
    /// </summary>
    public async UniTask<T> GetAssetAsync<T>(string tableName, string key) where T : UnityEngine.Object
    {
        var operation = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<T>(
            new TableEntryReference(tableName, key)
        );
        
        await operation;
        return operation.Result;
    }
    
    /// <summary>
    /// Получить список доступных языков
    /// </summary>
    public List<Locale> GetAvailableLocales()
    {
        return LocalizationSettings.AvailableLocales.Locales;
    }
}
```

---

## Код: LocalizedText (UI компонент)

```csharp
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string _tableName = "UI";
    [SerializeField] private string _key;
    
    private TextMeshProUGUI _text;
    private LocalizationService _localization;
    
    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    
    public void Initialize(LocalizationService localization)
    {
        _localization = localization;
        _localization.OnLocaleChanged += OnLocaleChanged;
        
        UpdateText();
    }
    
    private void OnDestroy()
    {
        if (_localization != null)
        {
            _localization.OnLocaleChanged -= OnLocaleChanged;
        }
    }
    
    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateText();
    }
    
    private void UpdateText()
    {
        if (_localization != null)
        {
            _text.text = _localization.GetString(_tableName, _key);
        }
    }
    
    public void SetKey(string tableName, string key)
    {
        _tableName = tableName;
        _key = key;
        UpdateText();
    }
}
```

---

## Код: LevelReference (типобезопасные ссылки)

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class LevelReference
{
    [SerializeField] private int _levelIndex;
    [SerializeField] private AssetReference _assetReference;
    
    public int LevelIndex => _levelIndex;
    public string Key => _assetReference.RuntimeKey.ToString();
    
    public async Cysharp.Threading.Tasks.UniTask<GameObject> LoadAsync(ContentService content)
    {
        return await content.LoadAsync<GameObject>(Key);
    }
}

[System.Serializable]
public class UIReference
{
    [SerializeField] private string _screenName;
    [SerializeField] private AssetReference _assetReference;
    
    public string ScreenName => _screenName;
    public string Key => _assetReference.RuntimeKey.ToString();
    
    public async Cysharp.Threading.Tasks.UniTask<GameObject> InstantiateAsync(
        ContentService content, 
        Transform parent = null)
    {
        return await content.InstantiateAsync(Key, parent);
    }
}
```

---

## Installer (регистрация сервисов)

```csharp
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class CoreContentInstaller : LifetimeScope
{
    [SerializeField] private ContentConfig _contentConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Content
        builder.RegisterInstance(_contentConfig);
        builder.Register<ContentService>(Lifetime.Singleton);
        
        // Localization
        builder.Register<LocalizationService>(Lifetime.Singleton);
        
        // Entry point для инициализации
        builder.RegisterEntryPoint<ContentBootstrap>();
    }
}

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
        // 1. Инициализация локализации
        await _localization.InitializeAsync();
        
        // 2. Предзагрузка ключевых ассетов
        await _content.PreloadAsync(_config.PreloadKeys);
        
        UnityEngine.Debug.Log("[ContentBootstrap] Ready");
    }
}
```

---

## Использование: LevelLoader (обновлённый)

```csharp
public class LevelLoader
{
    private readonly ContentService _content;
    private readonly ProgressService _progress;
    
    public LevelLoader(ContentService content, ProgressService progress)
    {
        _content = content;
        _progress = progress;
    }
    
    public async UniTask<GameObject> LoadCurrentLevelAsync()
    {
        int levelIndex = _progress.CurrentLevel;
        string key = $"Level_{levelIndex}";
        
        // Проверка размера скачивания
        var downloadSize = await _content.GetDownloadSizeAsync(key);
        
        if (downloadSize > 0)
        {
            // Показать UI прогресса
            var progress = new Progress<float>(p => UpdateDownloadUI(p));
            await _content.DownloadAsync(key, progress);
        }
        
        // Загрузка и инстанцирование
        return await _content.InstantiateAsync(key);
    }
    
    private void UpdateDownloadUI(float progress)
    {
        // Обновить UI прогресс бар
    }
}
```

---

## Использование: UIManager (обновлённый)

```csharp
public class UIManager
{
    private readonly ContentService _content;
    private readonly LocalizationService _localization;
    private readonly Dictionary<string, GameObject> _openScreens = new();
    
    public UIManager(ContentService content, LocalizationService localization)
    {
        _content = content;
        _localization = localization;
    }
    
    public async UniTask<T> ShowScreenAsync<T>(string screenKey) where T : Component
    {
        // Загрузка UI префаба через Addressables
        var screen = await _content.InstantiateAsync(screenKey);
        
        if (screen == null) return null;
        
        // Инициализация локализации для всех LocalizedText компонентов
        var localizedTexts = screen.GetComponentsInChildren<LocalizedText>(true);
        foreach (var text in localizedTexts)
        {
            text.Initialize(_localization);
        }
        
        _openScreens[screenKey] = screen;
        
        return screen.GetComponent<T>();
    }
    
    public void HideScreen(string screenKey)
    {
        if (_openScreens.TryGetValue(screenKey, out var screen))
        {
            Object.Destroy(screen);
            _openScreens.Remove(screenKey);
        }
    }
}
```

---

## Setup Instructions (один раз навсегда)

### 1. Установить пакеты
```
Package Manager:
- Addressables (com.unity.addressables)
- Localization (com.unity.localization)
- UniTask (опционально, но рекомендуется)
```

### 2. Настроить Addressables
```
Window → Asset Management → Addressables → Groups

Create Groups:
- 00_Static (Local)
- 01_Localization (Local)
- 02_Levels_Local (Local)

Settings:
✅ Build Remote Catalog: ON
✅ Compress Bundles: ON
```

### 3. Настроить Localization
```
Window → Asset Management → Localization Tables

Create String Table Collection: "UI"
Add Locales: English (en), Russian (ru), Chinese (zh)

Create Asset Table Collection: "UI_Assets"
(для локализованных спрайтов, аудио)
```

### 4. Пометить ассеты как Addressable
```
Select asset → Inspector:
✅ Addressable
Address: "Level_1" (уникальный ключ)
Group: 02_Levels_Local
```

### 5. Создать ContentConfig
```
Create → Game → Content Config
Assign в CoreContentInstaller
```

### 6. Добавить CoreContentInstaller в сцену
```
GameObject → VContainer → LifetimeScope
Assign CoreContentInstaller script
```

---

## Migration Path (эволюция без переписывания)

### Day 1: Prototype
```
Groups:
- 00_Static (Local)
- 01_Localization (EN, RU)
- 02_Levels_Local (1-10)

Код: тот же ContentService + LocalizationService
```

### Week 4: Successful Prototype → Add Remote
```
+ 03_Levels_Remote (11+)

Настройка:
- Set Remote Build Path
- Update ContentConfig.PreloadKeys

Код: НЕ МЕНЯЕТСЯ, только добавляем ключи
```

### Month 3: Hybrid-Casual → Add LiveOps
```
+ 04_Events_Remote
+ 05_Seasons_Remote

Добавляем:
- EventSystem (использует ContentService)
- SeasonSystem (использует ContentService)

Код ContentService: НЕ МЕНЯЕТСЯ
```

### Month 6: LiveOps Platform
```
+ 06_DLC_Remote
+ 07_AssetBundles_PerCountry

Добавляем:
- ContentStreamingService (обёртка над ContentService)
- Region-specific loading logic

Код ContentService: НЕ МЕНЯЕТСЯ
```

**Ключ: одна и та же базовая система масштабируется без переписывания.**

---

## Обязательные классы (для всех архитектур)

### Core
1. `ContentService` - загрузка через Addressables
2. `LocalizationService` - управление языками
3. `ContentConfig` - конфигурация (ScriptableObject)

### UI Components
4. `LocalizedText` - автоматическая локализация TextMeshPro
5. `LocalizedAsset<T>` - локализованные спрайты/аудио

### References
6. `LevelReference` - типобезопасные ссылки на уровни
7. `UIReference` - типобезопасные ссылки на UI

### Bootstrap
8. `ContentBootstrap` - инициализация при старте

---

## Best Practices

### ✅ DO
- Всегда используй ContentService для загрузки
- Всегда используй LocalizationService для текстов
- Группируй ассеты логически (Level_1, Level_2...)
- Используй AssetReference в inspector для типобезопасности
- Кешируй часто используемые ассеты
- Предзагружай критичные ассеты при старте

### ❌ DON'T
- НЕ используй Resources.Load (НИКОГДА)
- НЕ хардкодь строки напрямую в UI
- НЕ загружай ассеты в Update()
- НЕ забывай вызывать Unload для больших ассетов
- НЕ ставь всё в одну группу Addressables

---

## Debug Tools

### ContentServiceDebugger
```csharp
public class ContentServiceDebugger : MonoBehaviour
{
    private ContentService _content;
    
    private void OnGUI()
    {
        GUILayout.Label($"Cached Assets: {_content.CachedCount}");
        GUILayout.Label($"Active Handles: {_content.HandleCount}");
        
        if (GUILayout.Button("Clear Cache"))
        {
            _content.ClearCache();
        }
    }
}
```

---

## Performance Tips

### Preloading Strategy
```csharp
// В LoadingState предзагружай следующий уровень
await _content.PreloadAsync(new[] 
{
    $"Level_{currentLevel + 1}",
    "UI_Results",
    "SFX_Victory"
});
```

### Memory Management
```csharp
// Выгружай предыдущие уровни
_content.Unload($"Level_{currentLevel - 1}");
```

### Bundle Size
- UI: < 5MB per screen
- Levels: < 10MB per level
- Audio: < 2MB per track (compressed)
- Localization: < 1MB per language

---

## Troubleshooting

### "Asset not found"
```
1. Check Addressable address matches key
2. Build Addressables content
3. Check group assignment
```

### "Download failed"
```
1. Check Remote Load Path в Settings
2. Verify CDN URL accessibility
3. Check network connectivity
```

### "Localization missing"
```
1. Check locale code (en, ru, zh)
2. Verify String Table exists
3. Check key spelling
```