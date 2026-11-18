import { apiBaseUrl } from './config'
import { parseProblemDetails, type ApiProblem } from './problemDetails'

export type HttpOutcome =
  | { kind: 'ok'; status: number; body: unknown }
  | { kind: 'problem'; status: number; problem: ApiProblem }
  | { kind: 'failure'; reason: 'network' | 'invalidResponse'; status?: number }
  | { kind: 'cancelled' }

export type RequestOptions = { signal?: AbortSignal }

function isAbort(error: unknown, signal?: AbortSignal): boolean {
  return (
    signal?.aborted === true ||
    (error instanceof DOMException && error.name === 'AbortError')
  )
}

/**
 * POSTs JSON and maps every outcome to a value. Never throws for HTTP or
 * network conditions and never surfaces raw response text or error messages.
 */
export async function postJson(
  path: string,
  body: unknown,
  options?: RequestOptions,
): Promise<HttpOutcome> {
  const signal = options?.signal

  let response: Response
  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json, application/problem+json',
      },
      body: JSON.stringify(body),
      signal,
    })
  } catch (error) {
    return isAbort(error, signal)
      ? { kind: 'cancelled' }
      : { kind: 'failure', reason: 'network' }
  }

  const status = response.status
  const isJson = (response.headers.get('content-type') ?? '')
    .toLowerCase()
    .includes('json')

  let payload: unknown
  let parsed = false
  if (response.ok || isJson) {
    try {
      payload = await response.json()
      parsed = true
    } catch (error) {
      if (isAbort(error, signal)) return { kind: 'cancelled' }
    }
  }

  if (response.ok) {
    return parsed
      ? { kind: 'ok', status, body: payload }
      : { kind: 'failure', reason: 'invalidResponse', status }
  }

  const problem = parsed ? parseProblemDetails(payload) : null
  return problem
    ? { kind: 'problem', status, problem }
    : { kind: 'failure', reason: 'invalidResponse', status }
}
