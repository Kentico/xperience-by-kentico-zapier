/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const { WEBSITE, AUTH_HEADER, authData } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

describe("hidden dropdown triggers", () => {
  const cases = [
    ["get_form_class_names", "/zapier/data/types/Form", {}],
    ["get_event_log_severity", "/zapier/data/event-log-severity", {}],
    ["get_content_types_objects", "/zapier/data/types/movetostep", {}],
    ["get_event_types", "/zapier/data/workflow-steps/DancingGoat.Cafe", { objectType: "DancingGoat.Cafe" }],
  ];

  it.each(cases)("%s GETs %s and returns the option list", async (key, path, inputData) => {
    const options = [
      { id: "A", name: "Option A" },
      { id: "B", name: "Option B" },
    ];
    const scope = nock(WEBSITE).get(path).matchHeader("authorization", AUTH_HEADER).reply(200, options);

    const result = await appTester(App.triggers[key].operation.perform, { authData: authData(), inputData });

    expect(result).toEqual(options);
    expect(scope.isDone()).toBe(true);
  });
});
