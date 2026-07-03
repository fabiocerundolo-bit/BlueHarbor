import { test, expect } from '@playwright/test'

// Unique suffix per run to avoid conflicts with existing data
const RUN_ID = Date.now()

const SHIP_TEMPLATES = {
  xl1: { id: '1', name: 'MSC Splendida' },
  xl2: { id: '2', name: 'Costa Favolosa' },
  l1: { id: '3', name: 'Norwegian Epic' },
  l2: { id: '4', name: 'Celebrity Reflection' },
  m1: { id: '5', name: 'Queen Mary 2' },
  m2: { id: '6', name: 'Disney Dream' },
  s1: { id: '7', name: 'Seabourn Odyssey' },
  s2: { id: '8', name: 'Wind Star' },
}

test.describe('Ship Assignment Flow', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForSelector('text=BlueHarbor')
  })

  // ─── Helper: create a ship as Operator ────────────────────────────────
  async function createShip(page, template, notes = '') {
    await page.click('button:has-text("Operator")')
    await page.waitForSelector('label:has-text("Ship Template")')
    await page.selectOption('select', template.id)
    if (notes) {
      await page.fill('textarea', notes)
    }
    await page.click('button:has-text("Register Ship")')
    await page.waitForSelector('text=Ship registered successfully', { timeout: 10000 })
  }

  // ─── Helper: assign a ship as Scheduler ───────────────────────────────
  async function assignShip(page, shipName) {
    // Click the ship row and select it
    const shipRow = page.locator('tbody tr', { hasText: shipName }).first()
    await shipRow.getByText('Select →', { exact: true }).click()

    // Wait until at least one compatible berth becomes clickable
    const compatibleBerth = page.locator('[data-testid="berth-clickable"]').first()
    await expect(compatibleBerth).toBeVisible({ timeout: 5000 })
    await compatibleBerth.click()

    // Verify that the modal shows "Confirm Assignment" (not the spinner state)
    const confirmBtn = page.locator('button:has-text("Confirm Assignment")')
    await expect(confirmBtn).toBeVisible({ timeout: 3000 })
    await expect(confirmBtn).toBeEnabled()

    // Confirm
    await confirmBtn.click()

    // Wait for the success toast
    await expect(page.locator('text=Ship assigned successfully')).toBeVisible({ timeout: 10000 })
  }

  // ════════════════════════════════════════════════════════════════════════
  // TEST 1: Ship creation
  // ════════════════════════════════════════════════════════════════════════
  test('creates a ship and shows it in the list', async ({ page }) => {
    const template = SHIP_TEMPLATES.xl1

    await page.click('button:has-text("Operator")')
    await page.waitForSelector('label:has-text("Ship Template")')
    await page.selectOption('select', template.id)

    // The button should be enabled once a valid template is selected
    await expect(page.locator('button:has-text("Register Ship")')).toBeEnabled()

    await page.click('button:has-text("Register Ship")')

    // Success banner appears with the details
    await expect(page.locator('text=Ship registered successfully')).toBeVisible({ timeout: 8000 })
    await expect(page.locator('tbody tr').filter({ hasText: template.name }).first()).toBeVisible()

    // The select resets after creation
    await expect(page.locator('select')).toHaveValue('')
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 2: "Register Ship" button disabled with no selection
  // ════════════════════════════════════════════════════════════════════════
  test('register ship button is disabled without a template', async ({ page }) => {
    await page.click('button:has-text("Operator")')
    await page.waitForSelector('label:has-text("Ship Template")')

    const btn = page.locator('button:has-text("Register Ship")')
    await expect(btn).toBeDisabled()

    await page.selectOption('select', '')
    await expect(btn).toBeDisabled()
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 3: Operator ↔ Scheduler role switch
  // ════════════════════════════════════════════════════════════════════════
  test('role switch shows the correct view', async ({ page }) => {
    // Default: Operator
    await expect(page.locator('text=Register New Ship')).toBeVisible()
    await expect(page.locator('text=Pending Ships')).not.toBeVisible()

    // Switch to Scheduler
    await page.click('button:has-text("Scheduler")')
    await expect(page.getByRole('heading', { name: 'Pending Ships' })).toBeVisible({ timeout: 5000 })
    await expect(page.getByRole('heading', { name: 'Berth Schedule' })).toBeVisible()
    await expect(page.locator('text=Register New Ship')).not.toBeVisible()

    // Switch back
    await page.click('button:has-text("Operator")')
    await expect(page.getByRole('heading', { name: 'Register New Ship' })).toBeVisible({ timeout: 3000 })
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 4: Assign the first ship - base flow
  // ════════════════════════════════════════════════════════════════════════
  test('assigns a ship to a berth', async ({ page }) => {
    const template = SHIP_TEMPLATES.xl1
    await createShip(page, template)

    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector('text=Pending Ships', { timeout: 8000 })
    await page.waitForSelector(`text=${template.name}`, { timeout: 5000 })

    await assignShip(page, template.name)
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 5: REGRESSION BUG - The second ship must not get stuck
  // Reproduces the reported bug: assigning 2 ships in sequence caused
  // the second one to open the modal already in the spinner state (loading=true
  // never reset by the success path of the first assignment).
  // ════════════════════════════════════════════════════════════════════════
  test('assigns two ships in sequence without an infinite block', async ({ page }) => {
    const template1 = SHIP_TEMPLATES.xl1
    const template2 = SHIP_TEMPLATES.xl2

    // Create both ships as Operator
    await createShip(page, template1)
    await createShip(page, template2)

    // Switch to Scheduler
    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector('text=Pending Ships', { timeout: 8000 })
    await page.waitForSelector(`text=${template1.name}`, { timeout: 5000 })

    // First assignment
    await assignShip(page, template1.name)

    // Wait for the toast to disappear (closed manually or by timeout)
    await page.waitForTimeout(500)

    // Second assignment
    // The second ship must still be in the list
    await page.waitForSelector(`text=${template2.name}`, { timeout: 5000 })

    // Select ship 2
    const row2 = page.locator('tbody tr', { hasText: template2.name }).first()
    await row2.getByText('Select →', { exact: true }).click()

    // Click the first compatible berth
    const compatibleBerth = page.locator('[data-testid="berth-clickable"]').first()
    await expect(compatibleBerth).toBeVisible({ timeout: 5000 })
    await compatibleBerth.click()

    // Regression check
    // The modal must show "Confirm Assignment" (NOT the spinner state)
    // Before the fix, this failed because loading=true persisted from the previous modal
    const confirmBtn = page.locator('button:has-text("Confirm Assignment")')
    await expect(confirmBtn).toBeVisible({ timeout: 3000 })
    await expect(confirmBtn).toBeEnabled()
    await expect(page.locator('text=Assigning...')).not.toBeVisible()

    // Complete the second assignment
    await confirmBtn.click()
    await expect(page.locator('text=Ship assigned successfully')).toBeVisible({ timeout: 10000 })
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 6: The schedule grid shows the assigned ship in the timeline
  // ════════════════════════════════════════════════════════════════════════
  test('the berth grid reflects the assigned ship', async ({ page }) => {
    const template = SHIP_TEMPLATES.xl1
    await createShip(page, template)

    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector(`text=${template.name}`, { timeout: 6000 })
    await assignShip(page, template.name)

    // The ship name must appear in the berth grid
    await expect(page.locator('.card').filter({ hasText: 'Berth Schedule' }).locator(`text=${template.name}`).first())
      .toBeVisible({ timeout: 5000 })
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 7: No compatible berth is selectable when no ship is selected
  // (berths must not have data-testid="berth-clickable")
  // ════════════════════════════════════════════════════════════════════════
  test('berths are not clickable without a selected ship', async ({ page }) => {
    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector('text=Berth Schedule', { timeout: 6000 })

    // No berth should have data-testid berth-clickable if no ship is selected
    await expect(page.locator('[data-testid="berth-clickable"]')).toHaveCount(0)
  })

})
