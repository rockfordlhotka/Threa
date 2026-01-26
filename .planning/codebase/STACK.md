# Technology Stack

**Analysis Date:** 2026-01-24

## Languages

**Primary:**
- C# 12+ - All business logic, backend services, DAL implementations

**Secondary:**
- HTML/Razor - Blazor components and pages in `Threa/Threa/Components/` and `Threa/Threa.Client/`
- JavaScript/TypeScript - Implicit via Blazor WASM and InteractiveServer rendering

## Runtime

**Environment:**
- .NET 10.0 - Current target framework for all projects
- .NET 8.0 - Legacy support in build configuration

**Package Manager:**
- NuGet (.csproj packages)
- Lockfile: `packages.lock.json` (auto-generated per project)

## Frameworks

**Core:**
- CSLA.NET 9.1.0 - Business object framework with data operations in `GameMechanics/`
  - `Csla` - Core framework
  - `Csla.AspNetCore` 9.1.0 - ASP.NET Core integration in `Threa/Threa/Threa.csproj`
  - `Csla.Blazor` 9.1.0 - Blazor binding and data binding
  - `Csla.Blazor.WebAssembly` 9.1.0 - WASM-specific integration in `Threa/Threa.Client/Threa.Client.csproj`

**UI/Web:**
- Blazor Web App with hybrid rendering (Server + WebAssembly) in `Threa/Threa/`
  - SDK: `Microsoft.NET.Sdk.Web`
  - Components: `Microsoft.AspNetCore.Components.QuickGrid` 10.0.1 in `Threa/Threa.Client/`
  - UI Components: `Radzen.Blazor` 8.4.2 (both Threa and Threa.Client)

**Testing:**
- MSTest - Test framework in `GameMechanics.Test/GameMechanics.Test.csproj`
  - `Microsoft.NET.Test.Sdk` 18.0.1
  - `MSTest.TestAdapter` 4.0.2
  - `MSTest.TestFramework` 4.0.2
  - Coverage: `coverlet.collector` 6.0.4

**Build/Dev:**
- Microsoft.Extensions.DependencyInjection 10.0.1 - Dependency injection across projects
- Microsoft.Extensions.Configuration 10.0.0 - Configuration management in `Threa.Admin/`
- Microsoft.Extensions.Logging.Abstractions 10.0.1 - Logging abstractions
- System.Reactive 6.0.1 - Reactive extensions for messaging in `GameMechanics.Messaging.InMemory/`

## Key Dependencies

**Critical:**
- `Csla` 9.1.0 - Business object framework, use for all domain entities
- `Microsoft.Data.Sqlite` 10.0.1 - SQLite data access in `Threa.Dal.SqlLite/`
- `BCrypt.Net-Next` 4.0.3 - Password hashing in `GameMechanics/`, `Threa.Dal.MockDb/`, `Threa.Dal.SqlLite/`

**Infrastructure:**
- `Microsoft.AspNetCore.Authentication.Cookies` - Built-in, used in `Threa/Threa/Program.cs`
- `Microsoft.AspNetCore.Components.WebAssembly.Server` 10.0.1 - Server-side WASM hosting
- `CsvHelper` 33.0.1 - CSV/TSV parsing in `Threa.Admin/` for skill and item imports
- `Spectre.Console` 0.49.1, `Spectre.Console.Cli` 0.49.1 - CLI framework in `Threa.Admin/`

## Configuration

**Environment:**
- Configuration source: `appsettings.json` and `appsettings.Development.json`
- Key configs in `Threa/Threa/appsettings.json`:
  - `Logging:LogLevel` - Default: Information, Microsoft.AspNetCore: Warning
  - `ConnectionStrings:Sqlite` - SQLite database path
  - `AllowedHosts` - "*" (all hosts allowed)
- ASPNETCORE_ENVIRONMENT environment variable configures Development vs Production

**Build:**
- Project files: `*.csproj` with property groups defining nullable, implicit usings
- All projects: `<Nullable>enable</Nullable>` - Strict null checking
- All projects: `<ImplicitUsings>enable</ImplicitUsings>` - Global imports
- Main Blazor app: `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>`
- Test project: `<Parallelize>true</Parallelize>` for concurrent test execution

## Platform Requirements

**Development:**
- Windows (win32) - Build currently targets Windows
- Visual Studio 2022+ or VS Code with C# extension
- .NET SDK 10.0+
- SQLite database support (Microsoft.Data.Sqlite)

**Production:**
- ASP.NET Core hosting (IIS, Docker, Linux with .NET Runtime)
- SQLite database file or configured connection string
- HTTPS for Blazor Server connections
- Launch ports: HTTPS `localhost:7133`, HTTP `localhost:5181` (development)
- IIS Express ports: `localhost:50599` (HTTP), `localhost:44364` (HTTPS)

---

*Stack analysis: 2026-01-24*
