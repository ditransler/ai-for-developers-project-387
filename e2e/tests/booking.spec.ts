import { expect, test } from '@playwright/test'

import { E2E_EVENT_TYPE_NAME } from '../constants.js'

test.describe('guest booking flow', () => {
  test('S1: book a slot from catalog through confirm', async ({ page }) => {
    await page.goto('/booking')

    await expect(
      page.getByRole('heading', { name: 'Select an event type' })
    ).toBeVisible()

    await page.getByRole('link', { name: E2E_EVENT_TYPE_NAME }).click()

    await expect(page.getByRole('heading', { level: 1 })).toHaveText(
      E2E_EVENT_TYPE_NAME
    )

    await expect(page.getByText('Slot status')).toBeVisible()
    const firstSlot = page
      .getByRole('listitem')
      .first()
      .getByRole('button', { name: /Available/ })
    await expect(firstSlot).toBeVisible()
    await firstSlot.click()

    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(
      page.getByRole('heading', { name: 'Confirm booking' })
    ).toBeVisible()

    await page
      .getByRole('textbox', { name: 'Your name (optional)' })
      .fill('Playwright E2E')

    await page.getByRole('button', { name: 'Confirm booking' }).click()

    await expect(
      page.getByText('Booking confirmed', { exact: true }).first()
    ).toBeVisible()
    await expect(page).toHaveURL(/\/booking$/)
  })
})
