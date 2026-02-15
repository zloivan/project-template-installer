# VContainer Installation - Important Note

## ✅ Fixed: VContainer Now Installs via Git URL

### The Issue
VContainer is **not available** in Unity's official Package Manager registry. It must be installed via:
- Git URL (recommended)
- OpenUPM
- Manual download

### The Solution
VContainer is now installed via Git URL, just like UniTask:

```json
{
  "dependencies": {
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
  }
}
```

This is the **official recommended method** from VContainer's documentation.

---

## How It Works

### In Your Project (manifest.json)
```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0",
    "com.unity.addressables": "1.22.3",
    "com.unity.localization": "1.4.5"
  }
}
```

### In Template Installer (TemplateDefinition.cs)
```csharp
[SerializeField] private List<string> requiredPackages = new List<string>
{
    "com.unity.addressables",
    "com.unity.localization",
    "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
};
```

The installer will automatically add VContainer via Git URL when installing packages.

---

## Why Git URL?

### Advantages ✅
1. **Official Method** - Recommended by VContainer maintainers
2. **Always Up-to-Date** - Can specify exact version via tag
3. **No Manual Steps** - Unity handles everything
4. **Same as UniTask** - Consistent with other Git packages
5. **Works Everywhere** - No registry configuration needed

### Comparison with Other Methods

| Method | Pros | Cons |
|--------|------|------|
| **Git URL** ✅ | Official, automatic, version control | Requires Git installed |
| OpenUPM | Easy updates | Requires registry configuration |
| Manual | No dependencies | Manual updates, no version control |

---

## Installation Process

### What Happens Automatically
1. Unity reads `manifest.json`
2. Detects Git URL for VContainer
3. Clones the repository
4. Extracts the package from the specified path
5. Installs version 1.15.4 (from tag)
6. Package appears in Package Manager

### Requirements
- ✅ Git must be installed on your system
- ✅ Internet connection
- ✅ Unity 2022.3 or newer

### Installation Time
- First time: ~1-2 minutes (downloads from GitHub)
- Subsequent: ~10-30 seconds (cached)

---

## Troubleshooting

### "Git is not installed"
**Problem:** Unity can't find Git

**Solution:**
1. Install Git: https://git-scm.com/downloads
2. Restart Unity
3. Unity will automatically detect Git

### "Failed to resolve packages"
**Problem:** Network or Git issues

**Solution:**
1. Check internet connection
2. Verify Git is installed: `git --version` in terminal
3. Try again - Unity will retry automatically
4. If persistent, restart Unity

### "Package not found"
**Problem:** Git URL is incorrect

**Solution:**
- The URL is correct and tested
- Check that you have the latest manifest.json
- Verify no typos in the URL

---

## Verification

### Check VContainer is Installed
1. Open `Window → Package Manager`
2. Select "Packages: In Project"
3. Look for "VContainer" in the list
4. Should show version 1.15.4

### Check in Code
```csharp
using VContainer;
using VContainer.Unity;

// If these imports work, VContainer is installed correctly
```

---

## Alternative Installation Methods

### Method 1: Git URL (Current - Recommended) ✅
```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
```

### Method 2: OpenUPM (Alternative)
If you prefer OpenUPM, add this to your `manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": ["jp.hadashikick.vcontainer"]
    }
  ],
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.17.0"
  }
}
```

Or use the CLI:
```bash
openupm add jp.hadashikick.vcontainer
```

### Method 3: Manual (Not Recommended)
1. Download from GitHub releases
2. Extract to `Assets/` or `Packages/`
3. Manual updates required

---

## Version Management

### Current Version
- **1.17.0** (Latest stable as of integration)

### Updating VContainer
To update to a newer version:

1. Find the version tag on GitHub: https://github.com/hadashiA/VContainer/tags
2. Update the URL in `manifest.json`:
   ```json
   "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.0"
   ```
3. Unity will automatically update

### Pinning to Specific Version
The `#1.17.0` at the end pins to that exact version:
- ✅ Ensures consistency across team
- ✅ Prevents unexpected breaking changes
- ✅ Can update when ready

---

## For Package Developers

### If You're Publishing This Package

**Important:** Don't include VContainer as a direct dependency in `package.json`:

```json
// ❌ DON'T DO THIS
{
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.15.4"  // Won't work!
  }
}
```

**Instead:** Document that users need to add VContainer via Git URL:

```json
// ✅ DO THIS
{
  "dependencies": {
    "com.unity.addressables": "1.21.0",
    "com.unity.localization": "1.4.0"
  }
}
```

And provide installation instructions in README.

---

## Summary

✅ **VContainer is now properly configured**
- Installs via Git URL (official method)
- Same approach as UniTask
- Automatic installation
- Version pinned to 1.17.0 (latest stable)
- No manual steps required

✅ **All templates work with VContainer out of the box**
- Services use dependency injection
- LifetimeScope installers generated
- Production-ready code

✅ **No action required from users**
- Unity handles everything
- Just wait for package installation
- Start using VContainer immediately

---

## Resources

- **VContainer GitHub:** https://github.com/hadashiA/VContainer
- **VContainer Docs:** https://vcontainer.hadashikick.jp/
- **Installation Guide:** https://vcontainer.hadashikick.jp/getting-started/installation
- **Git Installation:** https://git-scm.com/downloads

---

**VContainer is production-ready and properly configured! 🚀**
