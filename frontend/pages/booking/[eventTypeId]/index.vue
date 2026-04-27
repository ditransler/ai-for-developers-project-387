<script setup lang="ts">
import type { components } from '~/types/api.generated'
import {
  dateKeyLocal,
  formatLongDate,
  formatTimeRange,
  parseDateKeyLocal,
} from '~/utils/date'

type TimeSlot = components['schemas']['TimeSlot']

const { t } = useI18n()
const route = useRoute()
const appConfig = useAppConfig()
const api = useBookingApi()
const paramId = route.params.eventTypeId as string

const { data: eventType, error: typeError } = await useAsyncData(
  `event-type-${paramId}`,
  async () => {
    const list = await api.listPublicEventTypes()
    return list.find((e) => e.id === paramId) ?? null
  }
)

if (!typeError.value && eventType.value === null) {
  throw createError({ statusCode: 404, statusMessage: 'Event type not found' })
}

const {
  data: slots,
  pending: slotsPending,
  error: slotsError,
} = await useAsyncData(`slots-${paramId}`, () =>
  api.listAvailableSlots(paramId)
)

const slotsByDay = computed(() => {
  const m = new Map<string, TimeSlot[]>()
  for (const s of slots.value ?? []) {
    const k = dateKeyLocal(s.startAt)
    if (!m.has(k)) m.set(k, [])
    m.get(k)!.push(s)
  }
  for (const [, arr] of m)
    arr.sort((a, b) => a.startAt.localeCompare(b.startAt))
  return m
})

const selectedDateKey = ref<string | null>(null)
const selectedSlot = ref<TimeSlot | null>(null)

watch(
  slotsByDay,
  (m) => {
    const keys = [...m.keys()].sort()
    if (!keys.length) {
      selectedDateKey.value = null
      selectedSlot.value = null
      return
    }
    if (!selectedDateKey.value || !m.has(selectedDateKey.value))
      selectedDateKey.value = keys[0]!
  },
  { immediate: true }
)

watch(selectedDateKey, () => {
  selectedSlot.value = null
})

const viewMonth = ref(new Date())

watch(
  selectedDateKey,
  (k) => {
    if (k) {
      const d = parseDateKeyLocal(k)
      viewMonth.value = new Date(d.getFullYear(), d.getMonth(), 1)
    }
  },
  { immediate: true }
)

function localKey(y: number, monthIndex: number, day: number): string {
  const d = new Date(y, monthIndex, day, 12, 0, 0)
  return dateKeyLocal(d.toISOString())
}

const calendarCells = computed(() => {
  const v = viewMonth.value
  const y = v.getFullYear()
  const m = v.getMonth()
  const first = new Date(y, m, 1)
  const startPad = (first.getDay() + 6) % 7
  const daysInMonth = new Date(y, m + 1, 0).getDate()
  const cells: {
    label: number | null
    key: string | null
    inMonth: boolean
  }[] = []
  for (let i = 0; i < startPad; i++)
    cells.push({ label: null, key: null, inMonth: false })
  for (let d = 1; d <= daysInMonth; d++) {
    const key = localKey(y, m, d)
    cells.push({ label: d, key, inMonth: true })
  }
  return cells
})

const monthLabel = computed(() =>
  new Intl.DateTimeFormat('en', { month: 'long', year: 'numeric' }).format(
    viewMonth.value
  )
)

function shiftMonth(delta: number) {
  const v = viewMonth.value
  viewMonth.value = new Date(v.getFullYear(), v.getMonth() + delta, 1)
}

const slotsForSelectedDay = computed(() => {
  const k = selectedDateKey.value
  if (!k) return []
  return slotsByDay.value.get(k) ?? []
})

const selectedDateLabel = computed(() => {
  if (!selectedDateKey.value) return '—'
  return formatLongDate(parseDateKeyLocal(selectedDateKey.value))
})

const selectedTimeLabel = computed(() => {
  if (!selectedSlot.value) return t('booking.timeNotSelected')
  return formatTimeRange(selectedSlot.value.startAt, selectedSlot.value.endAt)
})

function selectDay(key: string) {
  if (!slotsByDay.value.has(key)) return
  selectedDateKey.value = key
}

function goBackToCatalog() {
  navigateTo('/booking')
}

function onContinue() {
  if (!selectedSlot.value) return
  navigateTo({
    path: `/booking/${paramId}/confirm`,
    query: { startAt: selectedSlot.value.startAt },
  })
}

function slotCountForKey(key: string | null) {
  if (!key) return 0
  return slotsByDay.value.get(key)?.length ?? 0
}

function dayCellClasses(cell: {
  label: number | null
  key: string | null
  inMonth: boolean
}) {
  const hasSlots = !!(cell.key && slotsByDay.value.has(cell.key))
  const isSelected = cell.key && selectedDateKey.value === cell.key
  if (!cell.key || !hasSlots) {
    return [
      'flex min-h-[3.25rem] w-full flex-col items-center justify-center gap-0.5 rounded-lg px-0.5 py-1.5 text-[11px] font-medium transition',
      'cursor-not-allowed bg-zinc-50 text-zinc-300',
    ]
  }
  const base =
    'flex min-h-[3.25rem] w-full flex-col items-center justify-center gap-0.5 rounded-lg px-0.5 py-1.5 text-[11px] font-medium transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 focus-visible:ring-offset-1'
  if (isSelected)
    return [
      base,
      'bg-white text-zinc-900 shadow-sm ring-2 ring-orange-500 ring-offset-0',
    ]
  return [base, 'bg-zinc-100 text-zinc-800 hover:bg-zinc-200/90']
}
</script>

<template>
  <UContainer class="py-10">
    <UAlert
      v-if="typeError"
      color="error"
      variant="soft"
      :title="t('errors.generic')"
      class="mb-6 rounded-xl"
    />

    <template v-else-if="eventType">
      <h1
        class="mb-8 text-2xl font-bold tracking-tight text-zinc-900 sm:text-3xl"
      >
        {{ eventType.name }}
      </h1>

      <div class="grid gap-6 lg:grid-cols-3">
        <!-- Summary -->
        <div
          class="h-fit overflow-hidden rounded-2xl border border-zinc-200/90 bg-white shadow-sm"
        >
          <div class="flex items-start gap-4 p-6">
            <UAvatar
              :src="appConfig.calendarHost.avatarSrc"
              size="3xl"
              :alt="appConfig.calendarHost.displayName"
              class="ring-2 ring-white shadow-md ring-offset-2 ring-offset-white"
            />
            <div class="min-w-0">
              <p class="font-semibold text-zinc-900">
                {{ appConfig.calendarHost.displayName }}
              </p>
              <p class="text-xs font-medium text-zinc-500">
                {{ t('booking.host') }}
              </p>
            </div>
          </div>
          <div class="border-t border-zinc-100 px-6 py-5">
            <div class="flex flex-wrap items-start justify-between gap-2">
              <h2 class="text-base font-semibold text-zinc-900">
                {{ eventType.name }}
              </h2>
              <span
                class="inline-flex shrink-0 items-center rounded-full bg-zinc-100 px-3 py-1 text-xs font-medium text-zinc-700"
              >
                {{ t('booking.durationMin', { n: eventType.durationMinutes }) }}
              </span>
            </div>
            <p class="mt-3 text-sm leading-relaxed text-zinc-600">
              {{ eventType.description }}
            </p>
          </div>
          <div class="space-y-3 border-t border-zinc-100 p-5">
            <div class="rounded-xl bg-sky-100/90 px-4 py-3">
              <p class="text-xs font-medium text-sky-900/85">
                {{ t('booking.selectedDate') }}
              </p>
              <p class="mt-1 text-sm font-medium text-zinc-900">
                {{ selectedDateLabel }}
              </p>
            </div>
            <div class="rounded-xl bg-sky-100/90 px-4 py-3">
              <p class="text-xs font-medium text-sky-900/85">
                {{ t('booking.selectedTime') }}
              </p>
              <p class="mt-1 text-sm font-medium text-zinc-900">
                {{ selectedTimeLabel }}
              </p>
            </div>
          </div>
        </div>

        <!-- Calendar -->
        <div
          class="overflow-hidden rounded-2xl border border-zinc-200/90 bg-white shadow-sm"
        >
          <div
            class="flex items-center justify-between border-b border-zinc-100 px-4 py-3"
          >
            <h2 class="text-sm font-semibold text-zinc-900">
              {{ t('booking.calendarTitle') }}
            </h2>
            <div class="flex gap-1.5">
              <UButton
                icon="i-heroicons-chevron-left"
                variant="ghost"
                color="neutral"
                size="sm"
                class="rounded-full"
                @click="shiftMonth(-1)"
              />
              <UButton
                icon="i-heroicons-chevron-right"
                variant="ghost"
                color="neutral"
                size="sm"
                class="rounded-full"
                @click="shiftMonth(1)"
              />
            </div>
          </div>
          <p class="px-4 pt-4 text-center text-sm font-medium text-zinc-700">
            {{ monthLabel }}
          </p>
          <div
            class="grid grid-cols-7 gap-1.5 px-3 pb-2 pt-3 text-center text-[11px] font-medium uppercase tracking-wide text-zinc-400"
          >
            <span>Mon</span><span>Tue</span><span>Wed</span><span>Thu</span
            ><span>Fri</span><span>Sat</span><span>Sun</span>
          </div>
          <div class="grid grid-cols-7 gap-1.5 px-3 pb-5">
            <template v-for="(cell, idx) in calendarCells" :key="idx">
              <div v-if="cell.label === null" class="min-h-[3.25rem]" />
              <button
                v-else
                type="button"
                :disabled="!cell.key || !slotsByDay.has(cell.key)"
                :class="dayCellClasses(cell)"
                @click="cell.key && selectDay(cell.key)"
              >
                <span>{{ cell.label }}</span>
                <span
                  v-if="cell.key && slotCountForKey(cell.key) > 0"
                  class="font-normal text-[10px] leading-none text-zinc-500"
                >
                  {{
                    t('booking.slotsForDay', { n: slotCountForKey(cell.key) })
                  }}
                </span>
              </button>
            </template>
          </div>
        </div>

        <!-- Slots -->
        <div
          class="flex flex-col overflow-hidden rounded-2xl border border-zinc-200/90 bg-white shadow-sm"
        >
          <div class="border-b border-zinc-100 px-4 py-3">
            <h2 class="text-sm font-semibold text-zinc-900">
              {{ t('booking.slotStatusTitle') }}
            </h2>
          </div>
          <div class="min-h-0 flex-1 p-4">
            <div v-if="slotsPending" class="flex justify-center py-10">
              <UIcon
                name="i-heroicons-arrow-path"
                class="size-8 animate-spin text-orange-500"
              />
            </div>
            <UAlert
              v-else-if="slotsError"
              color="error"
              variant="soft"
              :title="t('booking.loadError')"
              class="rounded-xl"
            />
            <ul
              v-else-if="slotsForSelectedDay.length"
              class="max-h-80 divide-y divide-zinc-100 overflow-auto rounded-lg border border-zinc-100"
            >
              <li v-for="slot in slotsForSelectedDay" :key="slot.startAt">
                <button
                  type="button"
                  class="flex w-full items-center justify-between gap-3 px-4 py-3.5 text-left transition hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-orange-400"
                  :class="
                    selectedSlot?.startAt === slot.startAt
                      ? 'bg-orange-50/60'
                      : ''
                  "
                  @click="selectedSlot = slot"
                >
                  <span class="text-sm font-medium text-zinc-800">
                    {{ formatTimeRange(slot.startAt, slot.endAt) }}
                  </span>
                  <span
                    class="shrink-0 rounded-md bg-zinc-100 px-2.5 py-1 text-xs font-semibold text-zinc-800"
                    :class="
                      selectedSlot?.startAt === slot.startAt
                        ? 'bg-white text-zinc-900 ring-1 ring-orange-200'
                        : ''
                    "
                  >
                    {{ t('booking.available') }}
                  </span>
                </button>
              </li>
            </ul>
            <p v-else class="py-8 text-center text-sm text-zinc-500">
              {{ t('booking.noSlotsDay') }}
            </p>
          </div>
          <div
            class="mt-auto flex flex-wrap gap-3 border-t border-zinc-100 p-4"
          >
            <UButton
              variant="outline"
              color="neutral"
              class="rounded-lg border-zinc-300"
              @click="goBackToCatalog"
            >
              {{ t('booking.back') }}
            </UButton>
            <UButton
              color="primary"
              class="rounded-lg"
              :disabled="!selectedSlot"
              @click="onContinue"
            >
              {{ t('booking.continue') }}
            </UButton>
          </div>
        </div>
      </div>
    </template>
  </UContainer>
</template>
