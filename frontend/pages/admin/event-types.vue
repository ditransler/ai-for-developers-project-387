<script setup lang="ts">
import {
  adminFormControlUi,
  adminFormFieldUi,
  adminFormTextareaUi,
} from '~/utils/nuxtFormUi'
import type { components } from '~/types/api.generated'

type EventType = components['schemas']['EventType']
type EventTypeCreate = components['schemas']['EventTypeCreate']
type EventTypeUpdate = components['schemas']['EventTypeUpdate']
type EventDurationMinutes = components['schemas']['EventDurationMinutes']

function toAllowedDuration(n: unknown): EventDurationMinutes | null {
  const d = Math.floor(Number(n))
  if (d === 15 || d === 30 || d === 45 || d === 60) return d
  return null
}

const { t } = useI18n()
const toast = useToast()
const api = useBookingApi()

const {
  data: items,
  pending,
  error,
  refresh,
} = await useAsyncData('admin-event-types', () => api.listAdminEventTypes())

const loadErrorMessage = computed(() =>
  error.value
    ? getFetchErrorMessage(error.value, t('errors.generic'), {
        network: t('errors.network'),
        server: t('errors.server'),
      })
    : ''
)

const modalOpen = ref(false)
const editing = ref<EventType | null>(null)
const deleteOpen = ref(false)
const deleteTarget = ref<EventType | null>(null)
const saving = ref(false)

const form = reactive({
  name: '',
  description: '',
  durationMinutes: 30,
})

const nameError = ref<string | boolean>(false)
const durationError = ref<string | boolean>(false)

const modalUi = {
  content:
    '!bg-white !text-zinc-900 !ring-zinc-200/80 divide-zinc-200/60 sm:max-w-lg',
  title: 'text-zinc-900',
  description: 'text-zinc-600',
  footer: 'justify-end border-t border-zinc-100 bg-white',
  body: '!bg-white',
  header: 'border-b border-zinc-100 bg-white',
}

const modalFieldUi = adminFormFieldUi
const modalControlUi = adminFormControlUi
const modalTextareaUi = adminFormTextareaUi

const DURATION_STEP = 15
const DURATION_MAX = 60

const listCardUi = {
  root: 'rounded-lg !bg-white text-zinc-900 ring-1 ring-zinc-200/80 shadow-sm',
  body: 'flex flex-col gap-4 !p-6 sm:flex-row sm:items-center sm:justify-between sm:gap-6',
}

const emptyCardUi = {
  root: 'rounded-lg !bg-white text-zinc-900 ring-1 ring-zinc-200/80 shadow-sm',
  body: 'flex flex-col items-center gap-4 !p-10 text-center sm:!p-12',
}

function roundDurationToStep(minutes: number) {
  if (!Number.isFinite(minutes) || minutes <= 0) return DURATION_STEP
  const rounded = Math.round(minutes / DURATION_STEP) * DURATION_STEP
  return Math.min(DURATION_MAX, Math.max(DURATION_STEP, rounded))
}

function clearFieldErrors() {
  nameError.value = false
  durationError.value = false
}

function openCreate() {
  editing.value = null
  form.name = ''
  form.description = ''
  form.durationMinutes = 30
  clearFieldErrors()
  modalOpen.value = true
}

function openEdit(et: EventType) {
  editing.value = et
  form.name = et.name
  form.description = et.description
  form.durationMinutes = roundDurationToStep(et.durationMinutes)
  clearFieldErrors()
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
}

function validateForm(): EventDurationMinutes | null {
  clearFieldErrors()
  const name = form.name.trim()
  if (!name) {
    nameError.value = t('admin.validationName')
    return null
  }
  const durationMinutes = toAllowedDuration(form.durationMinutes)
  if (durationMinutes === null) {
    durationError.value = t('admin.validationDurationStep')
    return null
  }
  return durationMinutes
}

async function save() {
  const durationMinutes = validateForm()
  if (!durationMinutes) return
  saving.value = true
  try {
    if (editing.value) {
      const body: EventTypeUpdate = {
        name: form.name.trim(),
        description: form.description.trim(),
        durationMinutes,
      }
      await api.updateEventType(editing.value.id, body)
      toast.add({ title: t('admin.save'), color: 'success' })
    } else {
      const body: EventTypeCreate = {
        id: crypto.randomUUID(),
        name: form.name.trim(),
        description: form.description.trim(),
        durationMinutes,
      }
      await api.createEventType(body)
      toast.add({ title: t('admin.create'), color: 'success' })
    }
    closeModal()
    await refresh()
  } catch (e: unknown) {
    toast.add({
      title: getFetchErrorMessage(e, t('errors.generic'), {
        network: t('errors.network'),
        server: t('errors.server'),
      }),
      color: 'error',
    })
  } finally {
    saving.value = false
  }
}

function openDelete(et: EventType) {
  deleteTarget.value = et
  deleteOpen.value = true
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  saving.value = true
  try {
    await api.deleteEventType(deleteTarget.value.id)
    toast.add({ title: t('admin.delete'), color: 'success' })
    deleteOpen.value = false
    deleteTarget.value = null
    await refresh()
  } catch (e: unknown) {
    const msg = getFetchErrorMessage(e, t('errors.generic'), {
      network: t('errors.network'),
      server: t('errors.server'),
    })
    const status = (e as { statusCode?: number }).statusCode
    toast.add({
      title: status === 409 ? t('admin.deleteBlocked') : msg,
      color: 'error',
    })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <UContainer class="py-10">
    <div class="mb-8 flex flex-wrap items-center justify-between gap-4">
      <h1 class="text-2xl font-bold text-zinc-900">
        {{ t('admin.eventTypesTitle') }}
      </h1>
      <div class="flex gap-2">
        <UButton to="/admin" variant="outline" color="primary">
          {{ t('admin.bookingsTitle') }}
        </UButton>
        <UButton color="primary" @click="openCreate">
          {{ t('admin.create') }}
        </UButton>
      </div>
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
      <div v-if="items?.length" class="space-y-4">
        <UCard v-for="et in items" :key="et.id" :ui="listCardUi">
          <div class="min-w-0 flex-1">
            <p class="font-semibold text-zinc-900">
              {{ et.name }}
            </p>
            <p class="mt-1 text-sm text-zinc-600">
              {{ et.description }}
            </p>
            <p class="mt-2 text-xs text-zinc-400">
              {{ et.id }} · {{ et.durationMinutes }} min
            </p>
          </div>
          <div
            class="flex w-full shrink-0 flex-col gap-2 sm:w-auto sm:flex-row sm:justify-end"
          >
            <UButton
              class="justify-center"
              variant="outline"
              color="primary"
              @click="openEdit(et)"
            >
              {{ t('admin.edit') }}
            </UButton>
            <UButton
              class="justify-center"
              color="error"
              variant="soft"
              @click="openDelete(et)"
            >
              {{ t('admin.delete') }}
            </UButton>
          </div>
        </UCard>
      </div>

      <UCard v-else :ui="emptyCardUi">
        <p class="text-zinc-900 font-medium">
          {{ t('admin.eventTypesEmpty') }}
        </p>
        <p class="max-w-md text-sm text-zinc-600">
          {{ t('admin.eventTypesEmptyHint') }}
        </p>
        <UButton color="primary" @click="openCreate">
          {{ t('admin.create') }}
        </UButton>
      </UCard>
    </template>

    <UModal
      v-model:open="modalOpen"
      :title="editing ? t('admin.editTitle') : t('admin.createTitle')"
      :ui="modalUi"
    >
      <template #body>
        <div class="space-y-5">
          <UFormField
            :label="t('admin.name')"
            :error="nameError"
            required
            :ui="modalFieldUi"
          >
            <UInput
              v-model="form.name"
              variant="none"
              color="neutral"
              :placeholder="t('admin.namePlaceholder')"
              autocomplete="off"
              :ui="modalControlUi"
            />
          </UFormField>
          <UFormField :label="t('admin.description')" :ui="modalFieldUi">
            <UTextarea
              v-model="form.description"
              variant="none"
              color="neutral"
              :placeholder="t('admin.descriptionPlaceholder')"
              :rows="3"
              autoresize
              :ui="modalTextareaUi"
            />
          </UFormField>
          <UFormField
            :label="t('admin.duration')"
            :error="durationError"
            required
            :ui="modalFieldUi"
          >
            <UInput
              v-model.number="form.durationMinutes"
              variant="none"
              color="neutral"
              type="number"
              :min="DURATION_STEP"
              :max="DURATION_MAX"
              :step="DURATION_STEP"
              :ui="modalControlUi"
            />
          </UFormField>
        </div>
      </template>
      <template #footer>
        <UButton variant="outline" color="primary" @click="closeModal">
          {{ t('admin.cancel') }}
        </UButton>
        <UButton color="primary" :loading="saving" @click="save">
          {{ t('admin.save') }}
        </UButton>
      </template>
    </UModal>

    <UModal
      v-model:open="deleteOpen"
      :title="t('admin.deleteConfirm')"
      :ui="modalUi"
    >
      <template #body>
        <p v-if="deleteTarget" class="text-sm text-zinc-600">
          {{ deleteTarget.name }}
        </p>
      </template>
      <template #footer>
        <UButton variant="outline" color="primary" @click="deleteOpen = false">
          {{ t('admin.cancel') }}
        </UButton>
        <UButton color="error" :loading="saving" @click="confirmDelete">
          {{ t('admin.delete') }}
        </UButton>
      </template>
    </UModal>
  </UContainer>
</template>
