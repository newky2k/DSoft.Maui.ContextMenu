# DSoft.Maui.ContextMenu — Agent Guide

## Project Overview

A .NET MAUI library that adds platform-native context menus (long-press or click) to any control on Android and iOS. Forked and updated from **The49.Maui.ContextMenu** to target .NET 10 and .NET 11 with added click-event support.

- **Namespace:** `The49.Maui.ContextMenu` (retained from original library)
- **XML namespace:** `https://schemas.the49.com/dotnet/2023/maui`
- **NuGet ID:** `DSoft.Maui.ContextMenu`

---

## Branching

- `development` is the working branch. **Every feature and bug fix goes to `development`**: branch from it and open pull requests into it. Never commit or open a pull request directly against `main`.
- `main` is the release branch. Every push to `main` runs `release.yml`, which publishes the package to nuget.org and creates a GitHub release, so only merge `development` into `main` when a release is intended, and only when asked to release.

---

## Repository Layout

```
DSoft.Maui.ContextMenu/
├── DSoft.Maui.ContextMenu/       # Library source (primary working dir)
│   ├── AttachedProperties/       # Cross-platform + platform-specific attached props
│   ├── Models/                   # Menu, Action, Group, Preview, MenuElement
│   ├── Platforms/Android/        # Popup, window, animations, utilities
│   ├── Platforms/iOS/            # CollectionView controller + delegator
│   ├── Handlers/                 # CollectionViewHandler override
│   ├── Extensions/               # ResourcesExtension
│   ├── Hosting/                  # MauiAppBuilderExtensions (UseContextMenu())
│   └── DSoft.Maui.ContextMenu.csproj
├── DSoft.Maui.ContextMenu.Sample/ # Manual test app (no automated tests)
├── Directory.Build.props         # Shared build properties (signing, SourceLink, NuGet)
├── .github/workflows/            # ci.yml and release.yml (GitHub Actions)
└── DSoft.Maui.ContextMenu.slnx
```

---

## Technology Stack

| Concern | Detail |
|---|---|
| Runtime | .NET 10 and .NET 11 |
| Framework | Microsoft.Maui.Controls 10.0.130 for net10.0, 11.0.10 for net11.0 |
| Target platforms | `net10.0` and `net11.0` for `-android` (API 33+), `-ios` (16+), `-maccatalyst` (16+) and `-windows10.0.19041.0` |
| Key dependencies | `CommunityToolkit.Mvvm 8.4.0`, `M.BindableProperty.Generator 0.11.1` |
| C# version | Latest |
| Nullable reference types | Enabled |
| Implicit usings | Enabled |
| Assembly signing | `DSoft.snk` strong-name key |

---

## Architecture

### Attached Properties pattern
All public API surfaces through attached properties on `VisualElement` defined in `AttachedProperties/ContextMenu.cs`:

- `ContextMenu.Menu` — `DataTemplate` that returns the menu definition
- `ContextMenu.Preview` — `DataTemplate` / `Preview` object for preview customisation
- `ContextMenu.ClickCommand` / `ClickCommandParameter` — standard tap command
- `ContextMenu.ShowMenuOnClick` — show menu on click instead of long-press

Platform setup/teardown is handled via **partial methods**:
- `ContextMenu.Android.cs` — registers touch/long-press listeners
- `ContextMenu.iOS.cs` — registers `UIContextMenuInteraction`

### Menu model hierarchy
```
MenuElement (abstract, has Title)
├── Menu       (has Children: ObservableCollection<MenuElement>, propagates BindingContext)
├── Group      (has Children, rendered as a section separator)
└── Action     (Command, CommandParameter, Icon, SystemIcon, IsEnabled, IsDestructive, SubTitle, Clicked event)
```

### Platform implementations
**Android:**
- `ContextMenuWindow` — snapshot preview + fade/scale animations via `WindowManager`
- `ContextMenuPopup` — `PopupWindow` + `RecyclerView` menu with submenu navigation, dark/light theming

**iOS:**
- `UIContextMenuInteraction` API; menus converted to `UIMenu`/`UIAction` hierarchy
- `CollectionViewDelegator` handles per-item context menus in `CollectionView`

### CollectionView support
`CollectionViewHandler` wraps the iOS handler to inject a custom `CollectionViewController` and `CollectionViewDelegator` for per-item context menus with data binding.

---

## Build & CI/CD

### Local build
```bash
dotnet build DSoft.Maui.ContextMenu.slnx -c Release
```
Install the .NET 10 and .NET 11 SDKs, then run `dotnet workload restore DSoft.Maui.ContextMenu.slnx`. `global.json` rolls forward to the latest installed SDK so the .NET 11 targets build.

### CI (GitHub Actions, `.github/workflows/`, `windows-latest`)
- `ci.yml`: runs on pull requests into `main` and `development`. Restores workloads, then restores and builds Release. Publishes nothing.
- `release.yml`: runs on every push to `main` (changes only to Markdown or workflow files are skipped; run it by hand to test a workflow change). Builds Release as `1.1.<yyMM>.<run number>` plus `RELEASE_SUFFIX` (empty for a stable version), uploads the package as the `drop` artifact, pushes it to nuget.org with Trusted Publishing (OIDC, `NUGET_USER` secret, `nuget` environment), then tags the commit `v<version>` and creates a GitHub release with the package attached.

### NuGet packaging
`GeneratePackageOnBuild` is enabled — a `.nupkg` is produced on every Release build. Package metadata lives in `Directory.Build.props` and the `.csproj`.

---

## Testing

There are **no automated tests**. Validation is manual via the sample app:

```
DSoft.Maui.ContextMenu.Sample/Pages/
├── Simple.xaml            — basic long-press menu
├── CollectionView.xaml    — per-item context menus
├── GroupsPage.xaml        — group separators
├── IconsPage.xaml         — icon / systemIcon usage
├── PreviewPage.xaml       — custom preview templates
├── ShowMenuOnClick.xaml   — click-activated menus
└── ClickCommandPage.xaml  — ClickCommand usage
```

When making changes, manually verify on both Android and iOS using the sample app.

---

## Code Conventions

- **Namespaces:** file-scoped, all under `The49.Maui.ContextMenu[.*]`
- **Platform guards:** `#if ANDROID` / `#if IOS` or separate `*.Android.cs` / `*.iOS.cs` partial files
- **Android interop:** alias `AView = Android.Views.View`; Java listener classes inherit `Java.Lang.Object`
- **Bindable properties:** generated via `M.BindableProperty.Generator` attributes — do not hand-write `BindableProperty` boilerplate
- **Private fields:** `_camelCase`
- **Resource keys (Android):** `SCREAMING_SNAKE_CASE`
- **Suppressed warnings:** CS1591 (missing XML docs on public members), CS8600–CS8765 (nullable)

---

## Key Extension Point

To initialise the library in a host app:

```csharp
builder.UseContextMenu();
```

This registers the `CollectionViewHandler` and sets up the MAUI XML namespace.
