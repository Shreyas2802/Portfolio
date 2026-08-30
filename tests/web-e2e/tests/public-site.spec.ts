import { test, expect } from '@playwright/test'

test.describe('Public site - visitor flows', () => {
  test('homepage loads with hero and featured work', async ({ page }) => {
    await page.goto('/')
    await expect(page.getByRole('heading', { name: /I build software/i })).toBeVisible()
    await expect(page.getByRole('link', { name: 'View Photography' })).toBeVisible()
  })

  test('visitor can browse the gallery and open the lightbox', async ({ page }) => {
    await page.goto('/work')
    await expect(page.getByRole('heading', { name: 'Photography' })).toBeVisible()

    // Wait for at least one photo to render, then open it.
    const firstPhoto = page.locator('button:has(img)').first()
    await firstPhoto.waitFor({ state: 'visible' })
    await firstPhoto.click()

    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByRole('button', { name: /close/i })).toBeVisible()

    await page.keyboard.press('Escape')
    await expect(page.getByRole('dialog')).not.toBeVisible()
  })

  test('visitor can view a project detail page', async ({ page }) => {
    await page.goto('/projects')
    await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()

    await page.getByRole('link', { name: /view case study/i }).first().click()
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible()
    await expect(page.getByText('Overview')).toBeVisible()
  })

  test('visitor can submit the contact form', async ({ page }) => {
    await page.goto('/contact')

    await page.getByLabel('Name').fill('Playwright E2E Test')
    await page.getByLabel('Email').fill(`e2e-test-${Date.now()}@example.com`)
    await page.getByLabel('Message').fill('This is an automated end-to-end test submission.')
    await page.getByRole('button', { name: /send message/i }).click()

    await expect(page.getByText(/thanks — your message has been sent/i)).toBeVisible()
  })

  test('unauthenticated visitor is redirected away from the admin panel', async ({ page }) => {
    await page.goto('/admin/photos')
    await expect(page).toHaveURL(/\/admin\/login/)
  })
})
