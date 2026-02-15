# Package Signature Issues - Resolution Guide

## ⚠️ Issue: Invalid Package Signatures

You may see warnings about invalid signatures for:
- `com.unity.addressables` - Invalid signature
- `com.unity.localization` - Invalid signature

**This is SAFE and expected** when packages are installed from Git URLs instead of Unity's official Package Manager registry.

---

## 🔍 Why This Happens

Unity's Package Manager validates package signatures to ensure they come from trusted sources. When packages are installed via Git URLs (like VContainer, UniTask, etc.), they don't have Unity's official signature, triggering this warning.

**Important:** These packages are safe - they're from official Unity repositories and trusted community sources.

---

## ✅ Solution: Use Official Unity Registry Versions

The packages are already correctly installed in your `manifest.json`:

```json
{
  "dependencies": {
    "com.unity.addressables": "1.22.3",  // ✅ Official version
    "com.unity.localization": "1.4.5"     // ✅ Official version
  }
}
```

These versions are from Unity's official registry and **should not** show signature warnings.

---

## 🛠️ If Warnings Persist

### Option 1: Refresh Package Manager (Recommended)
1. Open Unity
2. Go to `Window → Package Manager`
3. Click the refresh icon (↻) in the top-right
4. Wait for packages to re-resolve
5. Restart Unity if needed

### Option 2: Clear Package Cache
```bash
# Close Unity first, then run:
rm -rf Library/PackageCache
rm Packages/packages-lock.json
# Reopen Unity - it will re-download all packages
```

### Option 3: Manual Verification
Check your `Packages/manifest.json` file. It should have:

```json
{
  "dependencies": {
    "com.unity.addressables": "1.22.3",
    "com.unity.localization": "1.4.5"
  }
}
```

**NOT** Git URLs like:
```json
// ❌ WRONG - causes signature warnings
"com.unity.addressables": "https://github.com/..."
```

---

## 🔐 Understanding Package Signatures

### Valid Signatures ✅
- Packages from Unity's official registry
- Installed via Package Manager UI
- Version numbers like `"1.22.3"`

### Invalid Signatures ⚠️ (but still safe)
- Packages from Git URLs
- Community packages (VContainer, UniTask, etc.)
- URLs like `"https://github.com/..."`

---

## 📦 Your Current Package Setup

Your project uses a mix of official and Git-based packages:

### Official Unity Packages (Signed) ✅
- `com.unity.addressables`: 1.22.3
- `com.unity.localization`: 1.4.5
- `com.unity.textmeshpro`: 3.0.7
- All other `com.unity.*` packages

### Community Packages (Git URLs - No Signature) ⚠️
- `jp.hadashikick.vcontainer`: Git URL (VContainer)
- `com.cysharp.unitask`: Git URL (UniTask)
- `com.eflatun.scenereference`: Git URL (SceneReference)

**This is the correct and recommended setup!**

---

## 🎯 What Changed in v1.0.1

1. **Removed package dependencies from installer's `package.json`**
   - The Template Installer no longer declares Addressables/Localization as dependencies
   - This prevents version conflicts and signature issues
   - Packages are installed directly to your project's `manifest.json`

2. **Unity 6 support added**
   - Fixed version validation to support Unity 6000.x (Unity 6)
   - Now supports: Unity 2022.3 LTS, 2023.x, and Unity 6+

3. **Improved package installation flow**
   - Packages are validated before template installation
   - Better error messages if packages are missing

---

## 🚀 Next Steps

1. **Ignore the signature warnings** - they're cosmetic and don't affect functionality
2. **Verify packages are installed**: Open `Window → Package Manager` and check:
   - ✅ Addressables 1.22.3 (or newer)
   - ✅ Localization 1.4.5 (or newer)
   - ✅ VContainer 1.17.0 (or newer)

3. **Install your template**: Go to `Tools → Project Template Installer`

---

## ❓ FAQ

### Q: Are these warnings dangerous?
**A:** No. They're just Unity's way of saying "this package didn't come from our official store." The packages are safe and widely used.

### Q: Can I remove the warnings?
**A:** Only by using official Unity registry packages. Your Addressables and Localization are already from the official registry, so warnings should not appear for them. If they do, try Option 1 above (refresh Package Manager).

### Q: Why does VContainer show a warning?
**A:** VContainer is installed from GitHub (not Unity's registry), so it won't have Unity's signature. This is normal and expected.

### Q: Should I remove packages with warnings?
**A:** **NO!** These packages are essential for the template installer and are completely safe.

---

## 📞 Still Having Issues?

If you continue to see signature warnings for `com.unity.addressables` or `com.unity.localization` after:
1. Refreshing Package Manager
2. Restarting Unity
3. Verifying `manifest.json` has version numbers (not Git URLs)

Then please:
1. Check Unity Console for specific error messages
2. Verify your Unity version is 2022.3 LTS or newer
3. Try clearing the package cache (Option 2 above)

---

**Remember:** Package signature warnings for Git-based packages (VContainer, UniTask) are **normal and safe**. Only Addressables and Localization should be from the official registry.
