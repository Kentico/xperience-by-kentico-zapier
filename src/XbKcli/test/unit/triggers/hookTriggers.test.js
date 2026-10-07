/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const { WEBSITE, AUTH_HEADER, authData } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

const hookTriggers = [["form_submission"], ["event_log_create"], ["move_to_step"]];

describe("REST hook triggers (shared behaviour)", () => {
  it.each(hookTriggers)("%s perform returns the webhook payload unchanged as a single-item array", async (key) => {
    const payload = { Marker: key, FormInserted: "2024-01-01T00:00:00" };

    const result = await appTester(App.triggers[key].operation.perform, { authData: authData(), cleanedRequest: payload });

    expect(result).toEqual([payload]);
  });

  it.each(hookTriggers)("%s performUnsubscribe deletes the trigger id stored by performSubscribe", async (key) => {
    const scope = nock(WEBSITE)
      .delete("/zapier/triggers/42")
      .matchHeader("authorization", AUTH_HEADER)
      .reply(200, { status: "Success" });

    const result = await appTester(App.triggers[key].operation.performUnsubscribe, {
      authData: authData(),
      subscribeData: { triggerId: 42 },
    });

    expect(result).toBe(true);
    expect(scope.isDone()).toBe(true);
  });
});
