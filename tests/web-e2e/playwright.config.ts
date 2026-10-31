import { defineConfig, devices } from '@playwright/test'

/**
 * Runs against the real dev servers (both API and frontend need to
 * already be running - see tests/web-e2e/README.md) rather than
 * spinning up its own instances. This is a personal-scale project
 * with one environment to test against locally; a CI-managed
 * webServer config is more machinery than the current setup needs.
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: false, // tests share one admin account/session - avoid interleaving login state
  retries: 0,
  workers: 1,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
})
