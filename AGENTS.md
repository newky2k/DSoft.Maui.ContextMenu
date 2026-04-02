# DSoft.Maui.ContextMenu — Agent Guide

## Project Overview

A .NET MAUI library that adds platform-native context menus (long-press or click) to any control on Android and iOS. Forked and updated from **The49.Maui.ContextMenu** to target .NET 9.0 with added click-event support.

- **Namespace:** `The49.Maui.ContextMenu` (retained from original library)
- **XML namespace:** `https://schemas.the49.com/dotnet/2023/maui`
- **NuGet ID:** `DSoft.Maui.ContextMenu`

---

## Repository Layout

```
DSoft.Maui.ContextMenu/
├── src/                          # Library source (primary working dir)
│   ├── AttachedProperties/       # Cross-platform + platform-specific attached props
│   ├── Models/                   # Menu, Action, Group, Preview, MenuElement
│   ├── Platforms/Android/        # Popup, window, animations, utilities
│   ├── Platforms/iOS/            # CollectionView controller + delegator
│   ├── Handlers/                 # CollectionViewHandler override
│   ├── Extensions/               # ResourcesExtension
│   ├── Hosting/                  # MauiAppBuilderExtensions (UseContextMenu())
│   └── DSoft.Maui.ContextMenu.csproj
├── sample/                       # Manual test app (no automated tests)
├── Directory.Build.props         # Shared build properties (signing, SourceLink, NuGet)
├── azure-pipelines-release.yml   # Release CI — triggers on master
├── azure-pipelines-mergetest.yml # Merge validation CI — manual trigger
└── DSoft.Maui.ContextMenu.sln
```

---

## Technology Stack

| Concern | Detail |
|---|---|
| Runtime | .NET 9.0 |
| Framework | Microsoft.Maui.Controls 9.0.110 |
| Target platforms | `net9.0-android` (API 33+), `net9.0-ios` (15+), `net9.0-maccatalyst` (15+), `net9.0-windows10.0.19041.0` |
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
dotnet build src/DSoft.Maui.ContextMenu.csproj -c Release
```
Workloads required: `android ios maccatalyst maui`

### CI pipelines (Azure DevOps, `windows-latest`)
- **Release** (`azure-pipelines-release.yml`): triggers on `master`; versioned NuGet package artifact
- **Merge test** (`azure-pipelines-mergetest.yml`): manual trigger; Debug build for validation
- Version scheme: `1.0.{YYMM}.{DD}{rev:r}`

### NuGet packaging
`GeneratePackageOnBuild` is enabled — a `.nupkg` is produced on every Release build. Package metadata lives in `Directory.Build.props` and the `.csproj`.

---

## Testing

There are **no automated tests**. Validation is manual via the sample app:

```
sample/The49.Maui.ContextMenu.Sample/Pages/
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
