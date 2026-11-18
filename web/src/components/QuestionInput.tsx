import { useMemo, useState } from "react";
import type { FormEvent } from "react";

type QuestionInputProps = {
  onSubmit: (question: string) => void;
  isSubmitting: boolean;
  onCancel: () => void;
};

export default function QuestionInput({ onSubmit, isSubmitting, onCancel }: QuestionInputProps) {
  const [question, setQuestion] = useState("");

  const isSubmitDisabled = useMemo(
    () => isSubmitting || question.trim().length === 0,
    [isSubmitting, question],
  );

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (isSubmitting) {
      return;
    }

    const trimmedQuestion = question.trim();
    if (!trimmedQuestion) {
      return;
    }

    onSubmit(trimmedQuestion);
  };

  return (
    <form
      onSubmit={handleSubmit}
      aria-busy={isSubmitting}
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
        readOnly={isSubmitting}
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

      <div style={{ display: "flex", gap: "0.5rem", alignSelf: "flex-start" }}>
        <button
          type="submit"
          disabled={isSubmitDisabled}
          style={{
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

        {isSubmitting ? (
          <button
            type="button"
            onClick={onCancel}
            style={{
              padding: "0.5rem 1rem",
              borderRadius: "6px",
              border: "1px solid #111827",
              backgroundColor: "#ffffff",
              color: "#111827",
              cursor: "pointer",
              font: "inherit",
            }}
          >
            Cancel
          </button>
        ) : null}
      </div>
    </form>
  );
}
