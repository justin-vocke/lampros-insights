import { useState } from "react";
import QuestionInput from "./components/QuestionInput";
import QueryOutcome from "./components/QueryOutcome";
import { useGenerateSql } from "./hooks/useGenerateSql";

function App() {
  const { state, generate, cancel } = useGenerateSql();
  const [submittedQuestion, setSubmittedQuestion] = useState("");

  const handleSubmit = (question: string) => {
    setSubmittedQuestion(question);
    void generate(question);
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
      <QuestionInput
        onSubmit={handleSubmit}
        isSubmitting={state.status === "loading"}
        onCancel={cancel}
      />
      <QueryOutcome state={state} question={submittedQuestion} />
    </main>
  );
}

export default App;
