<template>
  <div
    class="flex min-h-screen flex-col bg-[#F4F7FB] text-zinc-900 antialiased"
  >
    <AppHeader />
    <main class="flex min-h-0 flex-1 flex-col">
      <slot />
    </main>
  </div>
</template>
