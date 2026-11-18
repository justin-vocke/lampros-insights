export type ApiProblem = {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Record<string, string[]>
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function parseErrors(value: unknown): Record<string, string[]> | undefined {
  if (!isRecord(value)) return undefined
  const errors: Record<string, string[]> = {}
  for (const [key, messages] of Object.entries(value)) {
    if (Array.isArray(messages) && messages.every((m) => typeof m === 'string')) {
      errors[key] = messages as string[]
    }
  }
  return errors
}

/**
 * Tolerant ProblemDetails parser. Requires at least a string `title` or numeric
 * `status`; copies only correctly-typed known fields.
 */
export function parseProblemDetails(value: unknown): ApiProblem | null {
  if (!isRecord(value)) return null
  if (typeof value.title !== 'string' && typeof value.status !== 'number') {
    return null
  }

  const problem: ApiProblem = {}
  if (typeof value.type === 'string') problem.type = value.type
  if (typeof value.title === 'string') problem.title = value.title
  if (typeof value.status === 'number') problem.status = value.status
  if (typeof value.detail === 'string') problem.detail = value.detail
  if (typeof value.instance === 'string') problem.instance = value.instance
  if (typeof value.traceId === 'string') problem.traceId = value.traceId
  const errors = parseErrors(value.errors)
  if (errors) problem.errors = errors
  return problem
}
