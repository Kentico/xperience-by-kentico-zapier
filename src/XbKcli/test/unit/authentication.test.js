/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../index");
const { WEBSITE, AUTH_HEADER, authData } = require("../support/testBundle");

const appTester = zapier.createAppTester(App);

describe("authentication", () => {
  it("calls /auth/me with the API key header and returns the response", async () => {
    const scope = nock(WEBSITE)
      .get("/auth/me")
      .matchHeader("authorization", AUTH_HEADER)
      .reply(200, { message: "Authentication successful", channel: "DancingGoat" });

    const response = await appTester(App.authentication.test, { authData: authData() });

    expect(response.data).toEqual({ message: "Authentication successful", channel: "DancingGoat" });
    expect(scope.isDone()).toBe(true);
  });

  it("rejects a website URL without protocol before making a request", async () => {
    await expect(
      appTester(App.authentication.test, { authData: { website: "xbyk.example.test", apiKey: "k" } })
    ).rejects.toThrow("You did not provide the website url in the correct format.");
  });

  describe("error mapping (afterResponse)", () => {
    const cases = [
      [401, "The API Key you supplied is incorrect."],
      [403, "You are not authorized to perform this action."],
      [404, "You may have entered the wrong url. Url must be absolute and without trailing slash"],
      [500, "Response status 500. You may check the Xperience by Kentico Event Log."],
    ];

    it.each(cases)("maps HTTP %i to a user-facing message", async (status, message) => {
      nock(WEBSITE).get("/auth/me").reply(status, "nope");

      await expect(appTester(App.authentication.test, { authData: authData() })).rejects.toThrow(message);
    });
  });
});
