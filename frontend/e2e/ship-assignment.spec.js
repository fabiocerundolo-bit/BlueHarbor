import { test, expect } from '@playwright/test'

// Unique suffix per run to avoid conflicts with existing data
const RUN_ID = Date.now()

test.describe('Ship Assignment Flow', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForSelector('text=BlueHarbor')
  })

  // ─── Helper: crea una nave come Operatore ──────────────────────────────
  async function createShip(page, name) {
    await page.click('button:has-text("Operatore")')
    await page.waitForSelector('[placeholder="es. MS Adriatic Star"]')
    await page.fill('[placeholder="es. MS Adriatic Star"]', name)
    await page.click('button:has-text("Registra Nave")')
    await page.waitForSelector('text=Nave registrata con successo', { timeout: 10000 })
  }

  // ─── Helper: assegna una nave come Scheduler ───────────────────────────
  async function assignShip(page, shipName) {
    // Clicca sulla riga della nave e selezionala
    const shipRow = page.locator('tr', { hasText: shipName })
    await shipRow.locator('text=Seleziona →').click()

    // Attende che almeno una banchina compatibile sia cliccabile
    const compatibleBerth = page.locator('[data-testid="berth-clickable"]').first()
    await expect(compatibleBerth).toBeVisible({ timeout: 5000 })
    await compatibleBerth.click()

    // Verifica che il modal mostri "Conferma Assegnazione" (non spinner)
    const confirmBtn = page.locator('button:has-text("Conferma Assegnazione")')
    await expect(confirmBtn).toBeVisible({ timeout: 3000 })
    await expect(confirmBtn).toBeEnabled()

    // Conferma
    await confirmBtn.click()

    // Attende il toast di successo
    await expect(page.locator('text=Nave assegnata con successo')).toBeVisible({ timeout: 10000 })
  }

  // ════════════════════════════════════════════════════════════════════════
  // TEST 1: Creazione nave
  // ════════════════════════════════════════════════════════════════════════
  test('crea una nave e la mostra in lista', async ({ page }) => {
    const name = `TestNave-${RUN_ID}`

    await page.click('button:has-text("Operatore")')
    await page.waitForSelector('[placeholder="es. MS Adriatic Star"]')
    await page.fill('[placeholder="es. MS Adriatic Star"]', name)

    // Il bottone deve essere abilitato con testo valido
    await expect(page.locator('button:has-text("Registra Nave")')).toBeEnabled()

    await page.click('button:has-text("Registra Nave")')

    // Comparsa del banner di successo con i dettagli
    await expect(page.locator('text=Nave registrata con successo')).toBeVisible({ timeout: 8000 })
    await expect(page.locator(`text=${name}`)).toBeVisible()

    // Il campo si è svuotato
    await expect(page.locator('[placeholder="es. MS Adriatic Star"]')).toHaveValue('')
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 2: Bottone "Registra Nave" disabilitato con campo vuoto
  // ════════════════════════════════════════════════════════════════════════
  test('bottone registra nave disabilitato se nome vuoto', async ({ page }) => {
    await page.click('button:has-text("Operatore")')
    await page.waitForSelector('[placeholder="es. MS Adriatic Star"]')

    const btn = page.locator('button:has-text("Registra Nave")')
    await expect(btn).toBeDisabled()

    await page.fill('[placeholder="es. MS Adriatic Star"]', '   ')
    await expect(btn).toBeDisabled()

    await page.fill('[placeholder="es. MS Adriatic Star"]', 'A')
    await expect(btn).toBeEnabled()
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 3: Switch ruolo Operatore ↔ Scheduler
  // ════════════════════════════════════════════════════════════════════════
  test('switch di ruolo mostra la view corretta', async ({ page }) => {
    // Default: Operatore
    await expect(page.locator('text=Registra Nuova Nave')).toBeVisible()
    await expect(page.locator('text=Navi in Attesa')).not.toBeVisible()

    // Switch a Scheduler
    await page.click('button:has-text("Scheduler")')
    await expect(page.locator('text=Navi in Attesa')).toBeVisible({ timeout: 5000 })
    await expect(page.locator('text=Tabellone Banchine')).toBeVisible()
    await expect(page.locator('text=Registra Nuova Nave')).not.toBeVisible()

    // Switch back
    await page.click('button:has-text("Operatore")')
    await expect(page.locator('text=Registra Nuova Nave')).toBeVisible({ timeout: 3000 })
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 4: Assegna prima nave — flusso base
  // ════════════════════════════════════════════════════════════════════════
  test('assegna una nave a una banchina', async ({ page }) => {
    const name = `AlphaTest-${RUN_ID}`
    await createShip(page, name)

    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector('text=Navi in Attesa', { timeout: 8000 })
    await page.waitForSelector(`text=${name}`, { timeout: 5000 })

    await assignShip(page, name)
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 5: REGRESSION BUG — La seconda nave non deve bloccarsi
  // Riproduce il bug segnalato: assegnare 2 navi in sequenza causava
  // che la seconda aprisse il modal già con lo spinner attivo (loading=true
  // mai resettato dal success path della prima assegnazione).
  // ════════════════════════════════════════════════════════════════════════
  test('assegna due navi in sequenza senza blocco infinito', async ({ page }) => {
    const name1 = `Ship1-${RUN_ID}`
    const name2 = `Ship2-${RUN_ID}`

    // Crea entrambe le navi come Operatore
    await createShip(page, name1)
    await createShip(page, name2)

    // Passa allo Scheduler
    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector('text=Navi in Attesa', { timeout: 8000 })
    await page.waitForSelector(`text=${name1}`, { timeout: 5000 })

    // ── Prima assegnazione ──
    await assignShip(page, name1)

    // Attende che il toast sparisca (chiuso manualmente o timeout)
    await page.waitForTimeout(500)

    // ── Seconda assegnazione ──
    // La seconda nave deve essere ancora in lista
    await page.waitForSelector(`text=${name2}`, { timeout: 5000 })

    // Seleziona nave2
    const row2 = page.locator('tr', { hasText: name2 })
    await row2.locator('text=Seleziona →').click()

    // Clicca prima banchina compatibile
    const compatibleBerth = page.locator('[data-testid="berth-clickable"]').first()
    await expect(compatibleBerth).toBeVisible({ timeout: 5000 })
    await compatibleBerth.click()

    // ── VERIFICA REGRESSIONE BUG ──
    // Il modal deve mostrare "Conferma Assegnazione" (NON "Assegnazione..." con spinner)
    // Prima del fix, questo falliva perché loading=true persisteva dal modal precedente
    const confirmBtn = page.locator('button:has-text("Conferma Assegnazione")')
    await expect(confirmBtn).toBeVisible({ timeout: 3000 })
    await expect(confirmBtn).toBeEnabled()
    await expect(page.locator('text=Assegnazione...')).not.toBeVisible()

    // Completa la seconda assegnazione
    await confirmBtn.click()
    await expect(page.locator('text=Nave assegnata con successo')).toBeVisible({ timeout: 10000 })
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 6: Il tabellone mostra la nave assegnata nella timeline
  // ════════════════════════════════════════════════════════════════════════
  test('il tabellone banchine riflette la nave assegnata', async ({ page }) => {
    const name = `GridTest-${RUN_ID}`
    await createShip(page, name)

    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector(`text=${name}`, { timeout: 6000 })
    await assignShip(page, name)

    // Il nome della nave deve comparire nella griglia banchine
    await expect(page.locator('.card').filter({ hasText: 'Tabellone Banchine' }).locator(`text=${name}`))
      .toBeVisible({ timeout: 5000 })
  })

  // ════════════════════════════════════════════════════════════════════════
  // TEST 7: Nessuna banchina compatibile è selezionabile se nessuna nave è
  // selezionata (le banchine non devono avere data-testid="berth-clickable")
  // ════════════════════════════════════════════════════════════════════════
  test('le banchine non sono cliccabili senza una nave selezionata', async ({ page }) => {
    await page.click('button:has-text("Scheduler")')
    await page.waitForSelector('text=Tabellone Banchine', { timeout: 6000 })

    // Nessuna banchina deve avere data-testid berth-clickable se non è selezionata una nave
    await expect(page.locator('[data-testid="berth-clickable"]')).toHaveCount(0)
  })

})
