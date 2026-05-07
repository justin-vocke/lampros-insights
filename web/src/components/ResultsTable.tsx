type ResultsTableProps = {
  data?: Array<Record<string, unknown>> | null;
};

const formatCellValue = (value: unknown): string => {
  if (value === null || value === undefined) {
    return "";
  }

  if (typeof value === "object") {
    try {
      return JSON.stringify(value);
    } catch {
      return String(value);
    }
  }

  return String(value);
};

export default function ResultsTable({ data }: ResultsTableProps) {
  if (!data || data.length === 0) {
    return <p>No results</p>;
  }

  const columns = Object.keys(data[0] ?? {});

  if (columns.length === 0) {
    return <p>No results</p>;
  }

  return (
    <div style={{ width: "100%", overflowX: "auto" }}>
      <table
        style={{
          width: "100%",
          borderCollapse: "collapse",
          border: "1px solid #d1d5db",
        }}
      >
        <thead>
          <tr>
            {columns.map((column) => (
              <th
                key={column}
                style={{
                  textAlign: "left",
                  border: "1px solid #d1d5db",
                  padding: "0.5rem",
                  backgroundColor: "#f9fafb",
                }}
              >
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {columns.map((column) => (
                <td
                  key={`${rowIndex}-${column}`}
                  style={{
                    border: "1px solid #d1d5db",
                    padding: "0.5rem",
                    verticalAlign: "top",
                  }}
                >
                  {formatCellValue(row[column])}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
