<script setup lang="ts">
const { t } = useI18n()
const api = useBookingApi()

const {
  data: bookings,
  pending,
  error,
  refresh,
} = await useAsyncData('admin-bookings', () => api.listAdminBookings())

const loadErrorMessage = computed(() =>
  error.value
    ? getFetchErrorMessage(error.value, t('errors.generic'), {
        network: t('errors.network'),
        server: t('errors.server'),
      })
    : ''
)

function formatWhen(iso: string) {
  return new Intl.DateTimeFormat('en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(iso))
}
</script>

<template>
  <UContainer class="py-10">
    <div class="mb-8 flex flex-wrap items-center justify-between gap-4">
      <h1 class="text-2xl font-bold text-zinc-900">
        {{ t('admin.bookingsTitle') }}
      </h1>
      <UButton to="/admin/event-types" color="primary" variant="outline">
        {{ t('admin.eventTypesTitle') }}
      </UButton>
    </div>

    <UAlert
      v-if="error"
      color="error"
      variant="soft"
      :title="loadErrorMessage"
      class="mb-6"
    >
      <template #description>
        <p class="text-sm">
          {{ t('errors.retryHint') }}
        </p>
      </template>
      <template #actions>
        <UButton
          size="xs"
          color="neutral"
          variant="outline"
          @click="() => refresh()"
        >
          {{ t('admin.retry') }}
        </UButton>
      </template>
    </UAlert>

    <div v-if="pending" class="flex justify-center py-16">
      <UIcon
        name="i-heroicons-arrow-path"
        class="size-10 animate-spin text-orange-500"
      />
    </div>

    <template v-else-if="!error">
      <UCard
        v-if="bookings?.length"
        :ui="{
          root: 'rounded-lg !bg-white text-zinc-900 ring-1 ring-zinc-200/80 shadow-sm overflow-hidden',
          body: 'divide-y divide-zinc-100 !p-0',
        }"
      >
        <div
          v-for="b in bookings"
          :key="b.id"
          class="flex flex-col gap-1 px-6 py-4 sm:flex-row sm:items-center sm:justify-between"
        >
          <div>
            <p class="font-medium text-zinc-900">
              {{ b.eventTypeName }}
            </p>
            <p class="text-sm text-zinc-600">
              {{ formatWhen(b.startAt) }} — {{ formatWhen(b.endAt) }}
            </p>
            <p v-if="b.guestDisplayName" class="text-sm text-zinc-500">
              {{ b.guestDisplayName }}
            </p>
          </div>
        </div>
      </UCard>

      <p v-else class="py-12 text-center text-zinc-500">
        {{ t('admin.bookingsEmpty') }}
      </p>
    </template>
  </UContainer>
</template>
