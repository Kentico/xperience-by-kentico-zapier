/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const { WEBSITE, AUTH_HEADER, TARGET_URL, authData } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

const trigger = App.triggers.move_to_step;
const OBJECT_TYPE = "DancingGoat.Cafe";
const EVENT_TYPE = "LegalReview";

describe("triggers.move_to_step", () => {
  it("shows only the content type field until a content type is chosen", async () => {
    const fieldsFn = trigger.operation.inputFields[0];

    const withoutType = await appTester(fieldsFn, { authData: authData(), inputData: {} });
    const withType = await appTester(fieldsFn, { authData: authData(), inputData: { objectType: OBJECT_TYPE } });

    expect(withoutType.map((f) => f.key)).toEqual(["objectType"]);
    expect(withType.map((f) => f.key)).toEqual(["objectType", "eventType"]);
    expect(withType[1]).toMatchObject({ dynamic: "get_event_types.id.name", required: true });
  });

  it("performSubscribe posts content type, workflow step and target URL", async () => {
    const scope = nock(WEBSITE)
      .post("/zapier/triggers/movetostep", {
        EventType: EVENT_TYPE,
        ObjectType: OBJECT_TYPE,
        ZapierUrl: TARGET_URL,
      })
      .query({ format: "json" })
      .matchHeader("authorization", AUTH_HEADER)
      .reply(200, { triggerId: 9 });

    const result = await appTester(trigger.operation.performSubscribe, {
      authData: authData(),
      inputData: { objectType: OBJECT_TYPE, eventType: EVENT_TYPE },
      targetUrl: TARGET_URL,
    });

    expect(result).toEqual({ triggerId: 9 });
    expect(scope.isDone()).toBe(true);
  });

  it("performList fetches sample data for the content type and step", async () => {
    const record = { DisplayName: "Content item name", ContentTypeName: OBJECT_TYPE };
    const scope = nock(WEBSITE)
      .get(`/zapier/triggers/movetostep/${OBJECT_TYPE}/${EVENT_TYPE}`)
      .query({ topN: 1, format: "json" })
      .reply(200, record);

    const result = await appTester(trigger.operation.performList, {
      authData: authData(),
      inputData: { objectType: OBJECT_TYPE, eventType: EVENT_TYPE },
    });

    expect(result).toEqual([record]);
    expect(scope.isDone()).toBe(true);
  });
});
