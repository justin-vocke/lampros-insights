import { useState } from "react";
import QuestionInput from "./components/QuestionInput";
import ResultsTable from "./components/ResultsTable";

type TableRow = Record<string, unknown>;

const sampleResults: TableRow[] = [
  { month: "January", totalRevenue: 18400, orders: 123, region: "North" },
  { month: "February", totalRevenue: 20150, orders: 139, region: "North" },
  { month: "March", totalRevenue: 19320, orders: 131, region: "North" },
];

function App() {
  const [lastQuestion, setLastQuestion] = useState("");

  const handleSubmit = (question: string) => {
    setLastQuestion(question);
  };

  return (
    <main
      style={{
        maxWidth: "960px",
        margin: "0 auto",
        padding: "2rem 1rem",
        display: "flex",
        flexDirection: "column",
        gap: "1.5rem",
      }}
    >
      <h1 style={{ margin: 0 }}>Analytics UI Preview</h1>
      <QuestionInput onSubmit={handleSubmit} />

      {lastQuestion ? (
        <p style={{ margin: 0 }}>
          Last question: <strong>{lastQuestion}</strong>
        </p>
      ) : null}

      <ResultsTable data={sampleResults} />
    </main>
  );
}

export default App;
