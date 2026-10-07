/* globals describe, it, expect */
// Same validation path `zapier validate` / `zapier push` use: functions are
// serialized to $func$ references before the JSON schema is applied.
// Deliberately imports core internals ("./src/*" is in the package exports map);
// if a core upgrade removes compileApp/validateApp, switch to `zapier validate` in CI.
const schemaTools = require("zapier-platform-core/src/tools/schema");

const App = require("../../index");

describe("app definition", () => {
  it("passes zapier-platform-schema validation", () => {
    const errors = schemaTools.validateApp(schemaTools.compileApp(App));
    expect(errors).toEqual([]);
  });

  it("exposes the public triggers and creates", () => {
    expect(Object.keys(App.triggers).sort()).toEqual(
      [
        "event_log_create",
        "form_submission",
        "get_content_types_objects",
        "get_event_log_severity",
        "get_event_types",
        "get_form_class_names",
        "move_to_step",
      ].sort()
    );
    expect(Object.keys(App.creates)).toEqual(["insertFormRecordAction"]);
  });

  it("points every dynamic dropdown field at an existing hidden trigger", () => {
    const dynamicFields = [
      require("../../fields/getFormClassNamesField")(),
      require("../../fields/getEventLogSeverityField")(),
      require("../../fields/getEventTypesField")(),
      require("../../fields/getContentTypesObjectsField")(),
    ];

    for (const field of dynamicFields) {
      const [triggerKey, idField, labelField] = field.dynamic.split(".");
      const trigger = App.triggers[triggerKey];
      expect(trigger).toBeDefined();
      expect(trigger.display.hidden).toBe(true);
      const outputKeys = trigger.operation.outputFields.map((f) => f.key);
      expect(outputKeys).toEqual(expect.arrayContaining([idField, labelField]));
    }
  });
});
