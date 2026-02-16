# Package Setup Guide

## ✅ Embedded Package Configuration

The Template Installer is now configured as an **embedded package** in your project.

### What This Means:

1. **Source Location:** `Assets/TempalateInstaller/`
2. **Package Reference:** Added to `Packages/manifest.json` as:
   ```json
   "com.ikhom.project-template-installer": "file:../Assets/TempalateInstaller"
   ```
3. **Editable:** You can edit files directly in `Assets/TempalateInstaller/`
4. **Version Control:** The package is part of your project and can be committed to Git

---

## 🔧 After Adding to manifest.json

Unity will now:
1. Recognize the package properly
2. Stop showing "immutable package" warnings
3. Allow you to edit files without errors
4. Properly resolve package dependencies

**Next Steps:**
1. **Restart Unity** (or wait for Unity to refresh)
2. Check that no errors appear in Console
3. The package should now work correctly

---

## 📦 Package Structure

```
Assets/TempalateInstaller/
├── Editor/                    # Editor scripts
│   ├── PackageAutoInstaller.cs
│   ├── ScriptingDefineManager.cs
│   ├── AddressablesConfigurator.cs
│   └── ...
├── Runtime/                   # Runtime scripts
│   └── InstallCore/
├── package.json              # Package manifest
├── README.md                 # Main documentation
├── QUICK_START.md           # Quick start guide
├── CHANGELOG.md             # Version history
└── ...
```

---

## 🚀 How to Use

### First Time Setup:
1. Unity will prompt to install Addressables & Localization
2. Click "Install" and wait for packages to install
3. Unity will recompile (adds `ADDRESSABLES_INSTALLED` define)
4. Open `Tools → Project Template Installer`

### Installing a Template:
1. Choose your template (Single-Scene, Modular, or Clean Architecture)
2. Configure options
3. Click "Install Template"
4. Wait for installation to complete

---

## 🔄 Updating the Package

Since this is an embedded package, you can:

1. **Edit files directly** in `Assets/TempalateInstaller/`
2. **Commit changes** to version control
3. **Share with team** - they'll get the same version

No need to reinstall or update via Package Manager!

---

## ⚠️ Important Notes

### Don't Move the Folder
The package path in `manifest.json` is relative:
```json
"file:../Assets/TempalateInstaller"
```

If you move the folder, update the path in `manifest.json`.

### Package Cache
Unity may create a cached copy in `Library/PackageCache/`. This is normal - always edit files in `Assets/TempalateInstaller/`, not the cache.

### Removing the Package
To remove the Template Installer:
1. Delete `Assets/TempalateInstaller/` folder
2. Remove the line from `Packages/manifest.json`:
   ```json
   "com.ikhom.project-template-installer": "file:../Assets/TempalateInstaller",
   ```

---

## 📚 Documentation

- **Quick Start:** `QUICK_START.md`
- **Architecture Guide:** `ARCHITECTURE.md`
- **Changelog:** `CHANGELOG.md`
- **Main README:** `README.md`

---

## 🐛 Troubleshooting

### "Immutable package" warnings
**Solution:** The package is now in `manifest.json`. Restart Unity to clear warnings.

### Compilation errors about Addressables
**Solution:** Install packages via `Tools → Template Installer → Install Required Packages`

### Dialog shows in infinite loop
**Solution:** Fixed in v1.0.1 - dialog only shows once per session

### Package not found in manifest
**Solution:** Already fixed - package is now in `manifest.json`

---

**Template Installer v1.0.1** - Embedded Package Setup Complete! ✅
