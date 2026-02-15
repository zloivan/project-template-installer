# Template Installer - Quick Start Guide

## How to Use (3 Simple Steps)

### Step 1: Install Required Packages
When you first import the Template Installer, it will automatically prompt you to install:
- ✅ Addressables (latest compatible version)
- ✅ Localization (latest compatible version)

Click **"Install"** when prompted, then wait for:
- Progress bar in bottom-right corner to complete
- Console message: "All required packages installed successfully!"
- No errors in Package Manager window

**Time:** ~1-2 minutes

**Note:** VContainer is already in your manifest.json and will be installed automatically.

---

### Step 2: Open Template Installer
```
Tools → Project Template Installer
```

---

### Step 3: Install Your Template

#### For Beginners / Prototypes:
1. Select: **Single-Scene Prototype**
2. Root Namespace: `MyGame`
3. Enable: "Generate Sample Levels"
4. Click: **Install Template**

#### For Production Games:
1. Select: **Multi-Scene Modular** or **Clean Architecture**
2. Root Namespace: Your game name (e.g., `ZombieShooter`)
3. Enable: "Generate Sample Levels"
4. Click: **Install Template**

---

## What Gets Created

### Single-Scene Template
```
Assets/_Project/
├── Bootstrap/
├── Core/
│   ├── Content/          ← ContentService
│   ├── Localization/     ← LocalizationService
│   └── StateMachine/     ← Game state management
├── Game/
│   ├── Levels/
│   └── UI/
└── Scenes/
    └── Game.unity        ← Main scene
```

### Modular Template
```
Assets/_Project/
├── 00_Bootstrap/
│   └── Bootstrap.unity
├── 01_Core/
│   ├── Content/
│   ├── Localization/
│   └── Scenes/
│       └── Persistent.unity
├── 02_Features/
│   ├── Meta/
│   ├── Gameplay/
│   └── Scenes/
│       ├── Shell.unity
│       └── Gameplay.unity
├── 03_Content/
└── 04_SDK/
```

---

## After Installation

### 1. Wait for Compilation
Unity will compile the generated scripts. This takes ~10-30 seconds.

### 2. Check What Was Created
- ✅ Folder structure in `Assets/_Project/`
- ✅ Scenes in Build Settings
- ✅ Addressables groups configured
- ✅ Localization tables created (English + Russian)

### 3. Start Building Your Game!

#### Load Content (with VContainer):
```csharp
using VContainer;

public class MyGameplay : MonoBehaviour
{
    [Inject] private ContentService _contentService;

    private async void Start()
    {
        var prefab = await _contentService.LoadAsync<GameObject>("MyPrefab");
    }
}
```

#### Use Localization (with VContainer):
```csharp
using VContainer;

public class MyUI : MonoBehaviour
{
    [Inject] private LocalizationService _localizationService;

    private void Start()
    {
        string text = _localizationService.GetString("UI", "play_button");
    }
}
```

---

## Common Questions

### Q: Can I install multiple templates?
**A:** No, choose one template per project. Templates are designed to be mutually exclusive.

### Q: What if I already have a _Project folder?
**A:** The installer won't overwrite existing files. You can safely run it, or delete the folder first for a clean install.

### Q: Can I customize the templates?
**A:** Yes! Run `Tools → Template Installer → Create Template Definitions` to create ScriptableObject assets you can modify.

### Q: Do I need VContainer?
**A:** Yes! VContainer is now automatically installed and integrated. All generated code uses VContainer for dependency injection out of the box.

---

## Troubleshooting

### "Package dependencies not installed"
- Wait for Unity to finish resolving packages
- Check Package Manager for errors
- Restart Unity if needed

### "Template validation failed"
- This should be fixed now
- If it persists, check Console for detailed errors

### "Addressables groups not created"
- Open: `Window → Asset Management → Addressables → Groups`
- Click: "Create Addressables Settings"
- Re-run template installer

---

## Next Steps

1. ✅ Install your chosen template
2. 📖 Read the generated README in `Assets/_Project/`
3. 🎮 Start building your game!
4. 📚 Check architecture docs in `Assets/TempalateInstaller/` for best practices

---

**Happy Game Development! 🎮**
