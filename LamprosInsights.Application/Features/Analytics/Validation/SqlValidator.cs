using LamprosInsights.Application.Features.Analytics.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Validation
{
    public class SqlValidator : ISqlValidator
    {
        private readonly List<SqlRule> _rules;
        private static readonly string[] Forbidden =
        {
            "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE", "EXEC", "EXECUTE"
        };
        public SqlValidator()
        {
            _rules = new List<SqlRule>
        {
            MustStartWithSelect,
            MustNotContainForbiddenKeywords,
            MustNotContainComments
        };
        }

        public SqlValidationResult Validate(string sql)
        {
            var result = new SqlValidationResult();

            if (string.IsNullOrWhiteSpace(sql))
            {
                result.Errors.Add("The resulting SQL is invalid. Please try a new request");
                return result;
            }

            var normalized = NormalizeSql(sql);

            foreach (var rule in _rules)
            {
                var error = rule(normalized);
                if (error != null)
                    result.Errors.Add(error);
            }

            return result;
        }

        private string NormalizeSql(string sql)
            => sql.Trim().ToUpperInvariant();

        private string? MustStartWithSelect(string sql)
        {
            return sql.StartsWith("SELECT")
                ? null
                : "SQL must begin with SELECT statement";
        }

        private string? MustNotContainForbiddenKeywords(string sql)
        {
            foreach (var keyword in Forbidden)
            {
                //TODO: Want a more robust check that doesn't use Contains, as this could become a false positive based on
                //incoming SQL.
                if (sql.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return $"SQL cannot contain {keyword}";
            }

            return null;
        }

        //private string? MustNotContainMultipleStatements(string sql)
        //{
        //    return sql.Contains(";")
        //        ? "Multiple SQL statements are not allowed"
        //        : null;
        //}

        private string? MustNotContainComments(string sql)
        {
            if (sql.Contains("--") || sql.Contains("/*") || sql.Contains("*/"))
                return "SQL comments are not allowed";

            return null;
        }
        private delegate string? SqlRule(string sql);
    }
}
