<script setup lang="ts">
const { t } = useI18n()
</script>

<template>
  <div class="landing-bg flex w-full flex-1 flex-col">
    <UContainer class="flex flex-1 flex-col py-12 lg:py-20">
      <div class="grid gap-12 lg:grid-cols-2 lg:items-center lg:gap-16">
        <div class="space-y-6">
          <p
            class="inline-flex rounded-full border border-zinc-200 bg-white px-3.5 py-1.5 text-xs font-medium uppercase tracking-wide text-zinc-600"
          >
            {{ t('landing.badge') }}
          </p>
          <h1
            class="text-4xl font-bold tracking-tight text-zinc-950 sm:text-5xl"
          >
            {{ t('landing.title') }}
          </h1>
          <p class="max-w-xl text-lg leading-relaxed text-zinc-600">
            {{ t('landing.subtitle') }}
          </p>
          <div>
            <UButton
              to="/booking"
              size="xl"
              color="primary"
              trailing-icon="i-heroicons-arrow-right"
              class="rounded-xl !text-white"
              :ui="{
                base: 'px-8 py-3.5 text-lg font-semibold shadow-sm gap-2',
                trailingIcon: 'size-4',
              }"
            >
              {{ t('landing.cta') }}
            </UButton>
          </div>
        </div>
        <UCard
          class="rounded-3xl bg-white shadow-md ring-1 ring-zinc-200/70"
          :ui="{ body: 'p-8 sm:p-8' }"
        >
          <h2 class="mb-6 text-xl font-semibold text-zinc-900">
            {{ t('landing.featuresTitle') }}
          </h2>
          <ul
            class="list-disc space-y-4 pl-5 text-zinc-600 marker:text-zinc-300"
          >
            <li class="pl-1">{{ t('landing.feature1') }}</li>
            <li class="pl-1">{{ t('landing.feature2') }}</li>
            <li class="pl-1">{{ t('landing.feature3') }}</li>
          </ul>
        </UCard>
      </div>
    </UContainer>
  </div>
</template>

<style scoped>
.landing-bg {
  min-height: 100%;
  background:
    radial-gradient(
      1200px circle at 10% -10%,
      rgb(255 237 213),
      transparent 55%
    ),
    radial-gradient(900px circle at 92% 2%, rgb(224 242 254), transparent 52%),
    linear-gradient(
      to bottom,
      rgb(252 252 252),
      rgb(248 247 252),
      rgb(245 245 247)
    );
  background-repeat: no-repeat;
  background-size: 100% 100%;
}
</style>
