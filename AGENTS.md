# AGENTS.md

## What this repo is

Multi-project .NET library published as 6 independent NuGet packages. Not an application — no runnable entry point, no dev server, no appsettings.

## Project layout and dependency order

Build order (each depends only on projects above it):

1. **MZBase.Domain** — `netstandard2.1` — Base model/entity interfaces (`IModel`, `Auditable`, `IDto`)
2. **MZBase.Infrastructure** — `netstandard2.1` — Repository/UoW interfaces, service base classes, exceptions. Depends on Domain
3. **MZBase.EntityFrameworkCore** — `net8.0;net9.0` — EF Core implementations of Infrastructure interfaces. Depends on Domain + Infrastructure
4. **MZBase.EntityFrameworkCore.Sql** — `net8.0;net9.0` — Standalone SQL Server helper (DB description updater). No project references
5. **MZBase.Microservices** — `net8.0;net9.0` — HTTP mediator/proxy base class. No project references
6. **MZBase.Test** — `netstandard2.1` — **Not a test runner project.** Publishable NuGet package containing abstract test base classes (xUnit + Moq + AutoBogus) for consumers to inherit. Depends on Domain + Infrastructure

## Commands

```bash
dotnet restore MZBase.sln
dotnet build MZBase.sln --configuration Release
dotnet pack --configuration Release --output ./nupkg MZBase.{ProjectName}/MZBase.{ProjectName}.csproj
```

There is no test runner. The CI workflows build and pack only — no `dotnet test` step exists. MZBase.Test contains abstract base classes that consumers inherit in their own test projects (see [MZBaseSample](https://github.com/mzand111/MZBaseSample)).

## Key conventions

- **Nullable reference types:** enabled in every project
- **Implicit usings:** only in net8.0/net9.0 projects (not in netstandard2.1 projects)
- **Generic type parameter names:** use descriptive PascalCase — `ModelItem`, `DBModelEntity`, `PrimaryKeyType`, `DomainModelEntity`
- **Generic constraints for repositories:** `where DBModelEntity : ModelItem, IConvertibleDBModelEntity<ModelItem>, new()`
- **Primary key constraint:** `where T : struct`
- **Private fields:** `_camelCase` underscore prefix
- **Regions:** used to group operations (e.g. `#region Add`, `#region Modify`, `#region single retrieve`)
- **Domain/Entity split:** Domain models inherit `Model<T>` or `Auditable<T>`; DB entities implement `IConvertibleDBModelEntity<T>` with `SetFieldsFromDomainModel()` and `GetDomainObject()`

## NuGet dependency

This library depends on `MZSimpleDynamicLinq` (see [GitHub](https://github.com/mzand111/MZSimpleDynamicLinq)):
- Domain uses `MZSimpleDynamicLinq.Core`
- EntityFrameworkCore uses `MZSimpleDynamicLinq.EFCoreExtensions`

## CI/CD

Per-project GitHub Actions workflows (6 auto-publish + 6 manual). Each:
- Triggers on push to its project folder or release creation (auto) / `workflow_dispatch` (manual)
- Uses .NET SDK 9.0.x
- Runs: restore → build → pack → push to NuGet.org
- Uses `--skip-duplicate` for idempotent publishes
- **No test, lint, or typecheck steps**

## Gotchas

- MZBase.Test looks like a test project but is a **packable library** — do not add test runner infrastructure here
- No `global.json` — no pinned SDK version; CI uses 9.0.x
- No `.editorconfig` or `Directory.Build.props` — no enforced formatting or central package management
- Each project manages its own package versions independently in its `.csproj`
- `ServiceMediatorBase.cs` is ~1600 lines of HTTP client logic with retry/resilience — read carefully before modifying
