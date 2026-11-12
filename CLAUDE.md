# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

LamprosInsights is an AI analytics / BI platform: a user asks a question in natural language, an LLM generates SQL Server SQL, the backend validates and runs it, and the React UI renders the results.
Lampros Insights is being developed as a portfolio-quality,
production-oriented analytics application.

Prioritize sound engineering practices, security, maintainability,
and realistic architectural decisions while avoiding unnecessary
complexity for the current MVP scope.

- Backend: .NET 8 Web API, clean architecture (`Api` → `Application` → `Domain`, with `Infrastructure` implementing Application abstractions)
- AI: OpenAI (`OpenAI` NuGet, `gpt-4.1-mini`), with Azure OpenAI planned (empty `Infrastructure/AI/AzureOpenAI/` folder)
- Database: SQL Server via EF Core 8
- Frontend: React 19 + Vite + TypeScript in `web/`

## Commands

Backend (from repo root):
```
dotnet build LamprosInsights.sln
dotnet run --project LamprosInsights.Api        # http://localhost:5284, https://localhost:7030; Swagger at /swagger in Development
```

EF Core migrations (DbContext lives in Infrastructure, startup project is Api):
```
dotnet ef migrations add <Name> --project LamprosInsights.Infrastructure --startup-project LamprosInsights.Api --output-dir Persistence/Migrations
dotnet ef database update --project LamprosInsights.Infrastructure --startup-project LamprosInsights.Api
```
Sample data: `LamprosInsights.Infrastructure/Persistence/Seed/seed.sql`.

Frontend (from `web/`):
```
npm install
npm run dev      # Vite dev server
npm run build    # tsc -b && vite build
npm run lint     # eslint
```

There are no test projects yet.

## Configuration

Copy `LamprosInsights.Api/appsettings.Development.json.example` to `appsettings.Development.json` (gitignored) and set `ConnectionStrings:DefaultConnection` and `OpenAI:ApiKey`. `OpenAIProvider` reads `OpenAI:ApiKey` directly from `IConfiguration`; `OpenAIOptions` and `OpenAIServiceCollectionExtensions` are empty placeholders.

## Architecture: the question → SQL → results pipeline

The single vertical slice is `POST /api/analytics/generate-sql` (`AnalyticsController`), orchestrated by `Application/Features/Analytics/Services/AnalyticsService.GenerateSqlAsync`:

1. `ISchemaProvider.GetSchemaContextAsync` — `SqlServerSchemaProvider` reads `INFORMATION_SCHEMA.COLUMNS` and formats it as plain text for the prompt. (A structured `GetSchemaAsync` returning `DatabaseSchema`/`TableSchema`/`ColumnSchema` models is in progress and currently throws `NotImplementedException`.)
2. `AnalyticsPromptBuilder.BuildSqlGenerationPrompt` — all prompts are centralized in `Application/Features/Analytics/Prompts/`.
3. `IAIProvider.GenerateSqlAsync` — `Infrastructure/AI/OpenAI/OpenAIProvider` returns raw SQL text.
4. `ISqlValidator.Validate` — `SqlValidator` is a list of rule delegates (must start with SELECT, no forbidden keywords like DROP/DELETE/UPDATE/INSERT/ALTER/EXEC, no comments).
   Invalid SQL is returned to the caller with errors and is never executed.
  
   **Current limitation:** Validation currently relies on basic
   keyword and string-based checks. It is not yet a comprehensive
   SQL syntax/statement-structure validation layer.

   **Required security standard:** SQL validation must account for
   SQL syntax and statement structure, not rely exclusively on
   keyword matching. All generated SQL must pass validation before
   execution.
5. `ISqlExecutor.ExecuteAsync` — `SqlServerSqlExecutor` runs the SQL with raw ADO.NET (`SqlConnection`, not EF) and returns a schema-agnostic `AnalyticsQueryResult` (column definitions + rows as `Dictionary<string, object?>` + timing).

Interfaces live in `Application/Features/Analytics/Abstractions` (plus `Validation/ISqlValidator`); implementations live in `Infrastructure`; all DI wiring is in `LamprosInsights.Api/Program.cs` (everything scoped). Application code is organized by feature folder (`Features/Analytics/{Abstractions,Dtos,Models,Prompts,Services,Validation}`).

`Domain/Analytics` holds the sample business entities (Customer, Order, OrderItem, Invoice, Payment, Product, Region, SalesRep) plus `AnalyticsQueries` (query history). They are mapped in `AnalyticsDbContext` via `IEntityTypeConfiguration` classes in `Infrastructure/Persistence/Configurations`. EF is used for schema/migrations; analytics queries themselves bypass EF.

The frontend (`web/src`) currently renders hardcoded sample data in `App.tsx`; it is not yet wired to the API. `ResultsTable` is meant to render any result shape dynamically.

## Architectural Decision Making

- For significant architectural changes, evaluate alternatives before
  implementation.
- Explain the trade-offs involving complexity, maintainability,
  security, reliability, performance, and operational cost.
- Prefer the simplest architecture that satisfies current requirements.
- Document significant, accepted architectural decisions as ADRs
  under `docs/architecture/decisions/`.
- Do not introduce distributed systems patterns or infrastructure
  solely for hypothetical future scalability.
  
## Engineering Standards

- Controllers stay thin; services handle orchestration.
- Domain must not depend on Application, Infrastructure, or Api.
- Application must not depend on Infrastructure or Api.
- Infrastructure implements abstractions defined by Application or Domain.
- Api is responsible for HTTP concerns and dependency injection configuration.
- Avoid introducing circular dependencies between projects.
- Never call OpenAI directly — always go through `IAIProvider`. Prompts stay centralized and reusable.
- Treat all AI-generated SQL as untrusted.
- Never bypass SQL validation.
- SQL execution must use appropriately restricted, read-only
  database credentials.
- Apply query execution timeouts and result limits.
- Validation must account for SQL syntax and statement structure,
  not rely exclusively on keyword matching.
- Never expose database credentials to AI providers.
- Use structured logging for application operations.
- Log relevant metadata such as request IDs, execution duration,
  provider, status, and error details.
- Never log API keys, credentials, or sensitive customer data.
- Do not log full AI prompts, generated SQL, or query results
  by default.
- If diagnostic logging of prompts or SQL is introduced, it must
  be explicitly configurable, appropriately redacted, and
  protected.
- Frontend: functional components and hooks, API calls centralized in a `/api` folder, no business logic in the UI, schema-agnostic result tables, minimal UI.
- Build vertical slices first (Question → AI → SQL → DB → UI). Avoid premature optimization: no microservices, no event-driven architecture, no complex caching yet.
- Before introducing a new NuGet or npm dependency, explain why it is
  necessary and whether existing functionality can solve the problem.
- Prefer established, actively maintained dependencies.
