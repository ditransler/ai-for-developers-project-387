import type { FetchError } from 'ofetch'

export function useApiClient() {
  const config = useRuntimeConfig()
  return $fetch.create({
    baseURL: config.public.apiBase as string,
  })
}

export type FetchErrorHints = {
  network?: string
  server?: string
}

export function getFetchErrorMessage(
  error: unknown,
  fallback: string,
  hints?: FetchErrorHints
): string {
  const e = error as FetchError<{ message?: string }> & {
    statusCode?: number
    message?: string
  }

  const data = e.data
  if (data && typeof data === 'object' && 'message' in data) {
    const m = String((data as { message?: string }).message ?? '').trim()
    if (m) return m
  }

  const status = e.statusCode
  const rawMessage = typeof e.message === 'string' ? e.message : ''

  if (hints?.server && typeof status === 'number' && status >= 500) {
    return hints.server
  }

  const networkish =
    status === undefined ||
    status === 0 ||
    /failed to fetch|networkerror|load failed|fetch failed|econnrefused|enotfound|aborted|cors|timeout/i.test(
      rawMessage
    )

  if (hints?.network && networkish) {
    return hints.network
  }

  if (rawMessage.trim() && typeof status === 'number' && status >= 400) {
    return rawMessage
  }

  return fallback
}
