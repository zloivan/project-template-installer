# Single-Scene Prototype (Hypercasual)

## Применение
- Быстрое прототипирование (1-3 недели на MVP)
- Hypercasual игры
- **С первого дня: Addressables + Localization**
- Готовность к масштабированию

---

## Структура папок

```
Assets/
├── _Project/
│   ├── Bootstrap/
│   │   └── BootstrapInstaller.cs
│   ├── Core/
│   │   ├── Content/
│   │   │   ├── ContentService.cs
│   │   │   └── ContentConfig.asset
│   │   ├── Localization/
│   │   │   ├── LocalizationService.cs
│   │   │   └── LocalizedText.cs
│   │   ├── StateMachine/
│   │   └── Services/
│   │       ├── ProgressService.cs
│   │       └── AnalyticsService.cs
│   ├── Game/
│   │   ├── Levels/
│   │   │   ├── LevelConfig.cs (ScriptableObject)
│   │   │   └── Prefabs/ (Addressable)
│   │   ├── Gameplay/
│   │   │   ├── Player/
│   │   │   └── Obstacles/
│   │   └── UI/
│   │       └── Screens/
│   └── SDK/
│       ├── AdsManager.cs
│       ├── IAPManager.cs
│       └── SDKInstaller.cs
├── Localization/
│   ├── StringTables/
│   │   ├── UI_en.asset
│   │   └── UI_ru.asset
│   └── AssetTables/
└── AddressableAssets/
    └── Groups/
        ├── 00_Static/
        ├── 01_Localization/
        ├── 02_Levels_Local/ (уровни 1-10)
        └── 04_UI/
```

---

## Addressables Groups (Prototype)

```
00_Static (Local)
  - GameConfig
  - CorePrefabs
  
01_Localization (Local)
  - StringTables_EN
  - StringTables_RU
  - UI_Assets (локализованные спрайты)

02_Levels_Local (Local)
  - Level_1 до Level_10
  Keys: "Level_1", "Level_2", ...

04_UI (Local)
  - Screen_MainMenu
  - Screen_Gameplay
  - Screen_Results
  Keys: "UI_MainMenu", "UI_Gameplay", ...
```

---

## Bootstrap Flow

```
App Launch
  ↓
Game.unity loads
  ↓
SceneContext.Awake()
  ↓
BootstrapInstaller регистрирует:
  - ContentService
  - LocalizationService
  - Core services
  - SDK
  - State Machine
  ↓
EntryPoint.Start()
  ↓
ContentBootstrap:
  - Initialize Localization (async)
  - Preload core assets
  ↓
GameStateMachine.Enter(LoadingState)
  ↓
LoadingState:
  - Load LevelConfig via ContentService
  ↓
GameplayState creates level
  ↓
ResultsState shows results
```

---

## Код: BootstrapInstaller

```csharp
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class BootstrapInstaller : LifetimeScope
{
    [SerializeField] private ContentConfig _contentConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Content & Localization (ОБЯЗАТЕЛЬНО с первого дня)
        builder.RegisterInstance(_contentConfig);
        builder.Register<ContentService>(Lifetime.Singleton);
        builder.Register<LocalizationService>(Lifetime.Singleton);
        
        // Core services
        builder.Register<ProgressService>(Lifetime.Singleton);
        builder.Register<AnalyticsService>(Lifetime.Singleton);
        
        // SDK
        builder.Register<AdsManager>(Lifetime.Singleton)
            .AsImplementedInterfaces();
        builder.Register<IAPManager>(Lifetime.Singleton)
            .AsImplementedInterfaces();
        
        // State machine
        builder.Register<GameStateMachine>(Lifetime.Singleton);
        builder.Register<LoadingState>(Lifetime.Transient);
        builder.Register<GameplayState>(Lifetime.Transient);
        builder.Register<ResultsState>(Lifetime.Transient);
        
        // Level
        builder.Register<LevelLoader>(Lifetime.Singleton);
        
        // UI
        builder.Register<UIManager>(Lifetime.Singleton);
        
        // Entry points
        builder.RegisterEntryPoint<ContentBootstrap>();
        builder.RegisterEntryPoint<GameEntryPoint>();
    }
}
```

---

## Код: ContentBootstrap (инициализация)

```csharp
using VContainer.Unity;
using Cysharp.Threading.Tasks;

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

## Код: GameEntryPoint

```csharp
using VContainer.Unity;

public class GameEntryPoint : IStartable
{
    private readonly GameStateMachine _stateMachine;
    private readonly ContentService _content;
    
    public GameEntryPoint(
        GameStateMachine stateMachine,
        ContentService content)
    {
        _stateMachine = stateMachine;
        _content = content;
    }
    
    public void Start()
    {
        // Ждём завершения ContentBootstrap через UniTask.Yield
        StartGameAsync().Forget();
    }
    
    private async Cysharp.Threading.Tasks.UniTaskVoid StartGameAsync()
    {
        // Небольшая задержка для завершения ContentBootstrap
        await Cysharp.Threading.Tasks.UniTask.Yield();
        
        _stateMachine.Enter<LoadingState>();
    }
}
```

---

## Код: LoadingState (с Addressables)

```csharp
using Cysharp.Threading.Tasks;

public class LoadingState : IState
{
    private readonly LevelLoader _levelLoader;
    private readonly GameStateMachine _stateMachine;
    private readonly UIManager _ui;
    
    public LoadingState(
        LevelLoader levelLoader, 
        GameStateMachine stateMachine,
        UIManager ui)
    {
        _levelLoader = levelLoader;
        _stateMachine = stateMachine;
        _ui = ui;
    }
    
    public async void Enter()
    {
        // Показать loading screen
        await _ui.ShowScreenAsync<LoadingScreen>("UI_Loading");
        
        // Загрузить уровень через Addressables
        var level = await _levelLoader.LoadCurrentLevelAsync();
        
        if (level != null)
        {
            _stateMachine.Enter<GameplayState>();
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load level");
        }
    }
    
    public void Exit()
    {
        _ui.HideScreen("UI_Loading");
    }
}
```

---

## Код: LevelLoader (Addressables)

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelLoader
{
    private readonly ContentService _content;
    private readonly ProgressService _progress;
    
    private GameObject _currentLevel;
    
    public LevelLoader(ContentService content, ProgressService progress)
    {
        _content = content;
        _progress = progress;
    }
    
    public async UniTask<GameObject> LoadCurrentLevelAsync()
    {
        int levelIndex = _progress.CurrentLevel;
        string key = $"Level_{levelIndex}";
        
        // Проверка доступности (для будущего remote контента)
        bool isAvailable = await _content.IsAvailableAsync(key);
        
        if (!isAvailable)
        {
            Debug.LogError($"Level {levelIndex} not available");
            return null;
        }
        
        // Загрузка и инстанцирование
        _currentLevel = await _content.InstantiateAsync(key);
        
        return _currentLevel;
    }
    
    public void UnloadCurrentLevel()
    {
        if (_currentLevel != null)
        {
            Object.Destroy(_currentLevel);
            _currentLevel = null;
        }
    }
}
```

---

## Код: UIManager (Addressables + Localization)

```csharp
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private readonly ContentService _content;
    private readonly LocalizationService _localization;
    private readonly Dictionary<string, GameObject> _openScreens = new();
    private readonly Transform _uiRoot;
    
    public UIManager(
        ContentService content, 
        LocalizationService localization)
    {
        _content = content;
        _localization = localization;
        
        // Создать UI root если нет
        _uiRoot = GameObject.Find("UIRoot")?.transform;
        if (_uiRoot == null)
        {
            var root = new GameObject("UIRoot");
            _uiRoot = root.transform;
            Object.DontDestroyOnLoad(root);
        }
    }
    
    public async UniTask<T> ShowScreenAsync<T>(string screenKey) where T : Component
    {
        // Загрузка UI префаба через Addressables
        var screen = await _content.InstantiateAsync(screenKey, _uiRoot);
        
        if (screen == null)
        {
            Debug.LogError($"Failed to load screen: {screenKey}");
            return null;
        }
        
        // Инициализация локализации
        InitializeLocalization(screen);
        
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
    
    private void InitializeLocalization(GameObject screen)
    {
        var localizedTexts = screen.GetComponentsInChildren<LocalizedText>(true);
        
        foreach (var text in localizedTexts)
        {
            text.Initialize(_localization);
        }
    }
}
```

---

## Код: LocalizedText Component

```csharp
using UnityEngine;
using TMPro;

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
}
```

---

## Код: ResultsState (с локализацией)

```csharp
using Cysharp.Threading.Tasks;

public class ResultsState : IState
{
    private readonly ProgressService _progress;
    private readonly GameStateMachine _stateMachine;
    private readonly UIManager _ui;
    private readonly LocalizationService _localization;
    private readonly LevelLoader _levelLoader;
    
    public ResultsState(
        ProgressService progress,
        GameStateMachine stateMachine,
        UIManager ui,
        LocalizationService localization,
        LevelLoader levelLoader)
    {
        _progress = progress;
        _stateMachine = stateMachine;
        _ui = ui;
        _localization = localization;
        _levelLoader = levelLoader;
    }
    
    public async void Enter()
    {
        // Выгрузить уровень
        _levelLoader.UnloadCurrentLevel();
        
        // Показать results screen
        var resultsScreen = await _ui.ShowScreenAsync<ResultsScreen>("UI_Results");
        
        // Установить локализованный текст
        string victoryText = _localization.GetString("UI", "victory_title");
        resultsScreen.SetTitle(victoryText);
        
        // Сохранить прогресс
        _progress.IncrementLevel();
        
        // Слушать кнопку "Next Level"
        resultsScreen.OnNextLevel += OnNextLevel;
    }
    
    private void OnNextLevel()
    {
        _stateMachine.Enter<LoadingState>();
    }
    
    public void Exit()
    {
        _ui.HideScreen("UI_Results");
    }
}
```

---

## Setup: Localization Tables

### 1. Создать String Tables
```
Window → Asset Management → Localization Tables

New Table Collection:
- Name: "UI"
- Type: String Table Collection

Add Locales:
- English (en)
- Russian (ru)

Add Entries:
Key                 | EN                    | RU
-------------------|----------------------|------------------
victory_title      | Victory!             | Победа!
defeat_title       | Defeat               | Поражение
next_level_button  | Next Level           | Следующий уровень
retry_button       | Retry                | Повтор
```

### 2. Пометить как Addressable
```
Select String Table Collection → Inspector:
✅ Addressable
Group: 01_Localization
```

---

## Setup: Levels как Addressables

### 1. Создать Level Prefabs
```
Create level prefabs: Level_1, Level_2, Level_3
```

### 2. Mark as Addressable
```
Select Level_1 → Inspector:
✅ Addressable
Address: "Level_1"
Group: 02_Levels_Local
Labels: "level", "local"
```

### 3. Repeat for all levels
```
Level_2: Address "Level_2"
Level_3: Address "Level_3"
...
```

---

## ContentConfig Setup

```
Create → Game → Content Config

Preload Keys:
- "GameConfig"
- "UI_MainMenu"
- "SFX_Common"

Settings:
- Use Memory Cache: ✅
- Max Cache Size: 50
- Max Concurrent Downloads: 3
```

---

## Migration to Remote (когда прототип успешен)

### Week 4: Add Remote Levels

```
1. Create new group:
   03_Levels_Remote (Remote)

2. Move Level_11+ to Remote group

3. Set Remote paths:
   Build Path: ServerData/[BuildTarget]
   Load Path: https://cdn.yourgame.com/[BuildTarget]

4. Build Addressables

5. Upload ServerData/ to CDN
```

**Код LevelLoader: НЕ МЕНЯЕТСЯ!** Автоматически скачает remote levels.

---

## Обязательные классы

### Core
1. `ContentService` - загрузка через Addressables
2. `LocalizationService` - управление языками
3. `ContentConfig` - конфигурация

### Bootstrap
4. `ContentBootstrap` - инициализация content/localization
5. `GameEntryPoint` - запуск игры

### Game
6. `LevelLoader` - загрузка уровней через ContentService
7. `UIManager` - управление UI через ContentService

### UI Components
8. `LocalizedText` - авто-локализация текстов

### States
9. `LoadingState` - загрузка уровня
10. `GameplayState` - игра
11. `ResultsState` - результаты

---

## Плюсы системы

### ✅ С первого дня
- Addressables готовы к remote контенту
- Локализация встроена
- Типобезопасные ссылки
- Кеширование

### ✅ Масштабирование
- Добавление remote групп без изменения кода
- Новые языки - просто новые таблицы
- A/B тестирование через ключи

### ✅ Production-ready
- Memory management
- Download с прогрессом
- Retry логика
- Error handling

---

## Testing Recommendations

### Unit Tests
```csharp
[Test]
public async Task ContentService_LoadLevel_Success()
{
    var content = new ContentService(_config);
    var level = await content.LoadAsync<GameObject>("Level_1");
    
    Assert.IsNotNull(level);
}

[Test]
public async Task LocalizationService_GetString_ReturnsTranslation()
{
    var localization = new LocalizationService();
    await localization.InitializeAsync();
    
    var text = localization.GetString("UI", "victory_title");
    
    Assert.IsNotEmpty(text);
}
```

### Play Mode Tests
```
1. Запуск игры → проверка локализации UI
2. Смена языка → проверка обновления текстов
3. Загрузка уровня → проверка Addressables
4. Выгрузка уровня → проверка memory cleanup
```

---

## Performance Tips

### Preloading
```csharp
// В ContentConfig.PreloadKeys добавить:
- "Level_1" (первый уровень)
- "UI_MainMenu" (главное меню)
- "SFX_Common" (частые звуки)
```

### Memory Management
```csharp
// В ResultsState после показа результатов:
_levelLoader.UnloadCurrentLevel();
_content.Unload($"Level_{previousLevel}");
```

### Bundle Sizes (рекомендации)
- Level: < 5MB
- UI Screen: < 1MB
- Localization Table: < 500KB

---

## Когда использовать

### ✅ ДА (ВСЕГДА):
- Любой новый прототип
- Hypercasual игры
- MVP за 2-4 недели
- Команда 1-2 человека
- **С расчётом на масштабирование**

### ❌ НЕТ (НИКОГДА):
- ~~Если не планируете локализацию~~ (всегда планируете)
- ~~Если не нужен remote контент~~ (понадобится позже)
- ~~Если слишком сложно~~ (настраивается один раз)

---

## Итого

**Этот подход с первого дня даёт:**
- Addressables вместо Resources (правильно)
- Локализация готова (2+ языка легко)
- Готовность к remote контенту (добавляется без рефакторинга)
- Production-quality code (не технический долг)
- Быстрое прототипирование (не медленнее Resources)

**Результат: прототип, который становится продуктом, а не выбрасывается.**