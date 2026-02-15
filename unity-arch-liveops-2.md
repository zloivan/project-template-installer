# Shell + Content Streaming (LiveOps Platform)

## Применение
- Games-as-service (Clash Royale, Brawl Stars модель)
- Еженедельные события
- Сезонный контент
- Тяжёлая зависимость от сервера
- Постоянное обновление без релизов
- **100% контента через Addressables**
- **Multi-region localization**
- **APK < 50MB**

---

## Структура папок

```
Assets/
├── _Project/
│   ├── Shell/
│   │   ├── Core/
│   │   │   ├── ShellBootstrap.cs
│   │   │   ├── ShellInstaller.cs
│   │   │   ├── ContentService.cs (тот же)
│   │   │   └── LocalizationService.cs (тот же)
│   │   ├── Streaming/
│   │   │   ├── ContentStreamingService.cs
│   │   │   ├── ContentCatalogService.cs
│   │   │   └── RegionalContentService.cs
│   │   ├── LiveOps/
│   │   │   ├── EventSystem.cs
│   │   │   ├── SeasonSystem.cs
│   │   │   └── LiveContentManager.cs
│   │   ├── UI/
│   │   │   ├── MainMenu/
│   │   │   ├── Events/
│   │   │   └── Seasons/
│   │   └── Scenes/Shell.unity
│   ├── Backend/
│   │   ├── API/
│   │   │   ├── EventsAPI.cs
│   │   │   ├── SeasonsAPI.cs
│   │   │   └── ContentAPI.cs
│   │   ├── DTOs/
│   │   └── Sync/
│   └── SDK/
├── Localization/
│   ├── StringTables_Core/ (минимум для Shell)
│   └── DynamicTables/ (загружаются по событиям)
└── AddressableAssets/
    ├── Settings/
    │   └── Profiles/
    │       ├── Dev (Local CDN)
    │       ├── Staging (Staging CDN)
    │       ├── Production (Production CDN)
    │       └── Regional_CN (China CDN)
    └── Groups/
        ├── 00_Shell_Minimal/ (Local, < 30MB)
        ├── 01_Localization_Core/ (Local top 3)
        ├── 02_Localization_Extended/ (Remote)
        ├── 03_Season_1/ (Remote)
        ├── 04_Season_2/ (Remote)
        ├── 05_Event_Valentine/ (Remote)
        ├── 06_Event_Halloween/ (Remote)
        ├── 07_Mode_Battle/ (Remote)
        ├── 08_Mode_PvP/ (Remote)
        └── 09_Regional_CN/ (Remote)
```

---

## Addressables Groups (LiveOps)

### Minimal Shell (в билде, < 30MB)

```
00_Shell_Minimal (Local)
  Keys:
  - "Shell_UI_Core"
  - "Loading_Screen"
  - "Error_Screen"
  - "Core_Icons"
  
  Build: Include in Player
  Load Path: Local

01_Localization_Core (Local)
  Keys:
  - "StringTable_EN"
  - "StringTable_ES"
  - "StringTable_PT"
  
  Build: Include in Player
  Load Path: Local
  Size: ~3MB
```

### Dynamic Content (все Remote)

```
02_Localization_Extended (Remote)
  Keys:
  - "StringTable_RU"
  - "StringTable_ZH"
  - "StringTable_JA"
  - "StringTable_KO"
  - "AssetTable_Icons_RU"
  
  Build: Remote
  Load Path: https://cdn.yourgame.com/localization/
  Size: ~5MB

03_Season_1 (Remote)
  Keys:
  - "Season1_Scene"
  - "Season1_Characters"
  - "Season1_Items"
  - "Season1_UI"
  
  Build: Remote
  Load Path: https://cdn.yourgame.com/seasons/season1/
  Size: ~50MB

04_Event_Valentine (Remote)
  Keys:
  - "Event_Valentine_Scene"
  - "Event_Valentine_Items"
  - "Event_Valentine_UI"
  
  Build: Remote
  Load Path: https://cdn.yourgame.com/events/valentine/
  Size: ~20MB

07_Mode_Battle (Remote)
  Keys:
  - "Mode_Battle_Scene"
  - "Battle_Arenas"
  - "Battle_Characters"
  
  Build: Remote
  Load Path: https://cdn.yourgame.com/modes/battle/
  Size: ~40MB

09_Regional_CN (Remote)
  Keys:
  - "CN_Censored_Characters"
  - "CN_Payment_UI"
  - "CN_Legal_Screens"
  
  Build: Remote
  Load Path: https://cdn-cn.yourgame.com/regional/
  Size: ~10MB
  Region: China only
```

---

## Bootstrap Flow

```
1. App Launch
   ↓
2. Shell.unity loads (< 30MB в памяти)
   ↓
3. ShellBootstrap.Initialize()
   - Init SDK (Analytics, Ads, IAP)
   - Connect to Backend
   - Authenticate Player
   ↓
4. ContentCatalogService.UpdateCatalog()
   - Fetch latest catalog from CDN
   - Check catalog version
   - Update if needed (без перезапуска)
   ↓
5. LocalizationService.Initialize()
   - Detect system language
   - Load core strings (Local)
   - Download extended if needed (Remote)
   ↓
6. EventSystem.RefreshActiveEvents()
   - Query backend: /api/events/active
   - Parse event list
   - Check local availability
   - Prompt download if new event
   ↓
7. SeasonSystem.LoadCurrentSeason()
   - Query backend: /api/seasons/current
   - Check if season content downloaded
   - Prompt download if needed
   ↓
8. ShellStateMachine.Enter(MainMenuState)
   - Show main menu
   - Display active events (banners)
   - Highlight season pass
   - Show available modes
   ↓
9. User selects Event / Season / Mode
   ↓
10. ContentStreamingService.LoadContentAsync(key)
    - Check if cached locally
    - Download if not (with progress UI)
    - Load scene additive
    ↓
11. Content plays (Event, Season, Mode)
    ↓
12. ContentStreamingService.UnloadContentAsync(key)
    - Unload scene
    - Release assets
    - Return to Shell
```

---

## Код: ShellBootstrap

```csharp
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShellBootstrap : IStartable
{
    private readonly ContentService _content;
    private readonly LocalizationService _localization;
    private readonly ContentCatalogService _catalog;
    private readonly IBackendClient _backend;
    private readonly EventSystem _eventSystem;
    private readonly SeasonSystem _seasonSystem;
    private readonly ShellStateMachine _stateMachine;
    
    public ShellBootstrap(
        ContentService content,
        LocalizationService localization,
        ContentCatalogService catalog,
        IBackendClient backend,
        EventSystem eventSystem,
        SeasonSystem seasonSystem,
        ShellStateMachine stateMachine)
    {
        _content = content;
        _localization = localization;
        _catalog = catalog;
        _backend = backend;
        _eventSystem = eventSystem;
        _seasonSystem = seasonSystem;
        _stateMachine = stateMachine;
    }
    
    public async void Start()
    {
        await InitializeAsync();
    }
    
    private async UniTask InitializeAsync()
    {
        // 1. Check и update Addressables catalog
        await _catalog.UpdateCatalogAsync();
        
        // 2. Инициализация локализации
        await _localization.InitializeAsync();
        
        // 3. Подключение к backend
        await _backend.ConnectAsync();
        await _backend.AuthenticateAsync();
        
        // 4. Проверка активных событий
        await _eventSystem.RefreshActiveEventsAsync();
        
        // 5. Загрузка текущего сезона
        await _seasonSystem.LoadCurrentSeasonDataAsync();
        
        // 6. Предзагрузка критичного контента
        await PreloadEssentialContentAsync();
        
        // 7. Переход в main menu
        _stateMachine.Enter<MainMenuState>();
        
        Debug.Log("[ShellBootstrap] Initialization complete");
    }
    
    private async UniTask PreloadEssentialContentAsync()
    {
        // Preload main menu UI если ещё не загружен
        await _content.LoadAsync<GameObject>("Shell_UI_MainMenu");
    }
}
```

---

## Код: ContentCatalogService

```csharp
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class ContentCatalogService
{
    private readonly IBackendClient _backend;
    private ContentCatalog _localCatalog;
    private ContentCatalog _remoteCatalog;
    
    public ContentCatalogService(IBackendClient backend)
    {
        _backend = backend;
    }
    
    /// <summary>
    /// Проверка и обновление каталога Addressables
    /// </summary>
    public async UniTask UpdateCatalogAsync()
    {
        // 1. Получить версию каталога с backend
        var remoteVersion = await _backend.GetContentCatalogVersionAsync();
        
        // 2. Сравнить с локальной версией
        var localVersion = PlayerPrefs.GetString("CatalogVersion", "0");
        
        if (remoteVersion != localVersion)
        {
            Debug.Log($"[ContentCatalog] Updating from {localVersion} to {remoteVersion}");
            
            // 3. Обновить каталог Addressables
            var catalogs = await Addressables.CheckForCatalogUpdates().Task;
            
            if (catalogs != null && catalogs.Count > 0)
            {
                await Addressables.UpdateCatalogs(catalogs).Task;
            }
            
            // 4. Сохранить новую версию
            PlayerPrefs.SetString("CatalogVersion", remoteVersion);
            PlayerPrefs.Save();
        }
        
        // 5. Загрузить каталог контента с backend
        _remoteCatalog = await _backend.GetContentCatalogAsync();
    }
    
    /// <summary>
    /// Получить доступные моды
    /// </summary>
    public List<ModeDefinition> GetAvailableModes()
    {
        return _remoteCatalog?.Modes ?? new List<ModeDefinition>();
    }
    
    /// <summary>
    /// Получить доступные события
    /// </summary>
    public List<EventDefinition> GetAvailableEvents()
    {
        return _remoteCatalog?.Events ?? new List<EventDefinition>();
    }
    
    /// <summary>
    /// Получить текущий сезон
    /// </summary>
    public SeasonDefinition GetCurrentSeason()
    {
        return _remoteCatalog?.CurrentSeason;
    }
}

[System.Serializable]
public class ContentCatalog
{
    public int Version;
    public List<ModeDefinition> Modes;
    public List<EventDefinition> Events;
    public SeasonDefinition CurrentSeason;
}

[System.Serializable]
public class ModeDefinition
{
    public string Id;
    public string Name;
    public string ContentKey; // Addressable key
    public bool IsAvailable;
    public long DownloadSizeBytes;
}
```

---

## Код: ContentStreamingService

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ContentStreamingService
{
    private readonly ContentService _content;
    private readonly Dictionary<string, AsyncOperationHandle<UnityEngine.SceneManagement.Scene>> _loadedScenes = new();
    
    public ContentStreamingService(ContentService content)
    {
        _content = content;
    }
    
    /// <summary>
    /// Загрузка контента (сцены) с автоматическим скачиванием
    /// </summary>
    public async UniTask<bool> LoadContentAsync(
        string contentKey,
        System.IProgress<DownloadProgress> progress = null)
    {
        // Проверка: уже загружен?
        if (_loadedScenes.ContainsKey(contentKey))
        {
            Debug.Log($"[ContentStreaming] {contentKey} already loaded");
            return true;
        }
        
        // 1. Проверка размера скачивания
        var downloadSize = await _content.GetDownloadSizeAsync(contentKey);
        
        if (downloadSize > 0)
        {
            float sizeMB = downloadSize / (1024f * 1024f);
            Debug.Log($"[ContentStreaming] {contentKey} requires {sizeMB:F2}MB download");
            
            // 2. Скачивание dependencies
            await DownloadContentAsync(contentKey, progress);
        }
        
        // 3. Загрузка сцены
        var handle = Addressables.LoadSceneAsync(
            contentKey,
            UnityEngine.SceneManagement.LoadSceneMode.Additive
        );
        
        await handle.Task;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedScenes[contentKey] = handle;
            
            // Set active scene
            var scene = handle.Result.Scene;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            
            Debug.Log($"[ContentStreaming] {contentKey} loaded successfully");
            return true;
        }
        
        Debug.LogError($"[ContentStreaming] Failed to load {contentKey}");
        return false;
    }
    
    /// <summary>
    /// Выгрузка контента
    /// </summary>
    public async UniTask UnloadContentAsync(string contentKey)
    {
        if (!_loadedScenes.TryGetValue(contentKey, out var handle))
        {
            Debug.LogWarning($"[ContentStreaming] {contentKey} not loaded");
            return;
        }
        
        await Addressables.UnloadSceneAsync(handle).Task;
        _loadedScenes.Remove(contentKey);
        
        Debug.Log($"[ContentStreaming] {contentKey} unloaded");
    }
    
    /// <summary>
    /// Скачивание контента с прогрессом
    /// </summary>
    private async UniTask DownloadContentAsync(
        string contentKey,
        System.IProgress<DownloadProgress> progress)
    {
        var handle = Addressables.DownloadDependenciesAsync(contentKey);
        
        while (!handle.IsDone)
        {
            var status = handle.GetDownloadStatus();
            
            progress?.Report(new DownloadProgress
            {
                Percent = status.Percent,
                DownloadedBytes = status.DownloadedBytes,
                TotalBytes = status.TotalBytes
            });
            
            await UniTask.Yield();
        }
        
        await handle.Task;
        Addressables.Release(handle);
    }
    
    /// <summary>
    /// Получение размера скачивания для списка ключей
    /// </summary>
    public async UniTask<long> GetBatchDownloadSizeAsync(IEnumerable<string> keys)
    {
        long totalSize = 0;
        
        foreach (var key in keys)
        {
            totalSize += await _content.GetDownloadSizeAsync(key);
        }
        
        return totalSize;
    }
}

public struct DownloadProgress
{
    public float Percent;
    public long DownloadedBytes;
    public long TotalBytes;
    
    public float SizeMB => TotalBytes / (1024f * 1024f);
    public float DownloadedMB => DownloadedBytes / (1024f * 1024f);
}
```

---

## Код: EventSystem

```csharp
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class EventSystem
{
    private readonly ContentStreamingService _streaming;
    private readonly IBackendClient _backend;
    private readonly IEventBus _eventBus;
    private readonly LocalizationService _localization;
    
    private List<EventData> _activeEvents = new();
    
    public EventSystem(
        ContentStreamingService streaming,
        IBackendClient backend,
        IEventBus eventBus,
        LocalizationService localization)
    {
        _streaming = streaming;
        _backend = backend;
        _eventBus = eventBus;
        _localization = localization;
    }
    
    /// <summary>
    /// Обновление списка активных событий с backend
    /// </summary>
    public async UniTask RefreshActiveEventsAsync()
    {
        // Fetch from backend
        _activeEvents = await _backend.GetActiveEventsAsync();
        
        // Publish event для обновления UI
        _eventBus.Publish(new ActiveEventsUpdatedEvent(_activeEvents));
        
        UnityEngine.Debug.Log($"[EventSystem] {_activeEvents.Count} active events");
    }
    
    /// <summary>
    /// Старт события
    /// </summary>
    public async UniTask<bool> StartEventAsync(string eventId)
    {
        var eventData = _activeEvents.FirstOrDefault(e => e.Id == eventId);
        
        if (eventData == null)
        {
            UnityEngine.Debug.LogError($"[EventSystem] Event {eventId} not found");
            return false;
        }
        
        // Проверка: контент уже скачан?
        var downloadSize = await _streaming.GetBatchDownloadSizeAsync(
            eventData.ContentKeys
        );
        
        if (downloadSize > 0)
        {
            // Показать download prompt
            string title = _localization.GetString("Events", "download_required");
            string message = _localization.GetString("Events", "event_download_message");
            
            bool confirmed = await ShowDownloadPromptAsync(
                eventData.Name,
                message,
                downloadSize
            );
            
            if (!confirmed)
            {
                return false;
            }
            
            // Скачивание с progress UI
            var progress = new System.Progress<DownloadProgress>(p =>
            {
                UpdateDownloadUI(eventData.Name, p);
            });
            
            foreach (var key in eventData.ContentKeys)
            {
                await _streaming.LoadContentAsync(key, progress);
            }
        }
        
        // Загрузка контента события
        bool success = await _streaming.LoadContentAsync(eventData.MainSceneKey);
        
        if (success)
        {
            // Analytics
            Analytics.TrackEvent("event_started", new Dictionary<string, object>
            {
                { "event_id", eventId },
                { "event_type", eventData.Type },
                { "download_size_mb", downloadSize / (1024f * 1024f) }
            });
            
            // Publish event
            _eventBus.Publish(new EventStartedEvent(eventData));
        }
        
        return success;
    }
    
    /// <summary>
    /// Завершение события
    /// </summary>
    public async UniTask EndEventAsync(string eventId)
    {
        var eventData = _activeEvents.FirstOrDefault(e => e.Id == eventId);
        
        if (eventData != null)
        {
            await _streaming.UnloadContentAsync(eventData.MainSceneKey);
            
            _eventBus.Publish(new EventEndedEvent(eventData));
        }
    }
    
    private async UniTask<bool> ShowDownloadPromptAsync(
        string eventName,
        string message,
        long sizeBytes)
    {
        // Показать UI prompt через EventBus или UIManager
        // Return user decision
        return true; // Simplified
    }
    
    private void UpdateDownloadUI(string eventName, DownloadProgress progress)
    {
        // Update UI progress bar
    }
    
    public List<EventData> GetActiveEvents() => _activeEvents;
}

[System.Serializable]
public class EventData
{
    public string Id;
    public string Name;
    public string Description;
    public string MainSceneKey; // Addressable key главной сцены
    public List<string> ContentKeys; // Все ключи контента события
    public EventType Type;
    public System.DateTime StartTime;
    public System.DateTime EndTime;
    
    public bool IsActive => 
        System.DateTime.UtcNow >= StartTime && 
        System.DateTime.UtcNow <= EndTime;
}

public enum EventType
{
    Tournament,
    Challenge,
    Seasonal,
    Limited
}
```

---

## Код: SeasonSystem

```csharp
using Cysharp.Threading.Tasks;

public class SeasonSystem
{
    private readonly ContentStreamingService _streaming;
    private readonly IBackendClient _backend;
    private readonly IEventBus _eventBus;
    
    private SeasonData _currentSeason;
    
    public SeasonSystem(
        ContentStreamingService streaming,
        IBackendClient backend,
        IEventBus eventBus)
    {
        _streaming = streaming;
        _backend = backend;
        _eventBus = eventBus;
    }
    
    /// <summary>
    /// Загрузка данных текущего сезона
    /// </summary>
    public async UniTask LoadCurrentSeasonDataAsync()
    {
        // Fetch from backend
        _currentSeason = await _backend.GetCurrentSeasonAsync();
        
        if (_currentSeason == null)
        {
            UnityEngine.Debug.LogWarning("[SeasonSystem] No active season");
            return;
        }
        
        UnityEngine.Debug.Log($"[SeasonSystem] Current season: {_currentSeason.Name}");
        
        // Publish для UI
        _eventBus.Publish(new SeasonLoadedEvent(_currentSeason));
    }
    
    /// <summary>
    /// Загрузка контента сезона
    /// </summary>
    public async UniTask<bool> LoadSeasonContentAsync()
    {
        if (_currentSeason == null)
        {
            return false;
        }
        
        // Проверка размера
        var downloadSize = await _streaming.GetBatchDownloadSizeAsync(
            _currentSeason.ContentKeys
        );
        
        if (downloadSize > 0)
        {
            // Download prompt
            float sizeMB = downloadSize / (1024f * 1024f);
            UnityEngine.Debug.Log($"[SeasonSystem] Season requires {sizeMB:F2}MB");
            
            // Show UI и wait for confirmation
            // Simplified - auto download
            
            var progress = new System.Progress<DownloadProgress>(p =>
            {
                UpdateSeasonDownloadUI(p);
            });
            
            foreach (var key in _currentSeason.ContentKeys)
            {
                await _streaming.LoadContentAsync(key, progress);
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Получить прогресс в сезоне
    /// </summary>
    public async UniTask<SeasonProgress> GetPlayerProgressAsync()
    {
        return await _backend.GetSeasonProgressAsync(_currentSeason.Id);
    }
    
    private void UpdateSeasonDownloadUI(DownloadProgress progress)
    {
        // Update UI
    }
    
    public SeasonData GetCurrentSeason() => _currentSeason;
}

[System.Serializable]
public class SeasonData
{
    public string Id;
    public int SeasonNumber;
    public string Name;
    public string MainSceneKey;
    public List<string> ContentKeys;
    public System.DateTime StartTime;
    public System.DateTime EndTime;
    public string ThemeId;
}

[System.Serializable]
public class SeasonProgress
{
    public int CurrentTier;
    public int CurrentXP;
    public List<SeasonReward> UnlockedRewards;
}
```

---

## Код: RegionalContentService

```csharp
using Cysharp.Threading.Tasks;

public class RegionalContentService
{
    private readonly ContentService _content;
    private readonly IBackendClient _backend;
    
    private string _region;
    
    public RegionalContentService(
        ContentService content,
        IBackendClient backend)
    {
        _content = content;
        _backend = backend;
    }
    
    /// <summary>
    /// Определение региона игрока
    /// </summary>
    public async UniTask InitializeAsync()
    {
        // Получить регион с backend (по IP или настройкам аккаунта)
        _region = await _backend.GetPlayerRegionAsync();
        
        UnityEngine.Debug.Log($"[RegionalContent] Region: {_region}");
    }
    
    /// <summary>
    /// Загрузка региональных ассетов
    /// </summary>
    public async UniTask<T> LoadRegionalAssetAsync<T>(string baseKey) 
        where T : UnityEngine.Object
    {
        // Попытка загрузить региональную версию
        string regionalKey = $"{baseKey}_{_region}";
        
        bool hasRegional = await _content.IsAvailableAsync(regionalKey);
        
        if (hasRegional)
        {
            UnityEngine.Debug.Log($"[RegionalContent] Loading regional: {regionalKey}");
            return await _content.LoadAsync<T>(regionalKey);
        }
        
        // Fallback на базовую версию
        UnityEngine.Debug.Log($"[RegionalContent] Loading default: {baseKey}");
        return await _content.LoadAsync<T>(baseKey);
    }
    
    /// <summary>
    /// Пример: загрузка персонажа с региональными правками
    /// </summary>
    public async UniTask<GameObject> LoadCharacterAsync(string characterId)
    {
        // В Китае могут быть цензурированные модели
        if (_region == "CN")
        {
            return await LoadRegionalAssetAsync<GameObject>($"Character_{characterId}_CN");
        }
        
        return await _content.LoadAsync<GameObject>($"Character_{characterId}");
    }
}
```

---

## ShellInstaller

```csharp
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ShellInstaller : LifetimeScope
{
    [SerializeField] private ContentConfig _contentConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Core (базовые сервисы)
        builder.RegisterInstance(_contentConfig);
        builder.Register<ContentService>(Lifetime.Singleton);
        builder.Register<LocalizationService>(Lifetime.Singleton);
        
        // Streaming
        builder.Register<ContentStreamingService>(Lifetime.Singleton);
        builder.Register<ContentCatalogService>(Lifetime.Singleton);
        builder.Register<RegionalContentService>(Lifetime.Singleton);
        
        // LiveOps
        builder.Register<EventSystem>(Lifetime.Singleton);
        builder.Register<SeasonSystem>(Lifetime.Singleton);
        builder.Register<LiveContentManager>(Lifetime.Singleton);
        
        // State Machine
        builder.Register<ShellStateMachine>(Lifetime.Singleton);
        
        // Backend
        builder.Register<IBackendClient, BackendClient>(Lifetime.Singleton);
        builder.Register<EventsAPI>(Lifetime.Singleton);
        builder.Register<SeasonsAPI>(Lifetime.Singleton);
        
        // UI
        builder.Register<UIManager>(Lifetime.Singleton);
        
        // Events
        builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
        
        // SDK
        builder.Register<IAnalyticsService, AnalyticsService>(Lifetime.Singleton);
        builder.Register<IAdsService, AdsService>(Lifetime.Singleton);
        builder.Register<IAPService>(Lifetime.Singleton);
        
        // Entry
        builder.RegisterEntryPoint<ShellBootstrap>();
    }
}
```

---

## Content Update Pipeline (без релиза)

### 1. Дизайнер создаёт событие

```
1. Create event content (scenes, prefabs, configs)
2. Mark as Addressable
3. Assign keys: "Event_NewYear_Scene", "Event_NewYear_UI"
4. Add to group: "06_Event_NewYear" (Remote)
```

### 2. Build Addressables

```
Window → Addressables → Build → Build Player Content

Результат:
- ServerData/Android/06_Event_NewYear_bundle.bundle
- catalog_06_Event_NewYear.json
```

### 3. Upload to CDN

```bash
# Upload to Production CDN
aws s3 sync ./ServerData/Android/ s3://yourgame-cdn/android/ --acl public-read

# Или через CDN provider CLI
gcloud storage cp ./ServerData/Android/* gs://yourgame-cdn/android/
```

### 4. Update Backend

```json
POST /api/events
{
  "id": "event_newyear_2026",
  "name": "New Year 2026",
  "mainSceneKey": "Event_NewYear_Scene",
  "contentKeys": [
    "Event_NewYear_Scene",
    "Event_NewYear_UI",
    "Event_NewYear_Items"
  ],
  "startTime": "2025-12-31T00:00:00Z",
  "endTime": "2026-01-07T23:59:59Z",
  "type": "seasonal"
}
```

### 5. Client автоматически обнаружит

```csharp
// При следующем запуске или каждые N минут
await _catalog.UpdateCatalogAsync();
await _eventSystem.RefreshActiveEventsAsync();

// Новое событие появится в UI
// Пользователь скачает контент при клике
```

**Результат:** Новое событие без обновления APK/IPA!

---

## Localization для LiveOps

### Core Tables (Local, в билде)

```
Table: Shell_Core
Keys:
- main_menu_title
- events_button
- seasons_button
- loading_text
- error_title
```

### Dynamic Tables (Remote, для событий)

```
Table: Event_Valentine
Keys:
- event_title: "Valentine's Day Challenge"
- event_description: "Find love, earn rewards!"
- reward_description: "Exclusive Valentine skin"

AssetTable: Event_Valentine_Assets
Keys:
- banner_image (Sprite)
- background_music (AudioClip)
- icon_heart (Sprite)
```

### Загрузка динамической локализации

```csharp
public async UniTask LoadEventLocalizationAsync(string eventId)
{
    string tableKey = $"Event_{eventId}_Strings";
    
    // Download если нужно
    var size = await _content.GetDownloadSizeAsync(tableKey);
    
    if (size > 0)
    {
        await _content.DownloadAsync(tableKey);
    }
    
    // Load table
    var table = await _localization.LoadTableAsync(tableKey);
    
    // Теперь можно использовать
    string title = _localization.GetString($"Event_{eventId}", "event_title");
}
```

---

## Monitoring & Analytics

### Обязательные метрики

```csharp
// Catalog updates
Analytics.TrackEvent("catalog_updated", new 
{
    old_version,
    new_version,
    duration_seconds
});

// Content downloads
Analytics.TrackEvent("content_download_started", new 
{
    content_key,
    content_type, // event, season, mode
    download_size_mb
});

Analytics.TrackEvent("content_download_completed", new 
{
    content_key,
    duration_seconds,
    success
});

// Event participation
Analytics.TrackEvent("event_started", new 
{
    event_id,
    player_level,
    was_downloaded_now // true если скачали сейчас
});

// Season engagement
Analytics.TrackEvent("season_tier_unlocked", new 
{
    season_id,
    tier_number,
    time_to_unlock_minutes
});

// Regional content
Analytics.TrackEvent("regional_content_loaded", new 
{
    region,
    content_type,
    was_fallback // true если regional не найден
});
```

### Funnel Tracking

```
App Launch
  ↓ (track: duration)
Catalog Update
  ↓ (track: events_shown)
Events List Shown
  ↓ (track: click_rate)
Event Clicked
  ↓ (track: download_prompt_shown)
Download Prompt
  ↓ (track: download_acceptance_rate)
Download Started
  ↓ (track: download_success_rate)
Download Completed
  ↓ (track: event_start_rate)
Event Started
```

---

## Performance Optimization

### 1. Catalog Caching

```csharp
// Cache catalog locally для оффлайн работы
public class ContentCatalogService
{
    private const string CACHE_KEY = "CachedCatalog";
    
    private async UniTask<ContentCatalog> GetCatalogAsync()
    {
        // Try server first
        try
        {
            var catalog = await _backend.GetContentCatalogAsync();
            
            // Cache успешный результат
            SaveToCache(catalog);
            
            return catalog;
        }
        catch
        {
            // Fallback на кеш
            return LoadFromCache();
        }
    }
}
```

### 2. Preloading Strategy

```csharp
// Предзагрузка популярного контента в background
public async UniTask PreloadPopularContentAsync()
{
    var popularEvents = await _backend.GetPopularEventsAsync();
    
    foreach (var evt in popularEvents.Take(2))
    {
        // Silent download в background
        await _streaming.LoadContentAsync(evt.MainSceneKey, silent: true);
    }
}
```

### 3. Memory Management

```csharp
// Автоматическая выгрузка старого контента
public class ContentMemoryManager
{
    private Queue<string> _loadedContent = new();
    private const int MAX_CACHED_CONTENT = 3;
    
    public async UniTask LoadWithMemoryManagementAsync(string key)
    {
        // Unload oldest если превышен лимит
        if (_loadedContent.Count >= MAX_CACHED_CONTENT)
        {
            var oldest = _loadedContent.Dequeue();
            await _streaming.UnloadContentAsync(oldest);
        }
        
        // Load new
        await _streaming.LoadContentAsync(key);
        _loadedContent.Enqueue(key);
    }
}
```

---

## CDN Setup

### Multi-Region CDN

```
Production Regions:
- North America: cdn-na.yourgame.com
- Europe: cdn-eu.yourgame.com
- Asia: cdn-asia.yourgame.com
- China: cdn-cn.yourgame.com (отдельный провайдер)

Addressables Profile: Production_NA
Remote Load Path: https://cdn-na.yourgame.com/{BuildTarget}
```

### CDN Selection Logic

```csharp
public class CDNSelector
{
    public string GetOptimalCDN()
    {
        // Detect player region
        string region = DetectRegion();
        
        return region switch
        {
            "NA" => "https://cdn-na.yourgame.com",
            "EU" => "https://cdn-eu.yourgame.com",
            "ASIA" => "https://cdn-asia.yourgame.com",
            "CN" => "https://cdn-cn.yourgame.com",
            _ => "https://cdn-na.yourgame.com" // fallback
        };
    }
}
```

---

## Testing Strategy

### Local Testing (без CDN)

```
Addressables Profile: Dev_Local
- Remote Build Path: ServerData/[BuildTarget]
- Remote Load Path: file://{UnityEngine.Application.dataPath}/../ServerData/[BuildTarget]

Test flow:
1. Build Addressables
2. Play in Editor
3. Content загружается локально
```

### Staging Environment

```
Addressables Profile: Staging
- Remote Load Path: https://cdn-staging.yourgame.com/[BuildTarget]

Backend: api-staging.yourgame.com

Test flow:
1. Deploy to staging CDN
2. Build test APK
3. Test download flow
4. Verify analytics
```

### Production

```
Addressables Profile: Production
- Remote Load Path: https://cdn.yourgame.com/[BuildTarget]

Backend: api.yourgame.com

Release flow:
1. Deploy to production CDN
2. Staged rollout (10% → 50% → 100%)
3. Monitor metrics
4. A/B test new events
```

---

## Обязательные классы

### Shell Core
1. `ShellBootstrap` - Инициализация всей системы
2. `ShellInstaller` - DI setup
3. `ShellStateMachine` - Shell flow

### Streaming
4. `ContentStreamingService` - Управление динамическим контентом
5. `ContentCatalogService` - Управление каталогом
6. `RegionalContentService` - Региональный контент

### LiveOps
7. `EventSystem` - События
8. `SeasonSystem` - Сезоны
9. `LiveContentManager` - Оркестрация live контента

### Backend
10. `BackendClient` - HTTP клиент
11. `EventsAPI` - События API
12. `SeasonsAPI` - Сезоны API
13. `ContentAPI` - Контент API

### Core (те же что и везде)
14. `ContentService` - Базовая загрузка
15. `LocalizationService` - Базовая локализация

---

## Bundle Size Targets (LiveOps)

```
Shell APK (Android): < 50MB
  - Shell Scene: 10MB
  - Core UI: 5MB
  - Core Localization (3 языка): 3MB
  - SDK libraries: 15MB
  - Code: 10MB
  - Other: 7MB

Remote Content (per piece):
  - Event: 15-30MB
  - Season: 40-60MB
  - Mode: 30-50MB
  - Localization (per language): 1-2MB
  - Regional Content: 5-15MB

Total максимум для игрока:
  - Базовый APK: 50MB
  - 1 Season: 50MB
  - 2 Events: 60MB
  - 5 Modes: 200MB
  = ~360MB максимум
```

---

## Когда использовать

### ✅ ДА:
- Games-as-service
- Постоянная команда контента (дизайнеры, художники, QA)
- Бюджет на infrastructure (CDN, backend, DevOps)
- Команда 10+ человек
- Established IP / brand
- План на 2+ года поддержки
- **Эволюция из Clean Architecture**

### ❌ НЕТ:
- Инди проекты (< 5 человек)
- Офлайн игры
- Ограниченный бюджет (< $500k/год на ops)
- Прототипы
- Нет backend team
- Нет плана на live ops

---

## Примеры игр

- **Supercell** (Brawl Stars, Clash Royale): Еженедельные события, сезоны
- **Tencent** (PUBG Mobile, Call of Duty Mobile): Massive seasonal updates
- **Riot Games** (Wild Rift, TFT Mobile): Сезонные патчи, события
- **King** (Candy Crush): Weekly events, level packs

---

## Итого

**LiveOps Platform даёт:**
- ✅ Обновления без релизов
- ✅ A/B тестирование контента
- ✅ Минимальный APK (< 50MB)
- ✅ Regional content
- ✅ Сезонная retention
- ✅ Постоянный новый контент
- ✅ **Эволюция из Clean Architecture без переписывания**
- ✅ **Те же ContentService + LocalizationService**

**Время на миграцию из Clean Architecture: 2-4 недели инфраструктуры, не месяцы переписывания кода.**