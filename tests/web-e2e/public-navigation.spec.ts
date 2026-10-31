import { expect, test } from '@playwright/test'

test('a visitor can browse the public portfolio', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'I build software. I frame moments.' })).toBeVisible()

  await page.getByRole('link', { name: 'Photography' }).click()
  await expect(page).toHaveURL(/\/work$/)
  await expect(page.getByRole('heading', { name: 'Photography' })).toBeVisible()

  await page.getByRole('link', { name: 'Projects' }).click()
  await expect(page).toHaveURL(/\/projects$/)
  await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()
})
