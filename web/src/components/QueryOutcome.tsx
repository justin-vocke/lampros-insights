import type { GenerateSqlState } from "../hooks/useGenerateSql";
import ResultsTable from "./ResultsTable";

type QueryOutcomeProps = {
  state: GenerateSqlState;
  question?: string;
};

export default function QueryOutcome({ state, question }: QueryOutcomeProps) {
  switch (state.status) {
    case "idle":
      return null;

    case "loading":
      return (
        <div role="status" aria-live="polite">
          Generating and running query…
        </div>
      );

    case "success": {
      const { rows, rowCount, executionTimeMs, isTruncated } = state.data.result;
      return (
        <div style={{ display: "flex", flexDirection: "column", gap: "0.75rem" }}>
          {question ? (
            <p style={{ margin: 0 }}>
              Results for: <strong>{question}</strong>
            </p>
          ) : null}
          <p style={{ margin: 0 }}>
            {rowCount} {rowCount === 1 ? "row" : "rows"} · {executionTimeMs} ms
          </p>
          {isTruncated ? (
            <p style={{ margin: 0 }}>
              Showing the first {rowCount} rows. The query returned more; refine
              your question to narrow the results.
            </p>
          ) : null}
          <ResultsTable data={rows} />
        </div>
      );
    }

    case "invalidSql":
      return (
        <div role="alert">
          <p style={{ margin: 0 }}>
            The generated SQL was rejected by validation and was not run.
          </p>
          <ul>
            {state.data.validationErrors.map((message, index) => (
              <li key={index}>{message}</li>
            ))}
          </ul>
        </div>
      );

    case "error":
      return (
        <div role="alert">
          <p style={{ margin: 0 }}>{state.error.message}</p>
          {state.error.traceId ? (
            <p style={{ margin: "0.25rem 0 0", fontSize: "0.875rem", color: "#6b7280" }}>
              Reference: {state.error.traceId}
            </p>
          ) : null}
        </div>
      );

    case "cancelled":
      return (
        <div role="status" aria-live="polite">
          Query cancelled.
        </div>
      );

    default: {
      const unreachable: never = state;
      return unreachable;
    }
  }
}
