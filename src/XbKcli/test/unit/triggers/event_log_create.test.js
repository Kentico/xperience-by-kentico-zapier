/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const { WEBSITE, AUTH_HEADER, TARGET_URL, authData } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

const trigger = App.triggers.event_log_create;

describe("triggers.event_log_create", () => {
  it("asks for a list of severities (performSubscribe sends an array)", () => {
    expect(trigger.operation.inputFields[0]).toMatchObject({ key: "severity", list: true });
  });

  it("performSubscribe posts the target URL and the selected severities", async () => {
    const scope = nock(WEBSITE)
      .post("/zapier/triggers/eventlogcreate", { ZapierUrl: TARGET_URL, Severity: ["I", "W"] })
      .query({ format: "json" })
      .matchHeader("authorization", AUTH_HEADER)
      .reply(200, { triggerId: 7 });

    const result = await appTester(trigger.operation.performSubscribe, {
      authData: authData(),
      inputData: { severity: ["I", "W"] },
      targetUrl: TARGET_URL,
    });

    expect(result).toEqual({ triggerId: 7 });
    expect(scope.isDone()).toBe(true);
  });

  it("performList fetches the latest event log entry", async () => {
    const record = { EventID: 1, EventType: "E" };
    const scope = nock(WEBSITE)
      .get("/zapier/triggers/eventlogcreate")
      .query({ topN: 1, format: "json" })
      .reply(200, record);

    const result = await appTester(trigger.operation.performList, { authData: authData(), inputData: {} });

    expect(result).toEqual([record]);
    expect(scope.isDone()).toBe(true);
  });
});
