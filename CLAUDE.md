# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Dominex** is a .NET 9 Blazor WebAssembly + ASP.NET Core application implementing a Dominion-like card game. It uses gRPC for client-server communication and SignalR for real-time game updates.

## Common Commands

```powershell
# Build
dotnet build Dominex.sln

# Run (startup project is Web.Server)
dotnet run --project Web.Server

# Run all tests
dotnet test Dominex.sln

# Run a single test project
dotnet test GameCoreTests
dotnet test Facades.Tests

# Add EF migration (use Entity.Tests as startup project)
dotnet ef migrations add <MigrationName> --startup-project Entity.Tests --project Entity

# Update database
dotnet ef database update --startup-project Entity.Tests --project Entity

# Run code generator (generates DataSources, Repositories from Model entities)
dotnet efcodegenerator
```

## Architecture

The solution follows a strict layered architecture across ~25 projects:

```
Web.Server / Web.Client (Presentation)
    ↓ gRPC + SignalR
Facades   (Application use cases — GameFacade, GameSetupFacade, DataSeedFacade)
    ↓
Services  (Business logic)
    ↓
DataLayer (Generated repositories + data sources)
    ↓
Entity    (EF Core DbContext, entity configurations, migrations)
    ↓
Model     (Domain POCOs: User, Game, PresetKingdom, etc.)
```

**Key cross-cutting projects:**
- `Contracts` — gRPC service interfaces (`[ApiContract]`) and DTOs shared between client and server
- `GameCore` — Self-contained card game engine (card types, game state machine, AI players)
- `AI` — AI player implementations (evolution-based, ProvincialAI)
- `DependencyInjection` — All service registrations; services use `[Service]` source-generated DI attributes
- `Primitives` — Enums and role constants (`RoleEntry`)

## Data Access

Uses the **HAVIT EF Core Patterns** (Unit of Work + Repository + DataSource):

- **DataSources** (`IUserDataSource`, etc.) — low-level query builders, auto-generated
- **Repositories** (`IUserRepository`, etc.) — CRUD + domain queries, auto-generated
- Generated files live in `DataLayer/_generated/` — do not edit manually; regenerate with `dotnet efcodegenerator`
- Fake implementations for all repositories exist in test projects (in-memory EF or manual fakes)

EF migrations use `Entity.Tests` as the startup project (it has the right appsettings).

## gRPC & Real-Time

- API contracts are C# interfaces annotated with `[ApiContract]` in the `Contracts` project
- Client proxies and server stubs are generated from these interfaces (protobuf-net grpc, code-first)
- Three SignalR hubs for live game events: `LogHub`, `KingdomHub`, `PlayerStateHub`

## Dependency Injection

Services declare themselves via `[Service]` attributes (HAVIT source generators). DI profiles:
- `DefaultProfile` — shared services
- `WebServer` — server-only registrations
- `JobsRunner` — background job services (Hangfire, currently mostly TODO)

All registrations are wired in `DependencyInjection` project's `ServiceCollectionExtensions`.

## Domain Concepts

- **Card game**: Cards have types (Copper, Silver, Gold, Estate, Duchy, Province, plus action cards like Village, Market, Smithy). A **Kingdom** is a set of 10 action cards for a game session.
- **PresetKingdom**: Pre-configured card selections stored in the database (seeded in `CoreProfile`).
- **GameCore**: Fully self-contained engine — `Game`, `Player`, `CardCollection`, `IGameLogger`, observer interfaces.
- **Roles**: Defined as constants in `RoleEntry` (SystemAdministrator, UserSettingsAdministrator).
- **Localization**: `Language` entity + `ILocalized` interface; seed data includes language entries.

## Configuration

Config loads in this order (later overrides earlier):
1. `appsettings.WebServer.json`
2. `appsettings.WebServer.{Environment}.json`
3. `appsettings.WebServer.{Environment}.local.json` (git-ignored, for local secrets)
4. Environment variables → Azure Key Vault

Key settings to configure locally:
- `ConnectionStrings:Database` — SQL Server connection string
- `ConnectionStrings:AzureStorage` — optional, can use filesystem fallback
- `AppSettings:Migrations:RunMigrations` — set `true` to auto-migrate on startup

The `Entity/appSettings.Entity.json` file is copied to output and used during EF tooling commands.

## Authentication & Authorization

Auth uses **Google OAuth2 + Cookie** scheme via `Microsoft.AspNetCore.Authentication.Google`.

**Flow:**
1. Unauthenticated user → redirected to `/login` (Blazor page)
2. "Sign in with Google" → `GET /authentication/login` → Google OAuth challenge
3. Google callback → `HandleSigningInAsync` fires → `CustomClaimsBuilder.GetCustomClaimsAsync` runs
4. `GetCustomClaimsAsync` looks up local `User` by email; creates one if first login (`OnboardUserAsync`)
5. Custom claims (`dominex:user_id`, roles) baked into auth cookie
6. `PersistingAuthenticationStateProvider` (server) serializes `UserInfo` into prerendered HTML
7. `PersistentAuthenticationStateProvider` (client WASM) reads it — auth state fixed for session lifetime
8. Logout: `POST /authentication/logout` clears the cookie, full page reload required

**Key files:**
- `Web.Server/Infrastructure/ConfigurationExtensions/AuthenticationConfigurationExtension.cs` — registers cookie + Google schemes
- `Web.Server/Infrastructure/Security/LoginLogoutEndpointRouteBuilderExtensions.cs` — `/authentication/login` and `/authentication/logout` endpoints
- `Facades/Infrastructure/Security/Claims/CustomClaimsBuilder.cs` — find-or-create `User`, builds custom claims
- `Web.Server/Infrastructure/Security/PersistingAuthenticationStateProvider.cs` — server-side, persists auth state to HTML
- `Web.Client/Infrastructure/Security/PersistentAuthenticationStateProvider.cs` — client-side, reads persisted state
- `Web.Client/Pages/Account/Login.razor` — login landing page
- `Contracts/Infrastructure/Security/ClaimConstants.cs` — claim name constants (`dominex:user_id`)

**Roles** are stored in `UserRoles` DB table. First user in DEBUG gets all roles automatically.

**Local secrets** (never commit):
```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "..." --project Web.Server
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project Web.Server
```

Google Cloud Console redirect URI must match: `https://localhost:{port}/signin-google`

**Protecting pages/endpoints:**
- All Blazor pages: `@attribute [Authorize]` in `Web.Client/_Imports.razor`
- Login page exempt: `@attribute [AllowAnonymous]` in `Login.razor`
- gRPC facades: `[Authorize]` on `GameFacade`; `[Authorize(Roles = nameof(RoleEntry.SystemAdministrator))]` on `DataSeedFacade` and `MaintenanceFacade` (currently commented out — uncomment when ready)

## Build Notes

- `Directory.Build.props` sets `LangVersion=latest`, `Nullable=disable`, and `Warnings as Errors` in Release
- `Directory.Packages.props` centrally pins all NuGet versions — update versions there, not in individual `.csproj` files
- Code style is enforced in Release builds; development builds are more permissive
