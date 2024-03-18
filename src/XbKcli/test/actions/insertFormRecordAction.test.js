const zapier = require("zapier-platform-core");

// Use this to make test calls into your app:
const App = require("../../index");
const appTester = zapier.createAppTester(App);
// read the `.env` file into the environment, if available
zapier.tools.env.inject();

describe("actions.insertFormRecordAction", () => {
  it("should run", async () => {
    const bundle = {
      inputData: {
        classname: "BizForm.DancingGoatContactUs",
      },
      authData: {
        website: "https://vv2q1r24-26547.euw.devtunnels.ms",
        apiKey: "juwcdWAffCrNsI9IHgXVWIFef8T7wjLyH7oqa4HVZcM=",
      },
    };

    const results = await appTester(
      App.creates["insertFormRecordAction"].operation.perform,
      bundle
    );
    //expect(results).toBeDefined();

    //const results2 = await appTester(App.triggers['get_object_types'].operation.perform, bundle);
    // TODO: add more assertions
  });
});
