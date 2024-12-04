import { type Locator, type Page } from "@playwright/test";
import { BasePage } from "./BasePage";

export class AdminPage extends BasePage {
  readonly $username: Locator;
  readonly $password: Locator;
  readonly $signInBtn: Locator;
  readonly $generateApiKeyBtn: Locator;
  readonly $deleteApiKeyBtn: Locator;
  readonly $deleteApiKeyConfirmBtn: Locator;
  readonly $generatedApiKey: Locator;

  constructor(page: Page) {
    super(page);

    this.$username = page.getByTestId("userName");
    this.$password = page.getByTestId("password");
    this.$signInBtn = page.getByTestId("submit");
    this.$generatedApiKey = page.getByTestId("ApiKeyRawToken");
    this.$generateApiKeyBtn = page.getByTestId("Generate");
    this.$deleteApiKeyBtn = page.getByTestId("button-Delete");
    this.$deleteApiKeyConfirmBtn = page.getByTestId("confirm-action");
  }

  async signIn(username: string, password: string) {
    await this.$username.fill(username);
    await this.$password.fill(password);
    await this.$signInBtn.click();
  }
}
