// https://nuxt.com/docs/api/configuration/nuxt-config
/// <reference types="node" />
export default defineNuxtConfig({
  compatibilityDate: '2025-04-01',
  devtools: { enabled: true },
  modules: ['@nuxt/eslint', '@nuxt/ui', '@nuxtjs/i18n'],
  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    public: {
      apiBase:
        process.env.NUXT_PUBLIC_API_BASE_URL ?? 'http://localhost:4010',
    },
  },
  i18n: {
    defaultLocale: 'en',
    langDir: 'locales',
    locales: [{ code: 'en', name: 'English', file: 'en.json' }],
    strategy: 'no_prefix',
    bundle: {
      optimizeTranslationDirective: false,
    },
  },
})
