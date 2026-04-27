/** Shared Nuxt UI `ui` props for form fields — matches admin modal controls. */
export const adminFormFieldUi = {
  label: 'text-sm font-semibold text-zinc-800',
  hint: 'text-xs font-normal text-zinc-500',
  description: 'text-xs text-zinc-500',
  error: 'text-sm font-medium text-red-600',
  container: 'mt-2',
}

const adminControlBase = [
  '!w-full !rounded-lg !border-0 !bg-zinc-50 !px-3 !py-2 !text-sm !text-zinc-900 !shadow-none',
  '!ring-1 !ring-inset !ring-zinc-300/90',
  'placeholder:!text-zinc-400',
  'hover:!bg-white',
  'focus:!bg-white focus-visible:!outline-none focus-visible:!ring-2 focus-visible:!ring-orange-400/80',
].join(' ')

export const adminFormControlUi = {
  base: `${adminControlBase} !min-h-10`,
}

export const adminFormTextareaUi = {
  base: `${adminControlBase} !min-h-[5.75rem]`,
}
