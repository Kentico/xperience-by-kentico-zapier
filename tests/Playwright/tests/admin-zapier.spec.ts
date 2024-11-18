import { test, expect } from "@playwright/test";
import { AdminPage } from "../pageObjects/AdminPage";

const adminData = {
  username: "administrator",
  password: "admin",
};

test.describe("[Zapier integration]", () => {
  let adminPage: AdminPage;

  test.beforeEach(async ({ page }) => {
    adminPage = new AdminPage(page);
    await adminPage.goto("/admin/logon");
    await adminPage.signIn(adminData.username, adminData.password);
    await page.waitForURL("/admin/dashboard");
  });

  test(`[API Key] Generate and delete`, async ({ page }) => {
    test.info().annotations.push({
      type: "Info",
      description: "Tests generation and deletion of API key.",
    });

    await test.step("Go to zapier/apikey", async () => {
      await page.goto("/admin/zapier/apikey");
      await expect(adminPage.$generateApiKeyBtn).toBeVisible(); // page is loaded
    });
    await test.step("Generate new API key", async () => {
      await adminPage.$generateApiKeyBtn.click();
      await expect(adminPage.$generatedApiKey).toBeVisible();
      await page.locator("button", { hasText: "Continue" }).click();
    });
    await test.step("Check API key table for an entry", async () => {
      await expect(adminPage.$generateApiKeyBtn).toBeVisible(); // page is loaded
      await expect(page.getByTestId("table-cell-ApiKeyToken")).toBeVisible();
    });
    await test.step("Delete API key", async () => {
      await adminPage.$deleteApiKeyBtn.click();
      await adminPage.$deleteApiKeyConfirmBtn.click();
    });
    await test.step("API key table must be empty", async () => {
      await expect(page.getByTestId("table-cell-ApiKeyToken")).not.toBeAttached();
    });
  });

  test(`[Zapier triggers] List Zapier triggers`, async ({ page }) => {
    test.info().annotations.push({
      type: "Info",
      description:
        "Test checks for no errors to be visible after accessing zapier triggers and one of the triggers detail",
    });

    await test.step("Go to zapier/zaps", async () => {
      await page.goto("/admin/zapier/zaps");
    });
    await test.step("Check list of triggers", async () => {
      await expect(page.getByTestId("table-row")).toHaveCount(3); // 3 triggers visible on page
      await expect(page.getByTestId("snackbar-item-error")).not.toBeAttached();
      await expect(page.getByText("error")).not.toBeAttached();
    });
    await test.step("Check trigger detail", async () => {
      await page.getByTestId("table-row").first().click(); // view first trigger
      await expect(page.getByTestId("ZapierTriggerZapierURL")).toBeVisible();
      await expect(page.getByTestId("snackbar-item-error")).not.toBeAttached();
      await expect(page.getByText("error")).not.toBeAttached();
    });
  });
});
