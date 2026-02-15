# Assembly Definition Fixes

## Проблема

При попытке использовать Addressables и Localization API в Editor коде возникали ошибки:
- `Cannot resolve symbol 'AddressableAssets'`
- `Cannot resolve symbol 'AddressableAssetSettings'`
- И другие связанные с Addressables/Localization

## Причина

**Assembly Definition Files (.asmdef) не содержали ссылок на необходимые Unity assemblies.**

Unity использует систему Assembly Definition для управления зависимостями между модулями. Чтобы использовать API из других пакетов (Addressables, Localization), необходимо явно указать эти зависимости в `.asmdef` файле.

## Исправления

### 1. Editor Assembly Definition
**Файл**: `Editor/IKhom.TemplateInstaller.Editor.asmdef`

**Было**:
```json
"references": [
    "IKhom.TemplateInstaller.Runtime"
]
```

**Стало**:
```json
"references": [
    "IKhom.TemplateInstaller.Runtime",
    "Unity.Addressables",
    "Unity.Addressables.Editor",
    "Unity.ResourceManager",
    "Unity.Localization",
    "Unity.Localization.Editor"
]
```

### 2. Runtime Assembly Definition
**Файл**: `Runtime/IKhom.TemplateInstaller.Runtime.asmdef`

**Было**:
```json
"references": []
```

**Стало**:
```json
"references": [
    "Unity.Addressables",
    "Unity.ResourceManager",
    "Unity.Localization"
]
```

**Почему?** Runtime assembly нужен для работы сгенерированного кода, который использует Addressables и Localization API.

### 3. Удалены Version Defines

**Было**:
```json
"versionDefines": [
    {
        "name": "com.unity.addressables",
        "expression": "",
        "define": "ADDRESSABLES_INSTALLED"
    },
    {
        "name": "com.unity.localization",
        "expression": "",
        "define": "LOCALIZATION_INSTALLED"
    }
]
```

**Стало**:
```json
"versionDefines": []
```

**Почему?** Conditional compilation defines больше не нужны, так как:
1. Пакеты теперь являются **обязательными зависимостями** через `package.json`
2. Assembly references обеспечивают compile-time проверку наличия API
3. Использование try-catch для runtime проверки более надёжно

### 4. Изменена Conditional Compilation

#### AddressablesConfigurator.cs
**Было**:
```csharp
#if UNITY_EDITOR && ADDRESSABLES_INSTALLED
```

**Стало**:
```csharp
#if UNITY_EDITOR
```

#### LocalizationConfigurator.cs
**Было**:
```csharp
#if UNITY_EDITOR && LOCALIZATION_INSTALLED
```

**Стало**:
```csharp
#if UNITY_EDITOR
```

#### TemplateInstaller.cs
**Было**:
```csharp
#if ADDRESSABLES_INSTALLED
    var configurator = new AddressablesConfigurator();
    // ...
#else
    Debug.LogWarning("Package not installed");
#endif
```

**Стало**:
```csharp
try
{
    var configurator = new AddressablesConfigurator();
    // ...
}
catch (System.Exception ex)
{
    Debug.LogWarning($"Failed: {ex.Message}");
}
```

## Преимущества Нового Подхода

### ✅ Compile-Time Safety
- Ошибки обнаруживаются на этапе компиляции
- IntelliSense/автодополнение работает корректно
- Рефакторинг безопасен

### ✅ Упрощённый Код
- Меньше `#if` директив
- Более чистый и читаемый код
- Проще поддерживать

### ✅ Явные Зависимости
- Dependency graph чёткий и понятный
- Unity автоматически управляет загрузкой assemblies
- Проще отлаживать проблемы зависимостей

### ✅ Runtime Обработка Ошибок
- Try-catch обрабатывает реальные runtime ошибки
- Более информативные сообщения об ошибках
- Graceful degradation вместо compile-time блоков

## Unity Assembly Names Reference

Для справки, правильные имена Unity assemblies:

### Addressables
- `Unity.Addressables` - Runtime API
- `Unity.Addressables.Editor` - Editor API
- `Unity.ResourceManager` - Resource management (используется Addressables)

### Localization
- `Unity.Localization` - Runtime API
- `Unity.Localization.Editor` - Editor API

### Стандартные Unity
- `UnityEngine` - Core engine (автоматически)
- `UnityEditor` - Editor API (для Editor platform)

## Проверка Корректности

После применения этих исправлений:

1. ✅ Все `Cannot resolve symbol` ошибки должны исчезнуть
2. ✅ IntelliSense работает для Addressables API
3. ✅ IntelliSense работает для Localization API
4. ✅ Код компилируется без ошибок
5. ✅ Editor scripts работают корректно

## Дополнительные Рекомендации

### Если пакеты не установлены
Пакеты **должны быть установлены**, так как они объявлены как dependencies в `package.json`:

```json
"dependencies": {
    "com.unity.addressables": "1.21.0",
    "com.unity.localization": "1.4.0"
}
```

Unity автоматически установит их при добавлении Template Installer пакета.

### Если нужна Optional Dependency
Если в будущем понадобится сделать пакет опциональным:

1. Убрать из `package.json` dependencies
2. Использовать `versionDefines` в `.asmdef`
3. Вернуть `#if` директивы в код
4. Добавить fallback логику

Но для Template Installer это **не рекомендуется**, так как Addressables и Localization - это core features пакета.

---

## Summary

**Основная идея**: Вместо runtime проверки наличия пакетов через defines, мы используем **explicit assembly references** для compile-time проверки и **try-catch** для runtime обработки ошибок.

Это делает код:
- Более надёжным
- Более понятным
- Более поддерживаемым
- Production-ready

---

**Дата исправлений**: 2025-02-16
**Версия**: 1.0.1
