import { useCallback, useEffect, useRef, useState } from 'react'
import { generateSql } from '../api/analytics/generateSql'
import type {
  GenerateSqlInvalid,
  GenerateSqlSuccess,
} from '../api/analytics/types'

export type GenerateSqlState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: GenerateSqlSuccess }
  | { status: 'invalidSql'; data: GenerateSqlInvalid }
  | {
      status: 'error'
      error: {
        message: string
        status?: number
        traceId?: string
        kind: 'apiError' | 'failure'
      }
    }
  | { status: 'cancelled' }

export function useGenerateSql() {
  const [state, setState] = useState<GenerateSqlState>({ status: 'idle' })
  const controllerRef = useRef<AbortController | null>(null)

  const generate = useCallback(async (question: string) => {
    controllerRef.current?.abort()
    const controller = new AbortController()
    controllerRef.current = controller
    setState({ status: 'loading' })

    const outcome = await generateSql(question, { signal: controller.signal })

    // Ignore superseded, cancelled, or unmounted requests.
    if (controllerRef.current !== controller) return
    controllerRef.current = null

    switch (outcome.kind) {
      case 'success':
        setState({ status: 'success', data: outcome.data })
        break
      case 'invalidSql':
        setState({ status: 'invalidSql', data: outcome.data })
        break
      case 'apiError':
        setState({
          status: 'error',
          error: {
            kind: 'apiError',
            message: outcome.message,
            status: outcome.status,
            traceId: outcome.problem.traceId,
          },
        })
        break
      case 'failure':
        setState({
          status: 'error',
          error: {
            kind: 'failure',
            message: outcome.message,
            status: outcome.status,
          },
        })
        break
      case 'cancelled':
        break
    }
  }, [])

  const cancel = useCallback(() => {
    const controller = controllerRef.current
    if (!controller) return
    controllerRef.current = null
    controller.abort()
    setState({ status: 'cancelled' })
  }, [])

  useEffect(() => {
    return () => {
      controllerRef.current?.abort()
      controllerRef.current = null
    }
  }, [])

  return { state, generate, cancel }
}
