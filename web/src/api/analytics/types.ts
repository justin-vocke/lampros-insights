// The only module defining analytics API request/response/result types.
import type { ApiProblem } from '../problemDetails'

export type GenerateSqlRequest = { question: string }

export type ColumnDefinition = {
  name: string
  /** .NET type name, e.g. "Int32". */
  type: string
}

export type AnalyticsQueryResult = {
  columns: ColumnDefinition[]
  rows: Record<string, unknown>[]
  rowCount: number
  executionTimeMs: number
}

/** Raw wire shape of POST /analytics/generate-sql (200). */
export type GenerateSqlResponse = {
  sql: string
  isValid: boolean
  validationErrors: string[]
  result: AnalyticsQueryResult | null
}

export type GenerateSqlSuccess = {
  sql: string
  validationErrors: string[]
  result: AnalyticsQueryResult
}

export type GenerateSqlInvalid = {
  sql: string
  validationErrors: string[]
}

export type GenerateSqlOutcome =
  | { kind: 'success'; data: GenerateSqlSuccess }
  | { kind: 'invalidSql'; data: GenerateSqlInvalid }
  | { kind: 'apiError'; problem: ApiProblem; status: number; message: string }
  | {
      kind: 'failure'
      reason: 'network' | 'invalidResponse'
      status?: number
      message: string
    }
  | { kind: 'cancelled' }
