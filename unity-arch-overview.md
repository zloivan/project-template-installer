# Unity Mobile Архитектуры - Общий Обзор

## Философия: Production-Ready с первого дня

**Ключевой принцип:** Прототипы не выбрасывают - их масштабируют. Правильная архитектура с дня 1 экономит месяцы рефакторинга.

### Обязательные компоненты (для ВСЕХ архитектур)
- ✅ **Addressables** вместо Resources.Load
- ✅ **Localization** встроена изначально
- ✅ **ContentService** - единый для всех
- ✅ **LocalizationService** - единый для всех
- ✅ **Remote Content Ready** - готовность к масштабированию

**Результат:** Один и тот же код работает от прототипа до LiveOps платформы.

---

## 4 основных production паттерна

### 1. Single-Scene Prototype (Hypercasual)
- **Применение**: Быстрое прототипирование, циклы 1-3 недели
- **Сцены**: 1 (Game.unity)
- **DI Scope**: Scene-level
- **Addressables**: ✅ С первого дня (Local groups)
- **Localization**: ✅ 2+ языка из коробки
- **Remote Content**: ⚠️ Группы готовы, но не используются
- **Пример**: Match-3, бегалки, казуальные головоломки

### 2. Multi-Scene Modular (Hybrid-Casual)
- **Применение**: Жизненный цикл 3-12 месяцев, A/B тесты
- **Сцены**: 4 (Bootstrap → Persistent → Shell → Gameplay)
- **DI Scope**: Project + Scene
- **Addressables**: ✅ Local + Remote groups
- **Localization**: ✅ 3-5 языков + Asset Tables
- **Remote Content**: ✅ Уровни 11+, DLC
- **Пример**: Merge игры, idle игры с метой

### 3. Clean Architecture (Midcore)
- **Применение**: 1+ год, сложная экономика, PvP
- **Сцены**: Как Modular
- **DI Scope**: Project + Feature scopes + Domain layer
- **Addressables**: ✅ 100% контента через IContentRepository
- **Localization**: ✅ 5-10 языков, dynamic tables
- **Remote Content**: ✅ Весь контент по требованию
- **Пример**: RPG, стратегии, коллекционки

### 4. Shell + Streaming (LiveOps)
- **Применение**: Games-as-service, еженедельные события
- **Сцены**: 1 Shell + динамический контент
- **DI Scope**: Project (Shell-only)
- **Addressables**: ✅ 100% контента remote, < 30MB в билде
- **Localization**: ✅ 10+ языков, regional content
- **Remote Content**: ✅ События, сезоны, моды без релизов
- **Пример**: Brawl Stars, Clash Royale

---

## Сравнительная таблица

| Критерий | Prototype | Modular | Clean Arch | LiveOps |
|----------|-----------|---------|------------|---------|
| **Сложность Bootstrap** | Минимальная | Средняя | Высокая | Очень высокая |
| **State Machine** | 3 состояния | 5 состояний | 8+ состояний | Иерархия |
| **Addressables Groups** | 4 Local | 4 Local + 2 Remote | 5 Local + 5 Remote | 2 Local + 20+ Remote |
| **Remote Config** | Опционально | Да | Да | Обязательно |
| **Feature Modules** | Нет | Да | Да (слои) | Да (стриминг) |
| **Localization** | 2 языка | 3-5 языков | 5-10 языков | 10+ языков |
| **Bundle в APK** | 50-100MB | 100-150MB | 150-200MB | < 50MB |
| **Размер команды** | 1-2 | 3-5 | 5-10 | 10+ |
| **Время разработки** | 1-3 недели | 3-12 месяцев | 1+ год | Постоянное |
| **ContentService** | ✅ Тот же | ✅ Тот же | ✅ Тот же | ✅ Тот же |
| **LocalizationService** | ✅ Тот же | ✅ Тот же | ✅ Тот же | ✅ Тот же |

---

## Универсальный стек инструментов

### Обязательные для ВСЕХ (с первого дня)
- **VContainer / Zenject**: DI framework
- **Addressables**: Content management (даже для прототипов)
- **Unity Localization Package**: Мультиязычность
- **UniTask**: Async/await (замена корутин)
- **ContentService**: Единая обёртка над Addressables
- **LocalizationService**: Единая обёртка над Localization

### Hybrid-Casual и выше
- **Remote Config**: A/B тесты, баланс
- **ScriptableObjects**: Конфигурация дизайна
- **Feature Flags**: Управление фичами

### Midcore и LiveOps
- **UniRx**: Реактивные потоки
- **Event Bus**: Decoupled messaging
- **Backend Client**: REST/WebSocket интеграция
- **Content Catalog Service**: Dynamic content discovery

---

## Универсальные ключевые классы

### Core Layer (одинаковые для всех)

#### ContentService
```csharp
// Единый для всех архитектур
public class ContentService
{
    public async UniTask<T> LoadAsync<T>(string key);
    public async UniTask<T> LoadWithDownloadAsync<T>(string key, IProgress<float> progress);
    public async UniTask<GameObject> InstantiateAsync(string key, Transform parent);
    public async UniTask<long> GetDownloadSizeAsync(string key);
    public void Unload(string key);
}
```

#### LocalizationService
```csharp
// Единый для всех архитектур
public class LocalizationService
{
    public async UniTask InitializeAsync();
    public async UniTask SetLocaleAsync(string localeCode);
    public string GetString(string tableName, string key);
    public async UniTask<T> GetAssetAsync<T>(string tableName, string key);
}
```

### Bootstrap Layer
- `ContentBootstrap` - Инициализация Content + Localization
- `ProjectInstaller` / `LifetimeScope` - Composition Root
- `AppEntryPoint` - Точка входа

### Services Layer
- `AnalyticsService` - Аналитика
- `AdsService` - Реклама
- `RemoteConfigService` - Удалённая конфигурация (Modular+)
- `SaveSystem` - Сохранения

### Content Layer (используют ContentService)
- `LevelLoader` - Загрузка уровней
- `UIManager` - Управление UI
- `AudioManager` - Управление аудио

---

## Addressables Groups (эволюция без переписывания)

### Day 1: Prototype
```
00_Static (Local)
  - GameConfig
  
01_Localization (Local)
  - StringTables_EN, RU

02_Levels_Local (Local)
  - Level_1 до Level_10

04_UI (Local)
  - Screens
```

### Week 4: Add Remote (без изменения кода)
```
+ 03_Levels_Remote (Remote)
  - Level_11+

Код: НЕ МЕНЯЕТСЯ
Просто добавили группу и перенесли ассеты
```

### Month 3: Modular + LiveOps
```
+ 05_Events_Remote
+ 06_Seasons_Remote

Добавили:
- EventSystem (использует ContentService)
- SeasonSystem (использует ContentService)

ContentService: НЕ МЕНЯЕТСЯ
```

### Month 6: Full LiveOps
```
+ 07_DLC_Remote
+ 08_Regional_CN

Добавили:
- RegionalContentService (обёртка над ContentService)
- DynamicLocalizationService (обёртка над LocalizationService)

Базовые сервисы: НЕ МЕНЯЮТСЯ
```

---

## Поток выполнения (универсальный)

```
1. App Launch
   ↓
2. Bootstrap Scene / Shell Scene
   ↓
3. ProjectContext.Awake() → Installers.Configure()
   Регистрирует:
   - ContentService (ОБЯЗАТЕЛЬНО)
   - LocalizationService (ОБЯЗАТЕЛЬНО)
   - AppStateMachine
   - SDK, Services
   ↓
4. AppEntryPoint.Start()
   ↓
5. ContentBootstrap (параллельно)
   - Check Addressables catalog updates
   - Initialize Localization
   - Preload core assets
   ↓
6. StateMachine.Enter(BootstrapState / MainMenuState)
   ↓
7. [Архитектурно-специфичные состояния]
   ↓
8. LoadContent via ContentService
   - Check if Local or Remote
   - Download if needed
   - Instantiate
   ↓
9. Gameplay / Meta loop
   ↓
10. Unload content via ContentService
```

**Ключ:** Шаги 1-5 идентичны для всех архитектур.

---

## Когда использовать какой подход

### Single-Scene Prototype
✅ Прототипы за 1-3 недели  
✅ Валидация игровой механики  
✅ Hypercasual жанры  
✅ Команда 1-2 человека  
✅ **С готовностью к масштабированию**  
❌ Нет долгосрочного плана (но готово к нему)

### Multi-Scene Modular
✅ Established жанры (merge, idle)  
✅ Умеренный LiveOps  
✅ A/B тестирование фич  
✅ Remote balance  
✅ Команда 3-5 человек  
✅ **Прямая эволюция из Prototype**  
❌ Очень сложная domain логика (нужна Clean Architecture)

### Clean Architecture
✅ Долгосрочные проекты (1+ год)  
✅ Большая команда (5-10+ человек)  
✅ Сложная бизнес-логика  
✅ Backend-зависимость  
✅ Высокие требования к тестируемости  
✅ **Прямая эволюция из Modular**  
❌ Hypercasual прототипы (overkill)

### Shell + Streaming
✅ Games-as-service  
✅ Сезонный контент  
✅ Частые обновления без релизов  
✅ Постоянная команда контента  
✅ Большой бюджет (CDN, DevOps)  
✅ **Прямая эволюция из Clean Architecture**  
❌ Офлайн игры

---

## Миграционный путь (без переписывания)

```
Prototype (Week 1-4)
  ↓ Добавить: Scenes, Remote Groups, RemoteConfig
Modular (Month 1-3)
  ↓ Добавить: Domain Layer, UseCases, IContentRepository
Clean Architecture (Month 3-6)
  ↓ Добавить: EventSystem, SeasonSystem, CDN
LiveOps Platform (Month 6+)
```

### Что НЕ меняется при миграции:
- ✅ ContentService
- ✅ LocalizationService
- ✅ Addressables Groups (только добавляем новые)
- ✅ ContentConfig
- ✅ Localization Tables
- ✅ UI Components (LocalizedText)

### Что добавляется:
- Week 4: Сцены, States, Remote Groups
- Month 3: Domain Layer, Interfaces, UseCases
- Month 6: Backend API, EventSystem, CDN

**Результат:** Код не переписывается, только расширяется.

---

## Различия между подходами

### Prototype Factory Setup
- **Цель**: Максимальная скорость валидации идеи
- **Сцены**: 1
- **Инсталлеры**: 1 (все в одном)
- **Addressables**: Local groups only
- **State Machine**: Простой (3-4 состояния)
- **SDK Integration**: Базовая (Ads + Analytics)
- **Localization**: 2 языка minimum
- **Migration Ready**: ✅ Да

### Modular Feature Setup
- **Цель**: Масштабируемость и feature isolation
- **Сцены**: 4 (Bootstrap, Persistent, Shell, Gameplay)
- **Инсталлеры**: По сцене + feature installers
- **Addressables**: Local + Remote groups
- **State Machine**: Средний (5-7 состояний)
- **SDK Integration**: Полная (Ads, IAP, Analytics, RemoteConfig)
- **Localization**: 3-5 языков + AssetTables
- **Migration Ready**: ✅ К Clean Architecture

### Clean Architecture Setup
- **Цель**: Testability, maintainability, separation of concerns
- **Сцены**: Как Modular
- **Инсталлеры**: По слоям (Domain, Application, Infrastructure)
- **Addressables**: 100% через IContentRepository
- **State Machine**: Сложный (8+ состояний, вложенные)
- **UseCase Pattern**: Вся бизнес-логика через UseCases
- **Localization**: 5-10 языков, dynamic loading
- **Migration Ready**: ✅ К LiveOps Platform

### LiveOps Platform Setup
- **Цель**: Continuous content delivery без релизов
- **Сцены**: 1 Shell + стриминговые
- **Инсталлеры**: Shell + динамические модули
- **Addressables**: 100% контента remote, < 30MB APK
- **State Machine**: Иерархический (events, seasons, modes)
- **Backend Dependency**: Критичная (всё из API)
- **Localization**: 10+ языков, regional content
- **Migration Ready**: ✅ Финальная форма

---

## Критерии выбора архитектуры

### По размеру команды
- **1-2 человека** → Prototype (но с Addressables!)
- **3-5 человек** → Modular
- **5-10 человек** → Clean Architecture
- **10+ человек** → LiveOps Platform

### По жизненному циклу
- **1-4 недели** → Prototype
- **3-12 месяцев** → Modular
- **1-3 года** → Clean Architecture
- **Indefinite** → LiveOps Platform

### По бюджету на разработку
- **< $50k** → Prototype
- **$50k-$500k** → Modular
- **$500k-$2M** → Clean Architecture
- **$2M+** → LiveOps Platform

### По сложности контента
- **Простой** (10-50 уровней) → Prototype
- **Средний** (100+ уровней, мета) → Modular
- **Сложный** (RPG системы, PvP) → Clean Architecture
- **Динамический** (сезоны, события) → LiveOps Platform

### По количеству языков
- **2-3 языка** → Prototype
- **3-5 языков** → Modular
- **5-10 языков** → Clean Architecture
- **10+ языков + региональный контент** → LiveOps Platform

---

## Content Pipeline (универсальный)

### Дизайнер создаёт контент
```
1. Create prefab/ScriptableObject
2. Mark as Addressable
3. Assign key: "Level_5" или "Item_sword"
4. Choose group: Local или Remote
```

### Build процесс
```
1. Window → Addressables → Build → Build Player Content
2. ServerData/ folder generated
3. Upload to CDN (если есть Remote groups)
4. Build APK/IPA
```

### Runtime загрузка
```csharp
// Одинаково для всех архитектур
var level = await _content.LoadAsync<GameObject>("Level_5");

// Или с автоматическим скачиванием
var level = await _content.LoadWithDownloadAsync<GameObject>(
    "Level_5",
    new Progress<float>(p => UpdateUI(p))
);
```

---

## Localization Pipeline (универсальный)

### Настройка языков
```
1. Window → Asset Management → Localization Tables
2. Create String Table Collection: "UI"
3. Add Locales: en, ru, es, zh, ja
4. Mark as Addressable
5. Assign to 01_Localization group
```

### Добавление переводов
```
Table: UI
Key               | EN             | RU            | ES
-----------------|----------------|---------------|----------------
play_button      | Play           | Играть        | Jugar
settings_button  | Settings       | Настройки     | Ajustes
level_complete   | Level Complete | Уровень пройден | Nivel Completado
```

### Runtime использование
```csharp
// Одинаково для всех архитектур
string text = _localization.GetString("UI", "play_button");

// Или на UI компоненте
[SerializeField] private string _tableName = "UI";
[SerializeField] private string _key = "play_button";

// LocalizedText автоматически обновляется при смене языка
```

---

## Remote Content Best Practices

### Разделение Local / Remote

**Local (в билде):**
- Первые 3-10 уровней
- Core UI
- Частые звуки
- Топ 3 языка локализации
- Tutorial

**Remote (CDN):**
- Остальные уровни
- DLC контент
- Музыка
- Дополнительные языки
- События / сезоны

### Download Strategy

```csharp
// Проверка размера перед загрузкой
var size = await _content.GetDownloadSizeAsync("Level_11");

if (size > 0)
{
    // Показать UI prompt
    bool confirmed = await ShowDownloadPrompt(size);
    
    if (confirmed)
    {
        // Загрузить с прогрессом
        await _content.DownloadAsync("Level_11", progressCallback);
    }
}
```

---

## Testing Strategy (универсальная)

### Unit Tests (ContentService)
```csharp
[Test]
public async Task ContentService_LoadAsset_FromAddressables()
{
    var service = new ContentService(_config);
    var asset = await service.LoadAsync<GameObject>("Level_1");
    
    Assert.IsNotNull(asset);
}
```

### Unit Tests (LocalizationService)
```csharp
[Test]
public async Task LocalizationService_GetString_ReturnsTranslation()
{
    var service = new LocalizationService();
    await service.InitializeAsync();
    
    var text = service.GetString("UI", "play_button");
    
    Assert.IsNotEmpty(text);
}
```

### Integration Tests
```
1. Bootstrap flow → Content initialization
2. Remote content download
3. Language switch → UI updates
4. Memory management → Unload verification
```

---

## Performance Metrics (целевые)

### Startup Time
- **Prototype**: < 2 секунды (Local content)
- **Modular**: < 3 секунды (Catalog check + Preload)
- **Clean Architecture**: < 4 секунды (Catalog + Backend sync)
- **LiveOps**: < 5 секунд (Catalog + Backend + Event check)

### Memory Usage
- **Prototype**: 100-150MB
- **Modular**: 150-200MB
- **Clean Architecture**: 200-300MB
- **LiveOps**: 150-250MB (minimal APK, dynamic content)

### Bundle Sizes
- **Prototype APK**: 50-100MB
- **Modular APK**: 100-150MB
- **Clean Architecture APK**: 150-200MB
- **LiveOps APK**: < 50MB (shell only)

---

## Реальные примеры студий

### Prototype → Product (без переписывания)
- **Voodoo**: Прототипы с Addressables → успешные выбираются → Remote content добавляется
- **Kwalee**: Hypercasual с локализацией из коробки → международный запуск без изменений

### Modular → LiveOps
- **Playrix** (Homescapes): Modular подход → постепенная эволюция в LiveOps
- **Supercell** (Hay Day): Feature modules → сезонный контент

### Clean Architecture
- **Riot Games** (TFT Mobile): Clean Architecture для сложной логики автобаттлера

### LiveOps Platform
- **Supercell** (Brawl Stars, Clash Royale): Минимальный APK, весь контент remote
- **Tencent** (PUBG Mobile): Сезоны, события без релизов

---

## Итоговые рекомендации

### Для новых проектов
1. **Всегда начинайте с Single-Scene Prototype**
2. **Обязательно используйте Addressables + Localization**
3. **Создайте ContentService и LocalizationService сразу**
4. **Настройте 2 языка minimum**
5. **Создайте Remote группы (даже если не используете)**

### При масштабировании
1. **Не переписывайте ContentService / LocalizationService**
2. **Добавляйте новые Addressables Groups**
3. **Расширяйте State Machine**
4. **Добавляйте Feature Modules**
5. **Интегрируйте RemoteConfig**

### При переходе на LiveOps
1. **ContentService остаётся тем же**
2. **LocalizationService остаётся тем же**
3. **Добавляйте EventSystem, SeasonSystem**
4. **Настраивайте CDN**
5. **Минимизируйте APK через Remote groups**

---

## Заключение

**Ключевая идея:** Одна и та же базовая архитектура (ContentService + LocalizationService + Addressables) работает от прототипа до LiveOps платформы.

**Результат:**
- ✅ Нет выброшенного кода
- ✅ Нет технического долга
- ✅ Постепенное масштабирование
- ✅ Production-quality с первого дня
- ✅ Готовность к международному рынку
- ✅ Готовность к remote контенту

**Время на миграцию между архитектурами: 1-2 недели, не месяцы переписывания.**