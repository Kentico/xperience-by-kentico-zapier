/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const { WEBSITE, AUTH_HEADER, TARGET_URL, authData } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

const trigger = App.triggers.form_submission;
const CLASSNAME = "BizForm.DancingGoatContactUs";

describe("triggers.form_submission", () => {
  it("reads the form from the classname input field", () => {
    expect(trigger.operation.inputFields[0].key).toBe("classname");
  });

  it("performSubscribe posts the form class name and target URL", async () => {
    const scope = nock(WEBSITE)
      .post("/zapier/triggers/formsubmission", { ObjectType: CLASSNAME, ZapierUrl: TARGET_URL })
      .query({ format: "json" })
      .matchHeader("authorization", AUTH_HEADER)
      .matchHeader("accept", "application/json")
      .reply(200, { triggerId: 42 });

    const result = await appTester(trigger.operation.performSubscribe, {
      authData: authData(),
      inputData: { classname: CLASSNAME },
      targetUrl: TARGET_URL,
    });

    expect(result).toEqual({ triggerId: 42 });
    expect(scope.isDone()).toBe(true);
  });

  it("performList fetches the latest submission for the selected form", async () => {
    const record = { UserFirstName: "Ada", UserEmail: "ada@example.test" };
    const scope = nock(WEBSITE)
      .get(`/zapier/triggers/formsubmission/${CLASSNAME}`)
      .query({ topN: 1, format: "json" })
      .matchHeader("authorization", AUTH_HEADER)
      .reply(200, record);

    const result = await appTester(trigger.operation.performList, {
      authData: authData(),
      inputData: { classname: CLASSNAME },
    });

    expect(result).toEqual([record]);
    expect(scope.isDone()).toBe(true);
  });
});
