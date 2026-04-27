<script setup lang="ts">
const route = useRoute()
const { t } = useI18n()

const bookActive = computed(() => route.path.startsWith('/booking'))
const adminActive = computed(() => route.path.startsWith('/admin'))
</script>

<template>
  <header class="border-b border-zinc-200/90 bg-white">
    <UContainer class="flex flex-wrap items-center justify-between gap-4 py-4">
      <NuxtLink
        to="/"
        class="flex items-center gap-3 text-zinc-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 focus-visible:ring-offset-2"
      >
        <span
          class="flex size-9 shrink-0 items-center justify-center rounded-lg bg-orange-500 shadow-sm"
          aria-hidden="true"
        >
          <svg
            class="size-5 text-white"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <rect
              x="3.5"
              y="5"
              width="17"
              height="15.5"
              rx="2"
              stroke="currentColor"
              stroke-width="1.75"
            />
            <circle
              cx="12"
              cy="13.5"
              r="3.25"
              stroke="currentColor"
              stroke-width="1.75"
            />
            <line
              x1="12"
              y1="13.5"
              x2="12"
              y2="10.25"
              stroke="currentColor"
              stroke-width="1.75"
              stroke-linecap="round"
            />
          </svg>
        </span>
        <span class="text-base font-bold tracking-tight">{{
          t('nav.brand')
        }}</span>
      </NuxtLink>
      <nav
        class="flex items-center gap-2 sm:gap-3 text-sm font-medium"
        aria-label="Main"
      >
        <NuxtLink
          to="/booking"
          class="rounded-full px-4 py-2 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 focus-visible:ring-offset-2"
          :class="
            bookActive
              ? 'bg-sky-100 text-zinc-900 ring-2 ring-orange-500 ring-offset-0'
              : 'text-zinc-600 hover:bg-zinc-100 hover:text-zinc-900'
          "
        >
          {{ t('nav.book') }}
        </NuxtLink>
        <NuxtLink
          to="/admin"
          class="rounded-full px-2 py-2 text-zinc-600 transition-colors hover:text-zinc-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 focus-visible:ring-offset-2"
          :class="adminActive ? 'font-semibold text-zinc-900' : ''"
        >
          {{ t('nav.admin') }}
        </NuxtLink>
      </nav>
    </UContainer>
  </header>
</template>
