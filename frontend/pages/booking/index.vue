<script setup lang="ts">
const { t } = useI18n()
const appConfig = useAppConfig()
const api = useBookingApi()

const hostInitial = computed(() => {
  const n = appConfig.calendarHost.displayName.trim()
  return n ? n.charAt(0).toUpperCase() : '?'
})

const {
  data: eventTypes,
  pending,
  error,
} = await useAsyncData('public-event-types', () => api.listPublicEventTypes())
</script>

<template>
  <UContainer class="py-10">
    <div
      class="mb-10 rounded-2xl border border-zinc-200/90 bg-white px-8 py-9 shadow-sm sm:px-10 sm:py-10"
    >
      <!-- Host row, then title + hint: 8 / 24 / 12 on the 4px grid -->
      <div class="flex flex-col gap-8">
        <div class="flex items-center gap-4 sm:gap-5">
          <div
            class="flex size-[4.5rem] shrink-0 items-center justify-center rounded-full bg-orange-500 text-2xl font-semibold text-white shadow-md sm:size-20 sm:text-[1.75rem]"
            aria-hidden="true"
          >
            {{ hostInitial }}
          </div>
          <div class="flex min-w-0 flex-col gap-1.5">
            <p
              class="text-base font-semibold leading-snug text-zinc-800 sm:text-lg"
            >
              {{ appConfig.calendarHost.displayName }}
            </p>
            <p
              class="text-[11px] font-medium uppercase leading-snug tracking-[0.12em] text-zinc-400"
            >
              {{ t('booking.host') }}
            </p>
          </div>
        </div>
        <div class="flex flex-col gap-3">
          <h1
            class="text-2xl font-bold leading-tight tracking-tight text-zinc-900 sm:text-[1.75rem] sm:leading-snug"
          >
            {{ t('booking.selectTypeTitle') }}
          </h1>
          <p class="max-w-2xl text-base leading-relaxed text-zinc-600">
            {{ t('booking.selectTypeHint') }}
          </p>
        </div>
      </div>
    </div>

    <UAlert
      v-if="error"
      color="error"
      variant="soft"
      :title="t('errors.generic')"
      class="mb-6 rounded-xl"
    />

    <div v-if="pending" class="flex justify-center py-16">
      <UIcon
        name="i-heroicons-arrow-path"
        class="size-10 animate-spin text-orange-500"
      />
    </div>

    <div v-else class="grid gap-5 sm:grid-cols-2 sm:gap-6">
      <NuxtLink
        v-for="et in eventTypes"
        :key="et.id"
        :to="`/booking/${et.id}`"
        class="group block focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 focus-visible:ring-offset-2 rounded-2xl"
      >
        <div
          class="relative h-full rounded-2xl border border-zinc-200/90 bg-white p-6 pt-14 shadow-sm transition duration-200 group-hover:border-orange-200 group-hover:shadow-md"
        >
          <span
            class="absolute right-5 top-5 inline-flex items-center rounded-full bg-zinc-100 px-3 py-1 text-xs font-medium text-zinc-700"
          >
            {{ t('booking.durationMin', { n: et.durationMinutes }) }}
          </span>
          <h2 class="text-lg font-semibold text-zinc-900">
            {{ et.name }}
          </h2>
          <p class="mt-3 text-sm leading-relaxed text-zinc-600">
            {{ et.description }}
          </p>
        </div>
      </NuxtLink>
    </div>
  </UContainer>
</template>
