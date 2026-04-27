declare module 'nuxt/schema' {
  interface AppConfigInput {
    calendarHost?: {
      displayName: string
      roleKey: string
      avatarSrc: string
    }
  }
}

export {}
