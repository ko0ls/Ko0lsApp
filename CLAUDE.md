# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an **AutoCAD 2023/2024 plugin** written in C#, using a dual-target framework approach:
- Production: **.NET Framework 4.8** (runs inside AutoCAD)
- Tests: **.NET 8.0**

## Build Commands

```bash
dotnet restore
dotnet build                      # builds all configs
dotnet build -c "Debug 2023"      # target AutoCAD 2023
dotnet build -c "Debug 2024"      # target AutoCAD 2024
dotnet build -c "Release 2023"
dotnet build -c "Release 2024"
dotnet test                       # runs unit tests (targets net8.0)
dotnet clean
```

Output path: `bin\$(ConfigMode)\$(AutodeskVersion)\` (e.g., `bin\Debug\2023\`)

## Solution Architecture

```
Ko0lsApp.sln
├── src/
│   ├── AutoCADTools.Core          <- Domain models & interfaces
│   ├── AutoCADTools.Service      <- Business logic (depends on Core)
│   ├── AutoCADTools.Presentation <- WPF UI, MVVM base classes, DI
│   └── AutoCADTools.App          <- AutoCAD plugin entry point, DI host
└── tests/
    └── AutoCADTools.Test         <- xUnit tests (net8.0 + net48)
```

**Dependency flow:**
```
App -> Presentation -> Service -> Core
     (DI host)    WPF+CAD    (domain)
```

## Key Architecture Notes

- `AutoCADTools.App` outputs as `AutoCADTools.dll` (not `Ko0lsApp.dll`) and is loaded by AutoCAD as a bundle via `PackageContents.xml`.
- `AutoCADTools.Presentation` is the main implementation layer; `Core` and `Service` are currently stubs.
- The `BindableObject` class in `Presentation/Utils/` is the primary implemented feature -- an MVVM ViewModel base class implementing `IRevertibleChangeTracking`, `INotifyPropertyChanged`, and `INotifyDataErrorInfo`.
- Change tracking uses `[ChangeTracker]` attributed properties with JSON round-trip cloning via `ObjectCloner` (`Newtonsoft.Json`).
- `Ko0lsApp/` at the root is a legacy .NET Framework project not included in the solution.

## Style Conventions

- 2-space indentation, CRLF line endings.
- Braces on new lines for methods/types.
- `I` prefix for interfaces (e.g., `IBindableObject`).
- TreatWarningsAsErrors is enabled.

## Debugging

Use `.run/Ko0lsApp-2023.run.xml` (JetBrains Rider / IntelliJ) to launch `acad.exe` with the project built first.
