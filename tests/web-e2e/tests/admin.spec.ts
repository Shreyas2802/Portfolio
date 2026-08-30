import { test, expect } from '@playwright/test'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const TEST_PHOTO = path.join(__dirname, '..', 'fixtures', 'test-photo.jpg')

// These credentials must match whatever you set locally via
// `dotnet user-secrets set "AdminUser:Username"/"AdminUser:Password"`
// (see the main README) - never commit real credentials here.
const ADMIN_USERNAME = process.env.E2E_ADMIN_USERNAME ?? ''
const ADMIN_PASSWORD = process.env.E2E_ADMIN_PASSWORD ?? ''

test.describe('Admin flows', () => {
  test.skip(
    !ADMIN_USERNAME || !ADMIN_PASSWORD,
    'Set E2E_ADMIN_USERNAME and E2E_ADMIN_PASSWORD env vars to run admin tests (see tests/web-e2e/README.md)',
  )

  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/login')
    await page.getByLabel('Username').fill(ADMIN_USERNAME)
    await page.getByLabel('Password').fill(ADMIN_PASSWORD)
    await page.getByRole('button', { name: /sign in/i }).click()
    await expect(page).toHaveURL(/\/admin$/)
  })

  test('admin can log in and see the dashboard', async ({ page }) => {
    await expect(page.getByText('Overview')).toBeVisible()
    await expect(page.getByText('Photos')).toBeVisible()
    await expect(page.getByText('Albums')).toBeVisible()
  })

  test('admin can upload a photo and then delete it', async ({ page }) => {
    await page.goto('/admin/photos')

    const uniqueTitle = `E2E Test Photo ${Date.now()}`
    await page.setInputFiles('#photo-file-input', TEST_PHOTO)
    await page.getByLabel('Title').fill(uniqueTitle)
    await page.getByLabel('Album').selectOption({ index: 1 })
    await page.getByRole('button', { name: /^upload$/i }).click()

    // Wait for the new photo to appear in the grid.
    const uploadedCard = page.locator(`text=${uniqueTitle}`)
    await expect(uploadedCard).toBeVisible({ timeout: 15_000 })

    // Clean up: delete what this test just created, so repeated runs
    // don't pile up test photos in the admin's real database.
    const card = page.locator('div', { has: page.locator(`text=${uniqueTitle}`) }).first()
    await card.hover()
    page.once('dialog', (dialog) => dialog.accept())
    await card.getByRole('button', { name: 'Delete' }).click()
    await expect(uploadedCard).not.toBeVisible()
  })

  test('admin can log out', async ({ page }) => {
    await page.getByRole('button', { name: /log out/i }).click()
    await expect(page).toHaveURL(/\/admin\/login/)
  })
})

// The spec's testing checklist also lists "admin edits photo metadata"
// and "admin publishes photo" as E2E cases. Neither is implemented as
// a separate capability today: there is no photo-edit screen (only
// upload and delete), and upload always publishes immediately (no
// draft/unpublished state for photos specifically - only Albums and
// Projects have a publish toggle). These are real, known gaps rather
// than something to fake a passing test for - see the architecture
// decision log.
