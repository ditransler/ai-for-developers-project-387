/** Local calendar date key YYYY-MM-DD for grouping slot starts. */
export function dateKeyLocal(iso: string): string {
  const d = new Date(iso)
  return [
    d.getFullYear(),
    String(d.getMonth() + 1).padStart(2, '0'),
    String(d.getDate()).padStart(2, '0'),
  ].join('-')
}

export function parseDateKeyLocal(key: string): Date {
  const [y, m, day] = key.split('-').map(Number)
  return new Date(y, m - 1, day)
}

export function formatLongDate(d: Date, locale = 'en'): string {
  return new Intl.DateTimeFormat(locale, {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
  }).format(d)
}

export function formatTimeRange(
  startIso: string,
  endIso: string,
  locale = 'en'
): string {
  const start = new Date(startIso)
  const end = new Date(endIso)
  const tf = new Intl.DateTimeFormat(locale, {
    hour: '2-digit',
    minute: '2-digit',
  })
  return `${tf.format(start)} – ${tf.format(end)}`
}
