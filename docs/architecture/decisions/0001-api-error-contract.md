# 0001. API error contract

## Status

Accepted

## Context

- Unhandled exceptions were returned as the HTML developer exception page in Development and an empty `500` otherwise.
- Exception messages can contain connection strings, SQL Server details, or partial API keys. Returning them to clients is a leak risk.
- The frontend (LAM-27) needs one predictable error shape.

## Decision

- All API errors use RFC 9457 / 7807 ProblemDetails (`application/problem+json`) in every environment, via `AddProblemDetails`, an `IExceptionHandler` (`GlobalExceptionHandler`), and `UseStatusCodePages`.
- Unhandled exceptions become a generic `500` with a fixed title and detail, plus a `traceId` for log correlation. Exception details appear only in server logs (logged once, without the question, prompts, SQL, or query string values).
- A request aborted by the client is logged at Information and answered with `499` and no body.
- SQL validation failure is a domain outcome (`200`, `isValid: false`), not an error.
- `GenerateSqlResponse.ErrorMessage` is removed so there is a single error channel.

## Alternatives considered

- **Bare `{ "message": "..." }`**: non-standard, no correlation id, no extensibility.
- **`success` / `data` / `error` envelope**: often returns `200` for errors, which breaks HTTP semantics and tooling.
- **Vendor formats** (Azure / Google `{ "error": { "code", "message" } }`): non-standard for ASP.NET, and ProblemDetails is built in.
- **Exception-type mapping to 502 / 504 now**: deferred; nothing in the UI needs it yet.
- **Showing exception details in Development**: rejected; the frontend would get HTML or an inconsistent shape in dev.

## Consequences

- The UI cannot yet distinguish failure causes; every server failure is the same `500`.
- Debugging relies on server logs and the `traceId`.

## Revisit when

The UX needs to distinguish failure causes (for example LAM-71 query timeout mapped to `504`). Then introduce Application-level exception types mapped in the handler, and a stable machine-readable `code` / `type`, so clients never branch on message text.
