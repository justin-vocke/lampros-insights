# Frontend Audit

Jira: LAM-119, LAM-120 (parent LAM-118, "Audit frontend architecture and API integration readiness")
Date: 2026-11-10
Scope: `web/` (React 19 + Vite + TypeScript)

## Summary

The frontend is a single page with two components. It is not wired to the API: results come from a hardcoded array. `npm run build` and `npm run lint` both pass (the one build error found during the audit has been fixed).

## Pages and routing

- One page: `src/App.tsx`, titled "Analytics UI Preview".
- No router. None is needed for the MVP.

## Components

| Component | What it does | Keep? |
|---|---|---|
| `src/components/QuestionInput.tsx` | Form with a textarea and a "Run Query" button. Trims input, disables submit when empty, passes the question up via `onSubmit`. | Keep. Needs a disabled/loading state while a request runs. |
| `src/components/ResultsTable.tsx` | Renders any array of row objects as a table. Shows null as blank, objects as JSON, and "No results" when empty. | Keep, with changes (see gap 4). |

## Forms

- `QuestionInput` is the only form. It has no loading state and does not block a second submit while a request is in flight.

## Hardcoded and placeholder data

- `src/App.tsx:7-11`: `sampleResults` (month / totalRevenue / orders / region) is always rendered, regardless of the question. This is the main thing to remove.
- `src/App.tsx:31`: heading "Analytics UI Preview".
- `index.html:7`: browser tab title is still `web`.
- Unused Vite starter leftovers:
  - `src/App.css` (counter, hero, next-steps, etc.; never imported)
  - `.counter` rule in `src/index.css`
  - `src/assets/hero.png`, `src/assets/react.svg`, `src/assets/vite.svg`
  - possibly `public/icons.svg`

## Styling

- All component styles are inline with hardcoded colors.
- The starter `index.css` still defines its own light/dark theme, so the two may clash in dark mode.

## Gaps blocking API integration

1. **Build error (fixed).** `src/components/QuestionInput.tsx` imported `FormEvent` as a value while `verbatimModuleSyntax` is enabled (TS1484). It is now `import type { FormEvent } from "react";`. The dev server ran anyway because Vite does not type-check.
2. **No `src/api/` folder.** CLAUDE.md requires API calls to live there. There is no client for `POST /api/analytics/generate-sql` and no TypeScript types for the response:
   - `sql: string`
   - `isValid: boolean`
   - `validationErrors: string[]`
   - `errorMessage: string | null`
   - `result: { columns: { name, type }[], rows: Record<string, unknown>[], rowCount, executionTimeMs } | null`
3. **Frontend cannot reach the backend.** There is no Vite dev proxy in `vite.config.ts` and no CORS configuration in `LamprosInsights.Api/Program.cs`. One of the two is needed.
4. **`ResultsTable` builds its columns from the keys of the first row.** The API already returns an ordered `columns` list with types. The table should take `columns` and `rows` from the response, so column order is preserved and headers still show when a query returns zero rows.
5. **Missing UI the workflow needs:**
   - loading indicator
   - request/server error message (`errorMessage`, network failures)
   - validation error list when `isValid` is false
   - generated SQL, shown read-only
   - row count and execution time

## API client, configuration, and contracts (LAM-120)

### Current state

The frontend does not call the backend at all. There is no `fetch`, no HTTP library, no `import.meta.env` usage, and no reference to `/api` anywhere in `web/`.

| Area | Status | Evidence |
|---|---|---|
| Centralized API client (`src/api/`) | Missing | Folder does not exist. |
| Base URL configuration | Missing | No `.env*` files, no `VITE_API_BASE_URL`, no typed `ImportMetaEnv`. `tsconfig.app.json` already includes `vite/client` types, so adding a typed env var needs no new dependency. |
| Dev connectivity | Missing | No `server.proxy` in `vite.config.ts`, no CORS in `Program.cs`. |
| Request/response TS types | Missing | Only an ad-hoc `TableRow = Record<string, unknown>` in `App.tsx`. |
| Error handling | Missing | No client code to handle errors; backend also has no defined error shape (see below). |
| Cancellation / timeout | Missing | No `AbortController`. |
| HTTP dependency | Not needed | Native `fetch` covers the MVP. No axios or similar should be added. |

### Backend contract the client must match

`POST /api/analytics/generate-sql` (`AnalyticsController`). ASP.NET Core serializes with camelCase property names by default; no JSON options are customized.

Request: `{ question: string }`

Response (`200 OK`), from `GenerateSqlResponse` / `AnalyticsQueryResult` / `ColumnDefinition`:

```ts
type GenerateSqlResponse = {
  sql: string;
  isValid: boolean;
  validationErrors: string[];
  errorMessage: string | null; // declared, but never set by the backend today
  result: {
    columns: { name: string; type: string }[];
    rows: Record<string, unknown>[];
    rowCount: number;
    executionTimeMs: number;
  } | null; // null when isValid is false
};
```

### Contract gaps and risks

1. **Failures are not part of the contract.** `AnalyticsService` rethrows every exception (OpenAI error, SQL error, timeout), so the client gets a bare `500`: the developer exception page (HTML) in Development, and an empty body otherwise. `errorMessage` is never populated. The client cannot show a useful error until the API returns a consistent error body (ProblemDetails).
2. **Validation failure is a `200`.** Invalid SQL returns `200` with `isValid: false` and `result: null`. The client must check `isValid`, not just the HTTP status. This is a reasonable design, but it has to be documented and typed as a separate outcome.
3. **Row dictionaries lose data for duplicate or unnamed columns.** `SqlServerSqlExecutor` keys each row by column name. `SELECT c.Name, p.Name ...` produces two `Name` entries in `columns`, but the second value overwrites the first in every row. Unnamed expressions such as `COUNT(*)` get an empty name `""`. A frontend that renders from `columns` will show duplicate or blank headers and wrong cells, and React keys will collide. Possible fixes are positional rows (`unknown[][]`) or de-duplicating names on the server.
4. **Column `type` is a .NET type name** (`Int32`, `Decimal`, `DateTime`, `String`, `Boolean`, `Guid`, `Byte[]`, ...), not a SQL or JSON type. If the frontend uses it for formatting or alignment, the client types should treat this as a known string set.
5. **Value serialization caveats for the UI:**
   - `decimal` and `bigint` are sent as JSON numbers, so values above 2^53 or with high precision lose accuracy in JavaScript.
   - `DateTime` is sent without an offset (`"2024-01-01T00:00:00"`). `new Date()` reads it as local time.
   - `varbinary` is sent as a base64 string.
6. **No server-side request validation.** `GenerateSqlRequest.Question` has no `[Required]` or length limit, so an empty or very large question still reaches the AI provider. Client-side trimming in `QuestionInput` does not replace this.
7. **Client cancellation is not propagated.** `AnalyticsService` calls `_sqlExecutor.ExecuteAsync(sql)` without the request's `CancellationToken`. An aborted request from the browser still runs the query to completion.
8. **Unbounded payloads.** The executor sets no row limit and no explicit `CommandTimeout` (SqlClient default is 30s). The frontend could receive very large responses. CLAUDE.md requires both limits.
9. **HTTPS redirect and dev URLs.** `UseHttpsRedirection` is on. If the frontend calls `http://localhost:5284` while the API runs the `https` profile, it gets a redirect, which breaks CORS preflight. A Vite proxy to one fixed URL avoids this in dev.
10. **Endpoint shape vs. planned UX.** `generate-sql` generates and executes in one call. LAM-79 wants generated SQL shown *before* execution, which needs separate generate and execute calls. The client types should be written so this split is a small change.
11. **Template leftover.** `WeatherForecastController` is still registered and appears in Swagger.

### Where each gap belongs

To avoid duplicating LAM-27 ("Standardize frontend API and error handling"), the frontend-side items are listed as input to that ticket, not as new work.

| Gap | Covered by |
|---|---|
| `src/api/` client, typed DTOs, base URL / env config, dev proxy or CORS, error parsing, `AbortController` | LAM-27 |
| Consistent error body (ProblemDetails), `errorMessage` usage (1) | LAM-25 (backend problem responses) |
| Validation outcome typing (2), HTTPS redirect in dev (9) | LAM-27 |
| Duplicate/unnamed columns (3), type names (4), value formatting (5) | LAM-74 (result payload) and LAM-83 / LAM-84 (grid) |
| Row limit, timeout, cancellation (7, 8) | LAM-74 / LAM-77 |
| Separate generate and execute calls (10) | LAM-79 |
| Request validation (6), `WeatherForecastController` (11) | No clear owner. Candidates for LAM-122 to propose. |

## Out of scope for this audit

Apart from the one-line build fix above, no code was changed. Other fixes are expected to land in follow-up subtasks under LAM-118.
