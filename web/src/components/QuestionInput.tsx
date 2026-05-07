import { FormEvent, useMemo, useState } from "react";

type QuestionInputProps = {
  onSubmit: (question: string) => void;
};

export default function QuestionInput({ onSubmit }: QuestionInputProps) {
  const [question, setQuestion] = useState("");

  const isSubmitDisabled = useMemo(() => question.trim().length === 0, [question]);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const trimmedQuestion = question.trim();
    if (!trimmedQuestion) {
      return;
    }

    onSubmit(trimmedQuestion);
  };

  return (
    <form
      onSubmit={handleSubmit}
      style={{
        display: "flex",
        flexDirection: "column",
        gap: "0.75rem",
        width: "100%",
      }}
    >
      <textarea
        rows={4}
        value={question}
        onChange={(event) => setQuestion(event.target.value)}
        placeholder="Ask a question about your data..."
        style={{
          width: "100%",
          minHeight: "6rem",
          padding: "0.75rem",
          borderRadius: "6px",
          border: "1px solid #d1d5db",
          resize: "vertical",
          font: "inherit",
          lineHeight: 1.4,
          boxSizing: "border-box",
        }}
      />

      <button
        type="submit"
        disabled={isSubmitDisabled}
        style={{
          alignSelf: "flex-start",
          padding: "0.5rem 1rem",
          borderRadius: "6px",
          border: "1px solid #111827",
          backgroundColor: isSubmitDisabled ? "#d1d5db" : "#111827",
          color: "#ffffff",
          cursor: isSubmitDisabled ? "not-allowed" : "pointer",
          font: "inherit",
        }}
      >
        Run Query
      </button>
    </form>
  );
}
