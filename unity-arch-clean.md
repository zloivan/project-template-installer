# Clean Architecture + Features (Midcore)

## Применение
- Жизненный цикл 1+ год
- Сложная экономика
- PvP / События
- Тяжёлая интеграция с бэкендом
- Большая команда (5-10+ человек)
- **ContentService + LocalizationService в Domain слое**

---

## Структура папок

```
Assets/
├── _Project/
│   ├── 00_Bootstrap/
│   │   ├── Scenes/Bootstrap.unity
│   │   └── ProjectInstaller.cs
│   ├── 01_Core/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Player.cs
│   │   │   │   ├── Item.cs
│   │   │   │   └── Currency.cs
│   │   │   ├── UseCases/
│   │   │   │   ├── PurchaseItemUseCase.cs
│   │   │   │   ├── EquipItemUseCase.cs
│   │   │   │   └── StartBattleUseCase.cs
│   │   │   └── Interfaces/
│   │   │       ├── IContentRepository.cs
│   │   │       ├── IInventoryRepository.cs
│   │   │       └── IWalletRepository.cs
│   │   ├── Application/
│   │   │   ├── Services/
│   │   │   │   ├── ContentService.cs (базовый)
│   │   │   │   ├── LocalizationService.cs (базовый)
│   │   │   │   ├── InventoryService.cs
│   │   │   │   └── EconomyService.cs
│   │   │   └── Events/
│   │   │       ├── IEventBus.cs
│   │   │       └── EventBus.cs
│   │   ├── Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── InventoryRepository.cs
│   │   │   │   ├── ContentRepository.cs (Addressables)
│   │   │   │   └── SaveSystem.cs
│   │   │   ├── Network/
│   │   │   │   ├── BackendClient.cs
│   │   │   │   └── DTOs/
│   │   │   └── StateMachine/
│   │   └── Presentation/
│   │       ├── UI/
│   │       │   ├── Screens/
│   │       │   └── Widgets/
│   │       └── ViewModels/
│   │           ├── ShopViewModel.cs
│   │           └── InventoryViewModel.cs
│   ├── 02_Features/
│   │   ├── Battle/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   ├── Infrastructure/
│   │   │   ├── Presentation/
│   │   │   └── BattleInstaller.cs
│   │   ├── Inventory/
│   │   └── Social/
│   ├── 03_Shared/
│   │   ├── Events/
│   │   ├── Utils/
│   │   └── Extensions/
│   └── 04_Content/
│       ├── Configs/
│       └── Prefabs/
├── Localization/
│   ├── StringTables/
│   └── AssetTables/
└── AddressableAssets/
    └── Groups/
        ├── 00_Static/
        ├── 01_Localization/
        ├── 02_Content_Local/
        ├── 03_Content_Remote/
        ├── 04_UI/
        └── 05_Features/ (per-feature bundles)
```

---

## Архитектурные слои (с Content интеграцией)

### Domain (Inner Layer)
- **Entities**: Бизнес-сущности
- **UseCases**: Бизнес-логика
- **Interfaces**: Контракты для Repository (включая IContentRepository)

### Application Layer
- **Services**: ContentService, LocalizationService, etc
- **Events**: EventBus для decoupled communication

### Infrastructure Layer
- **Repositories**: Реализация через Addressables
- **Network**: Backend интеграция
- **Persistence**: Save system

### Presentation Layer
- **Views**: MonoBehaviour UI
- **ViewModels**: Presentation logic с локализацией

---

## Addressables Groups (Clean Architecture)

```
00_Static (Local)
  - GameConfig
  - ItemDefinitions (ScriptableObjects)
  
01_Localization (Local + Remote)
  - StringTables_EN, RU, ZH, JA, KO (Local top 3, Remote others)
  - AssetTables (локализованные иконки, аудио)

02_Content_Local (Local)
  - Characters_Starter (первые герои)
  - Items_Common (частые предметы)
  - Levels_Tutorial (обучение)

03_Content_Remote (Remote)
  - Characters_Premium
  - Items_Legendary
  - Levels_Campaign
  - Skins

04_UI (Local)
  - Screens (все UI экраны)
  - Widgets (переиспользуемые компоненты)

05_Features (Remote, по фичам)
  - Battle_Content
  - PvP_Content
  - Events_Content
  - Social_Content
```

---

## Код: IContentRepository (Domain Interface)

```csharp
namespace Core.Domain.Interfaces
{
    public interface IContentRepository
    {
        Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;
        Task<GameObject> InstantiateAsync(string key, Transform parent = null);
        Task<long> GetDownloadSizeAsync(string key);
        Task DownloadAsync(string key, IProgress<float> progress = null);
        void Unload(string key);
    }
    
    public interface IInventoryRepository
    {
        Task<ItemDefinition> GetItemDefinitionAsync(string itemId);
        Task<List<Item>> GetPlayerInventoryAsync();
        Task AddItemAsync(Item item);
        Task RemoveItemAsync(string itemId);
    }
}
```

---

## Код: ContentRepository (Infrastructure)

```csharp
namespace Core.Infrastructure.Persistence
{
    public class ContentRepository : IContentRepository
    {
        private readonly ContentService _contentService;
        
        public ContentRepository(ContentService contentService)
        {
            _contentService = contentService;
        }
        
        public async Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
        {
            return await _contentService.LoadAsync<T>(key);
        }
        
        public async Task<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            return await _contentService.InstantiateAsync(key, parent);
        }
        
        public async Task<long> GetDownloadSizeAsync(string key)
        {
            return await _contentService.GetDownloadSizeAsync(key);
        }
        
        public async Task DownloadAsync(string key, IProgress<float> progress = null)
        {
            await _contentService.DownloadAsync(key, progress);
        }
        
        public void Unload(string key)
        {
            _contentService.Unload(key);
        }
    }
}
```

---

## Код: InventoryRepository (с Addressables)

```csharp
namespace Core.Infrastructure.Persistence
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IBackendClient _backend;
        private readonly ISaveSystem _saveSystem;
        private readonly IContentRepository _content;
        
        public InventoryRepository(
            IBackendClient backend,
            ISaveSystem saveSystem,
            IContentRepository content)
        {
            _backend = backend;
            _saveSystem = saveSystem;
            _content = content;
        }
        
        /// <summary>
        /// Загрузка definition предмета через Addressables
        /// </summary>
        public async Task<ItemDefinition> GetItemDefinitionAsync(string itemId)
        {
            // Загрузка ScriptableObject через ContentRepository
            string key = $"ItemDef_{itemId}";
            return await _content.LoadAssetAsync<ItemDefinition>(key);
        }
        
        /// <summary>
        /// Получение inventory игрока (с синхронизацией)
        /// </summary>
        public async Task<List<Item>> GetPlayerInventoryAsync()
        {
            // Try local save first
            var localInventory = _saveSystem.Get<PlayerInventory>("inventory");
            
            if (localInventory != null && !localInventory.NeedsSync)
            {
                return localInventory.Items;
            }
            
            // Fetch from backend
            var serverInventory = await _backend.GetInventoryAsync();
            
            // Save locally
            _saveSystem.Set("inventory", serverInventory);
            _saveSystem.Save();
            
            return serverInventory.Items;
        }
        
        public async Task AddItemAsync(Item item)
        {
            var inventory = _saveSystem.Get<PlayerInventory>("inventory");
            inventory.Items.Add(item);
            inventory.NeedsSync = true;
            
            _saveSystem.Set("inventory", inventory);
            _saveSystem.Save();
            
            // Sync to backend
            await _backend.SyncInventoryAsync(inventory);
            inventory.NeedsSync = false;
        }
        
        public async Task RemoveItemAsync(string itemId)
        {
            var inventory = _saveSystem.Get<PlayerInventory>("inventory");
            inventory.Items.RemoveAll(i => i.Id == itemId);
            
            await _backend.SyncInventoryAsync(inventory);
        }
    }
}
```

---

## Код: PurchaseItemUseCase (с локализацией)

```csharp
namespace Core.Domain.UseCases
{
    public class PurchaseItemUseCase
    {
        private readonly IInventoryRepository _inventory;
        private readonly IWalletRepository _wallet;
        private readonly IContentRepository _content;
        private readonly IAnalyticsService _analytics;
        private readonly IEventBus _eventBus;
        
        public PurchaseItemUseCase(
            IInventoryRepository inventory,
            IWalletRepository wallet,
            IContentRepository content,
            IAnalyticsService analytics,
            IEventBus eventBus)
        {
            _inventory = inventory;
            _wallet = wallet;
            _content = content;
            _analytics = analytics;
            _eventBus = eventBus;
        }
        
        public async Task<Result<Item>> ExecuteAsync(string itemId)
        {
            // 1. Get item definition (через Addressables)
            var itemDef = await _inventory.GetItemDefinitionAsync(itemId);
            
            if (itemDef == null)
            {
                return Result<Item>.Failure("item_not_found");
            }
            
            // 2. Check download size (для remote items)
            string assetKey = $"Item_{itemId}";
            var downloadSize = await _content.GetDownloadSizeAsync(assetKey);
            
            if (downloadSize > 0)
            {
                // Требуется скачивание (например, premium скин)
                // Publish event для UI
                _eventBus.Publish(new DownloadRequiredEvent(itemId, downloadSize));
                
                // Ждём подтверждения от UI (через другой UseCase)
                return Result<Item>.Failure("download_required");
            }
            
            // 3. Check funds
            if (!_wallet.CanAfford(itemDef.Price))
            {
                return Result<Item>.Failure("insufficient_funds");
            }
            
            // 4. Process purchase
            _wallet.Spend(itemDef.Price, CurrencyType.Coins);
            
            var item = itemDef.CreateInstance();
            await _inventory.AddItemAsync(item);
            
            // 5. Analytics
            _analytics.TrackEvent("item_purchased", new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "price", itemDef.Price },
                { "was_remote", downloadSize > 0 }
            });
            
            // 6. Event
            _eventBus.Publish(new ItemPurchasedEvent(item));
            
            return Result<Item>.Success(item);
        }
    }
}
```

---

## Код: ShopViewModel (Presentation)

```csharp
namespace Core.Presentation.ViewModels
{
    public class ShopViewModel
    {
        private readonly PurchaseItemUseCase _purchaseUseCase;
        private readonly IEventBus _eventBus;
        private readonly LocalizationService _localization;
        private readonly IContentRepository _content;
        
        public ShopViewModel(
            PurchaseItemUseCase purchaseUseCase,
            IEventBus eventBus,
            LocalizationService localization,
            IContentRepository content)
        {
            _purchaseUseCase = purchaseUseCase;
            _eventBus = eventBus;
            _localization = localization;
            _content = content;
            
            // Подписка на события
            _eventBus.Subscribe<DownloadRequiredEvent>(OnDownloadRequired);
        }
        
        public async void OnItemClicked(string itemId)
        {
            var result = await _purchaseUseCase.ExecuteAsync(itemId);
            
            if (result.IsSuccess)
            {
                ShowSuccessPopup(result.Value);
            }
            else if (result.Error == "download_required")
            {
                // UI покажет download prompt через event
            }
            else
            {
                ShowErrorPopup(result.Error);
            }
        }
        
        private async void OnDownloadRequired(DownloadRequiredEvent evt)
        {
            // Показать UI download prompt
            string title = _localization.GetString("Shop", "download_required_title");
            string message = _localization.GetString("Shop", "download_required_message");
            
            float sizeMB = evt.DownloadSizeBytes / (1024f * 1024f);
            
            bool userConfirmed = await ShowDownloadPromptAsync(title, message, sizeMB);
            
            if (userConfirmed)
            {
                // Скачать контент
                await _content.DownloadAsync(
                    $"Item_{evt.ItemId}",
                    new Progress<float>(OnDownloadProgress)
                );
                
                // Повторить покупку
                OnItemClicked(evt.ItemId);
            }
        }
        
        private void OnDownloadProgress(float progress)
        {
            // Update UI progress bar
        }
        
        private void ShowSuccessPopup(Item item)
        {
            string message = _localization.GetString("Shop", "purchase_success");
            // Show UI
        }
        
        private void ShowErrorPopup(string errorKey)
        {
            string message = _localization.GetString("Errors", errorKey);
            // Show UI
        }
    }
}
```

---

## Код: BattleFeature (с Addressables)

### BattleInstaller

```csharp
public class BattleInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Domain
        builder.Register<StartBattleUseCase>(Lifetime.Transient);
        builder.Register<ExecuteSkillUseCase>(Lifetime.Transient);
        
        // Application
        builder.Register<BattleService>(Lifetime.Singleton);
        builder.Register<BattleContentLoader>(Lifetime.Singleton);
        
        // Infrastructure
        builder.Register<IBattleRepository, BattleRepository>(Lifetime.Singleton);
        
        // Presentation
        builder.Register<BattleViewModel>(Lifetime.Transient);
    }
}
```

### BattleContentLoader

```csharp
public class BattleContentLoader
{
    private readonly IContentRepository _content;
    private readonly Dictionary<string, GameObject> _loadedArenas = new();
    
    public BattleContentLoader(IContentRepository content)
    {
        _content = content;
    }
    
    /// <summary>
    /// Загрузка арены для битвы
    /// </summary>
    public async Task<GameObject> LoadArenaAsync(string arenaId)
    {
        if (_loadedArenas.TryGetValue(arenaId, out var cached))
        {
            return cached;
        }
        
        string key = $"Arena_{arenaId}";
        
        // Проверка remote content
        var downloadSize = await _content.GetDownloadSizeAsync(key);
        
        if (downloadSize > 0)
        {
            // Download с progress
            await _content.DownloadAsync(key, new Progress<float>(p => 
            {
                UnityEngine.Debug.Log($"Downloading arena: {p * 100}%");
            }));
        }
        
        // Загрузка и инстанцирование
        var arena = await _content.InstantiateAsync(key);
        _loadedArenas[arenaId] = arena;
        
        return arena;
    }
    
    /// <summary>
    /// Загрузка character model
    /// </summary>
    public async Task<GameObject> LoadCharacterAsync(string characterId)
    {
        string key = $"Character_{characterId}";
        return await _content.LoadAssetAsync<GameObject>(key);
    }
    
    /// <summary>
    /// Выгрузка арены
    /// </summary>
    public void UnloadArena(string arenaId)
    {
        if (_loadedArenas.TryGetValue(arenaId, out var arena))
        {
            UnityEngine.Object.Destroy(arena);
            _loadedArenas.Remove(arenaId);
            
            _content.Unload($"Arena_{arenaId}");
        }
    }
}
```

---

## Localization в Clean Architecture

### String Tables Structure

```
Table: UI (общий UI)
- main_menu_title
- settings_button
- back_button

Table: Shop (магазин)
- purchase_success
- insufficient_funds
- download_required_title

Table: Battle (битва)
- victory
- defeat
- skill_used

Table: Inventory (инвентарь)
- equip_success
- item_equipped

Table: Errors (ошибки)
- item_not_found
- network_error
- download_failed
```

### Asset Tables (локализованные ассеты)

```
Table: Character_Icons
- hero_warrior (Sprite для разных языков)
- hero_mage
- hero_archer

Table: Item_Icons
- sword_legendary
- armor_epic
- potion_health

Table: UI_Images
- flag_en, flag_ru, flag_zh
- icon_coins, icon_gems
```

---

## ProjectInstaller (Bootstrap)

```csharp
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectInstaller : LifetimeScope
{
    [SerializeField] private ContentConfig _contentConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Content & Localization (базовые сервисы)
        builder.RegisterInstance(_contentConfig);
        builder.Register<ContentService>(Lifetime.Singleton);
        builder.Register<LocalizationService>(Lifetime.Singleton);
        
        // Infrastructure
        builder.Register<AppStateMachine>(Lifetime.Singleton);
        builder.Register<SceneLoader>(Lifetime.Singleton);
        
        // Persistence
        builder.Register<ISaveSystem, SaveSystem>(Lifetime.Singleton);
        builder.Register<IBackendClient, BackendClient>(Lifetime.Singleton);
        
        // Repositories (реализация через Addressables)
        builder.Register<IContentRepository, ContentRepository>(Lifetime.Singleton);
        builder.Register<IInventoryRepository, InventoryRepository>(Lifetime.Singleton);
        builder.Register<IWalletRepository, WalletRepository>(Lifetime.Singleton);
        
        // Application
        builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
        
        // Domain UseCases
        builder.Register<PurchaseItemUseCase>(Lifetime.Transient);
        builder.Register<EquipItemUseCase>(Lifetime.Transient);
        builder.Register<StartBattleUseCase>(Lifetime.Transient);
        
        // SDK
        builder.Register<IAnalyticsService, AnalyticsService>(Lifetime.Singleton);
        builder.Register<IAdsService, AdsService>(Lifetime.Singleton);
        
        // Entry points
        builder.RegisterEntryPoint<ContentBootstrap>();
        builder.RegisterEntryPoint<AppEntryPoint>();
    }
}
```

---

## Testing Strategy (с Addressables Mock)

### Unit Tests (Domain)

```csharp
[Test]
public async Task PurchaseItem_WithRemoteContent_ShouldCheckDownloadSize()
{
    // Arrange
    var contentMock = new Mock<IContentRepository>();
    var inventoryMock = new Mock<IInventoryRepository>();
    var walletMock = new Mock<IWalletRepository>();
    
    // Mock: item требует скачивания 5MB
    contentMock.Setup(c => c.GetDownloadSizeAsync("Item_premium_sword"))
        .ReturnsAsync(5 * 1024 * 1024);
    
    walletMock.Setup(w => w.CanAfford(It.IsAny<int>())).Returns(true);
    
    var useCase = new PurchaseItemUseCase(
        inventoryMock.Object,
        walletMock.Object,
        contentMock.Object,
        Mock.Of<IAnalyticsService>(),
        Mock.Of<IEventBus>()
    );
    
    // Act
    var result = await useCase.ExecuteAsync("premium_sword");
    
    // Assert
    Assert.IsFalse(result.IsSuccess);
    Assert.AreEqual("download_required", result.Error);
    
    contentMock.Verify(c => c.GetDownloadSizeAsync("Item_premium_sword"), Times.Once);
}
```

### Integration Tests (с реальным Addressables)

```csharp
[UnityTest]
public IEnumerator ContentRepository_LoadItemDefinition_FromAddressables()
{
    // Arrange
    var contentService = new ContentService(_config);
    var repo = new ContentRepository(contentService);
    
    // Act
    var task = repo.LoadAssetAsync<ItemDefinition>("ItemDef_sword_basic");
    
    yield return new WaitUntil(() => task.IsCompleted);
    
    // Assert
    Assert.IsNotNull(task.Result);
    Assert.AreEqual("sword_basic", task.Result.Id);
}
```

---

## Performance Best Practices

### 1. Batch Load Definitions

```csharp
public async Task PreloadItemDefinitionsAsync(IEnumerable<string> itemIds)
{
    var tasks = itemIds.Select(id => 
        _content.LoadAssetAsync<ItemDefinition>($"ItemDef_{id}")
    );
    
    await Task.WhenAll(tasks);
}
```

### 2. Unload по завершении битвы

```csharp
public class BattleCleanupService
{
    public void CleanupAfterBattle(string arenaId, List<string> characterIds)
    {
        // Unload arena
        _content.Unload($"Arena_{arenaId}");
        
        // Unload characters
        foreach (var charId in characterIds)
        {
            _content.Unload($"Character_{charId}");
        }
    }
}
```

### 3. Preload Next Battle

```csharp
// В конце текущей битвы
await _battleContent.PreloadNextBattleAsync(nextBattleId);
```

---

## Bundle Size Targets

```
ItemDefinitions (Local): 5MB
Character_Starter (Local): 20MB
Character_Premium (Remote): 50MB per character

Arena_Basic (Local): 30MB
Arena_Advanced (Remote): 50MB per arena

UI (Local): 15MB total
Localization (Local top 3): 3MB, (Remote others): 1MB each
```

---

## Migration Path

### From Modular → Clean Architecture

```
1. Создать Domain слой с UseCases
2. Создать IContentRepository interface
3. Реализовать ContentRepository через ContentService
4. Перенести бизнес-логику в UseCases
5. Обновить ViewModels для использования UseCases

ContentService, LocalizationService: НЕ МЕНЯЮТСЯ
Addressables Groups: НЕ МЕНЯЮТСЯ
```

### To LiveOps Platform

```
1. Добавить EventSystem, SeasonSystem (использующие IContentRepository)
2. Создать per-event Addressable groups
3. Добавить dynamic content loading

Вся Clean Architecture остаётся: НЕ МЕНЯЕТСЯ
```

---

## Когда использовать

### ✅ ДА:
- Midcore+ игры (RPG, стратегии, коллекционки)
- Сложная бизнес-логика
- Большая команда (5-10+ человек)
- Долгосрочный проект (1+ год)
- Backend-зависимость
- Высокие требования к тестируемости
- **Remote контент как часть игровой экономики**

### ❌ НЕТ:
- Hypercasual / простые игры (используй Single-Scene)
- Прототипы (используй Single-Scene)
- Маленькая команда < 3 (используй Modular)

---

## Итого

**Clean Architecture + Addressables даёт:**
- Полная изоляция Domain от инфраструктуры
- IContentRepository абстрагирует Addressables
- UseCases автоматически работают с remote контентом
- Локализация интегрирована через весь стек
- Тестируемость через моки IContentRepository
- Масштабируемость до LiveOps без изменения Domain
- Один и тот же ContentService для всех слоёв