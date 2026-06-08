namespace LamprosInsights.Application.Features.Analytics.Prompts;

public class AnalyticsPromptBuilder
{
    public string BuildSqlGenerationPrompt(
        string schema,
        string userQuestion)
    {
        return $"""
            You are a senior SQL analytics assistant.

            Your task is to generate a valid SQL Server query.

            Rules:
            - ONLY generate SQL
            - DO NOT include markdown
            - DO NOT include explanations
            - ONLY generate SELECT statements, but don't allow SELECT * statements
            - NEVER generate UPDATE, DELETE, DROP, INSERT, or ALTER
            - Use TOP 100 unless explicitly requested otherwise
            - Use only tables and columns provided in the schema
            - Prefer readable aliases
            - Generate syntactically correct SQL Server syntax

            Database Schema:
            {schema}

            User Question:
            {userQuestion}

            Return ONLY the SQL query.
            """;
    }
}