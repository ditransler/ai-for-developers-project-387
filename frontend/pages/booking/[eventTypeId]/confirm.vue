<script setup lang="ts">
import { formatLongDate, formatTimeRange } from '~/utils/date'
import { adminFormControlUi, adminFormFieldUi } from '~/utils/nuxtFormUi'

const route = useRoute()
const { t } = useI18n()
const toast = useToast()
const api = useBookingApi()

const eventTypeId = route.params.eventTypeId as string
const startAtStr =
  typeof route.query.startAt === 'string' ? route.query.startAt : ''

if (!startAtStr) {
  throw createError({ statusCode: 400, statusMessage: 'Missing startAt' })
}

const { data: eventType } = await useAsyncData(
  `confirm-event-type-${eventTypeId}`,
  async () => {
    const list = await api.listPublicEventTypes()
    return list.find((e) => e.id === eventTypeId) ?? null
  }
)

if (eventType.value === null) {
  throw createError({ statusCode: 404, statusMessage: 'Event type not found' })
}

const guestName = ref('')
const submitting = ref(false)

const summaryDate = computed(() => formatLongDate(new Date(startAtStr)))

const { data: slots } = await useAsyncData(`confirm-slots-${eventTypeId}`, () =>
  api.listAvailableSlots(eventTypeId)
)

const matchingSlot = computed(() =>
  (slots.value ?? []).find((s) => s.startAt === startAtStr)
)

const timeLabel = computed(() => {
  if (matchingSlot.value)
    return formatTimeRange(matchingSlot.value.startAt, matchingSlot.value.endAt)
  return new Intl.DateTimeFormat('en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(startAtStr))
})

function goBackToSlots() {
  navigateTo(`/booking/${eventTypeId}`)
}

async function submit() {
  submitting.value = true
  try {
    await api.createBooking({
      eventTypeId,
      startAt: startAtStr,
      guestDisplayName: guestName.value.trim() || undefined,
    })
    toast.add({
      title: t('confirm.success'),
      color: 'success',
    })
    await navigateTo('/booking')
  } catch (e: unknown) {
    const msg = getFetchErrorMessage(e, t('errors.generic'), {
      network: t('errors.network'),
      server: t('errors.server'),
    })
    const status = (e as { statusCode?: number }).statusCode
    toast.add({
      title: status === 409 ? t('confirm.conflict') : msg,
      color: 'error',
    })
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <UContainer class="max-w-lg py-12">
    <div
      class="overflow-hidden rounded-2xl border border-zinc-200/90 bg-white shadow-sm"
    >
      <div class="space-y-2 border-b border-zinc-100 p-6">
        <h1 class="text-xl font-bold tracking-tight text-zinc-900">
          {{ t('confirm.title') }}
        </h1>
        <p class="text-sm leading-relaxed text-zinc-600">
          {{ t('confirm.subtitle') }}
        </p>
      </div>

      <div class="space-y-4 p-6">
        <div class="rounded-xl bg-[#EBF5FF] p-4 text-sm">
          <p class="font-semibold text-zinc-900">
            {{ eventType?.name }}
          </p>
          <p class="mt-2 text-zinc-700">{{ summaryDate }} · {{ timeLabel }}</p>
        </div>

        <UFormField :label="t('confirm.guestName')" :ui="adminFormFieldUi">
          <UInput
            v-model="guestName"
            variant="none"
            color="neutral"
            :placeholder="t('confirm.guestNamePlaceholder')"
            autocomplete="name"
            :ui="adminFormControlUi"
          />
        </UFormField>
      </div>

      <div
        class="flex flex-wrap justify-end gap-3 border-t border-zinc-100 p-6"
      >
        <UButton
          variant="outline"
          color="primary"
          :disabled="submitting"
          @click="goBackToSlots"
        >
          {{ t('admin.cancel') }}
        </UButton>
        <UButton color="primary" :loading="submitting" @click="submit">
          {{ t('confirm.submit') }}
        </UButton>
      </div>
    </div>
  </UContainer>
</template>
