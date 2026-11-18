import { postJson, type RequestOptions } from '../http'
import type {
  AnalyticsQueryResult,
  GenerateSqlOutcome,
  GenerateSqlRequest,
  GenerateSqlResponse,
} from './types'

const NETWORK_MESSAGE =
  'Could not reach the server. Check your connection and try again.'
const INVALID_RESPONSE_MESSAGE = 'The server returned an unexpected response.'
const GENERIC_PROBLEM_MESSAGE = 'The request failed.'

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function isStringArray(value: unknown): value is string[] {
  return Array.isArray(value) && value.every((v) => typeof v === 'string')
}

function isQueryResult(value: unknown): value is AnalyticsQueryResult {
  return (
    isRecord(value) &&
    Array.isArray(value.columns) &&
    value.columns.every(
      (c) =>
        isRecord(c) && typeof c.name === 'string' && typeof c.type === 'string',
    ) &&
    Array.isArray(value.rows) &&
    typeof value.rowCount === 'number' &&
    typeof value.executionTimeMs === 'number'
  )
}

// Shallow guard: rows are not inspected per-row (schema-agnostic).
function isGenerateSqlResponse(value: unknown): value is GenerateSqlResponse {
  return (
    isRecord(value) &&
    typeof value.sql === 'string' &&
    typeof value.isValid === 'boolean' &&
    isStringArray(value.validationErrors) &&
    (value.result === null || isQueryResult(value.result))
  )
}

export async function generateSql(
  question: string,
  options?: RequestOptions,
): Promise<GenerateSqlOutcome> {
  const request: GenerateSqlRequest = { question }
  const outcome = await postJson('/analytics/generate-sql', request, options)

  switch (outcome.kind) {
    case 'cancelled':
      return outcome
    case 'problem': {
      const { title, detail } = outcome.problem
      const message = title
        ? detail
          ? `${title} ${detail}`
          : title
        : GENERIC_PROBLEM_MESSAGE
      return {
        kind: 'apiError',
        problem: outcome.problem,
        status: outcome.status,
        message,
      }
    }
    case 'failure':
      return {
        kind: 'failure',
        reason: outcome.reason,
        status: outcome.status,
        message:
          outcome.reason === 'network' ? NETWORK_MESSAGE : INVALID_RESPONSE_MESSAGE,
      }
    case 'ok': {
      const body = outcome.body
      const invalid = {
        kind: 'failure',
        reason: 'invalidResponse',
        status: outcome.status,
        message: INVALID_RESPONSE_MESSAGE,
      } as const
      if (!isGenerateSqlResponse(body)) return invalid
      if (!body.isValid) {
        return {
          kind: 'invalidSql',
          data: { sql: body.sql, validationErrors: body.validationErrors },
        }
      }
      if (body.result === null) return invalid
      return {
        kind: 'success',
        data: {
          sql: body.sql,
          validationErrors: body.validationErrors,
          result: body.result,
        },
      }
    }
  }
}
