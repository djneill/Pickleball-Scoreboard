import { test, expect } from "@playwright/test";

// Helper function to register and start a game
async function setupGame(page: any) {
  const timestamp = Date.now();
  const testEmail = `test-${timestamp}@playwright.test`;
  const testPassword = "TestPassword123!";

  // Register
  await page.goto("http://localhost:5173/auth");
  await page.getByRole("button", { name: /register/i }).click();
  await page.getByLabel(/email/i).fill(testEmail);
  await page
    .getByLabel(/^password$/i)
    .first()
    .fill(testPassword);
  await page.getByLabel(/confirm password/i).fill(testPassword);

  await page
    .locator("form")
    .getByRole("button", { name: /create account/i })
    .click();

  await expect(page.getByRole("button", { name: /singles/i })).toBeVisible({
    timeout: 10000,
  });
  await page.getByRole("button", { name: /singles/i }).click();

  await page.waitForURL("http://localhost:5173/", { timeout: 10000 });

  await expect(page.getByTestId("You-score")).toBeVisible({ timeout: 10000 });
  await expect(page.getByTestId("Opponent-score")).toBeVisible({
    timeout: 10000,
  });

  await page.waitForTimeout(1000);
}

// Helper to get current scores
async function getScores(page: any) {
  await expect(page.getByTestId("You-score")).toBeVisible();
  await expect(page.getByTestId("Opponent-score")).toBeVisible();

  const youScore = await page.getByTestId("You-score").textContent();
  const opponentScore = await page.getByTestId("Opponent-score").textContent();

  return {
    you: parseInt(youScore || "0"),
    opponent: parseInt(opponentScore || "0"),
  };
}

test.describe("Pickleball Scoring", () => {
  test("should start with 0-0 score", async ({ page }) => {
    await setupGame(page);

    const scores = await getScores(page);
    expect(scores.you).toBe(0);
    expect(scores.opponent).toBe(0);

    console.log("✅ Game starts at 0-0");
  });

  test("should increment YOUR score when + button clicked", async ({
    page,
  }) => {
    await setupGame(page);

    let scores = await getScores(page);
    console.log("Starting scores:", scores);

    await page.getByTestId("You-plus-button").click();

    await page.waitForTimeout(1000);

    scores = await getScores(page);
    console.log("After clicking +:", scores);
    expect(scores.you).toBe(1);
    expect(scores.opponent).toBe(0);

    console.log("✅ YOUR score incremented to 1");
  });

  test("should increment OPPONENT score when + button clicked", async ({
    page,
  }) => {
    await setupGame(page);

    await page.getByTestId("Opponent-plus-button").click();

    await page.waitForTimeout(1000);

    const scores = await getScores(page);
    expect(scores.you).toBe(0);
    expect(scores.opponent).toBe(1);

    console.log("✅ OPPONENT score incremented to 1");
  });

  test("should decrement score when - button clicked", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");
    await yourPlusButton.click();
    await page.waitForTimeout(500);
    await yourPlusButton.click();
    await page.waitForTimeout(500);
    await yourPlusButton.click();
    await page.waitForTimeout(1000);

    await page.getByTestId("You-minus-button").click();
    await page.waitForTimeout(1000);

    const scores = await getScores(page);
    expect(scores.you).toBe(2);

    console.log("✅ Score decremented correctly (3 → 2)");
  });

  test("should not allow negative scores", async ({ page }) => {
    await setupGame(page);

    const yourMinusButton = page.getByTestId("You-minus-button");

    await expect(yourMinusButton).toBeDisabled();

    console.log("✅ Minus button is disabled at 0 - cannot go negative");
  });

  test("should win at 11-0", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");

    for (let i = 1; i <= 11; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }

    await page.waitForTimeout(2000);

    const scores = await getScores(page);
    expect(scores.you).toBe(11);
    expect(scores.opponent).toBe(0);

    await expect(page.getByText(/game complete/i)).toBeVisible();

    console.log('✅ Game ends at 11-0 with "Game Complete!" message');
  });

  test("should win at 11-9 (win by 2)", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");
    const opponentPlusButton = page.getByTestId("Opponent-plus-button");

    for (let i = 1; i <= 10; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }

    for (let i = 1; i <= 9; i++) {
      await opponentPlusButton.click();
      await page.waitForTimeout(200);
    }
    await yourPlusButton.click();
    await page.waitForTimeout(2000);

    const scores = await getScores(page);
    expect(scores.you).toBe(11);
    expect(scores.opponent).toBe(9);

    await expect(page.getByText(/game complete/i)).toBeVisible();

    console.log('✅ Game ends at 11-9 with "Game Complete!" message');
  });

  test("should NOT win at 11-10 (must win by 2)", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");
    const opponentPlusButton = page.getByTestId("Opponent-plus-button");

    for (let i = 1; i <= 10; i++) {
      await opponentPlusButton.click();
      await page.waitForTimeout(200);
    }

    for (let i = 1; i <= 11; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }

    await page.waitForTimeout(2000);

    const scores = await getScores(page);
    expect(scores.you).toBe(11);
    expect(scores.opponent).toBe(10);

    await expect(page.getByText(/game complete/i)).not.toBeVisible();

    console.log(
      '✅ Game continues at 11-10 (need win by 2) - no "Game Complete" message'
    );
  });

  test("should win at 12-10 after deuce", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");
    const opponentPlusButton = page.getByTestId("Opponent-plus-button");

    for (let i = 1; i <= 10; i++) {
      await opponentPlusButton.click();
      await page.waitForTimeout(200);
    }

    for (let i = 1; i <= 12; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }

    let scores = await getScores(page);
    expect(scores.you).toBe(12);
    expect(scores.opponent).toBe(10);

    await expect(page.getByText(/game complete/i)).toBeVisible();

    console.log(
      '✅ Game ends at 12-10 after deuce with "Game Complete!" message'
    );
  });

  test("should track match statistics correctly", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");

    for (let i = 1; i <= 11; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }
    await page.waitForTimeout(2000);

    await expect(page.getByText(/game complete/i)).toBeVisible();

    let yourWinsText = await page
      .getByText(/your wins/i)
      .locator("..")
      .textContent();
    expect(yourWinsText).toContain("1");

    await page.getByRole("button", { name: /new game/i }).click();
    await page.waitForTimeout(500);

    await page.getByRole("button", { name: /singles/i }).click();
    await page.waitForTimeout(2000);

    await expect(
      page.getByRole("button", { name: /clear all stats/i })
    ).toBeVisible();

    for (let i = 1; i <= 11; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }
    await page.waitForTimeout(2000);

    yourWinsText = await page
      .getByText(/your wins/i)
      .locator("..")
      .textContent();
    expect(yourWinsText).toContain("2");

    console.log("✅ Match statistics track multiple games correctly");
  });

  test("Clear All Stats button appears after first game completion", async ({
    page,
  }) => {
    await setupGame(page);

    await expect(
      page.getByRole("button", { name: /clear all stats/i })
    ).not.toBeVisible();

    const yourPlusButton = page.getByTestId("You-plus-button");

    for (let i = 1; i <= 11; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }
    await page.waitForTimeout(2000);

    await expect(page.getByText(/game complete/i)).toBeVisible();

    await page.getByRole("button", { name: /new game/i }).click();
    await page.waitForTimeout(500);
    await page.getByRole("button", { name: /singles/i }).click();

    await expect(
      page.getByRole("button", { name: /clear all stats/i })
    ).toBeVisible();

    console.log(
      "✅ Clear All Stats button appears after first game completion"
    );
  });

  test("should clear statistics when Clear All Stats clicked", async ({
    page,
  }) => {
    page.on("dialog", async (dialog) => {
      console.log("Dialog appeared:", dialog.message());
      await dialog.accept();
    });

    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");

    for (let i = 1; i <= 11; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }
    await page.waitForTimeout(2000);

    await page.getByRole("button", { name: /new game/i }).click();
    await page.waitForTimeout(500);
    await page.getByRole("button", { name: /singles/i }).click();

    await expect(
      page.getByRole("button", { name: /clear all stats/i })
    ).toBeVisible();

    await page.getByRole("button", { name: /clear all stats/i }).click();

    await page.waitForTimeout(1000);

    await expect(page.getByRole("button", { name: /singles/i })).toBeVisible();
    await expect(page.getByRole("button", { name: /doubles/i })).toBeVisible();

    await page.getByRole("button", { name: /singles/i }).click();
    await page.waitForTimeout(2000);

    const yourWinsText = await page
      .getByText(/your wins/i)
      .locator("..")
      .textContent();
    const opponentWinsText = await page
      .getByText(/opponent wins/i)
      .locator("..")
      .textContent();

    expect(yourWinsText).toContain("0");
    expect(opponentWinsText).toContain("0");

    console.log("✅ Statistics cleared successfully");
  });

  test("should persist scores across page reload", async ({ page }) => {
    await setupGame(page);

    const yourPlusButton = page.getByTestId("You-plus-button");
    const opponentPlusButton = page.getByTestId("Opponent-plus-button");

    for (let i = 0; i < 7; i++) {
      await yourPlusButton.click();
      await page.waitForTimeout(200);
    }
    for (let i = 0; i < 4; i++) {
      await opponentPlusButton.click();
      await page.waitForTimeout(200);
    }

    await page.waitForTimeout(1000);

    await page.reload();
    await page.waitForTimeout(2000);

    const scores = await getScores(page);
    expect(scores.you).toBe(7);
    expect(scores.opponent).toBe(4);

    console.log("✅ Scores persist after page reload (7-4)");
  });
});
