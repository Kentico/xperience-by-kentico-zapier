/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const { WEBSITE, AUTH_HEADER, authData, FORM_DEFINITION_XML } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

const create = App.creates.insertFormRecordAction;
const CLASSNAME = "BizForm.DancingGoatContactUs";

describe("creates.insertFormRecordAction", () => {
  it("reloads the dynamic form fields when the form changes", () => {
    expect(create.operation.inputFields[0]).toMatchObject({ key: "classname", altersDynamicFields: true });
  });

  it("loads the form schema and posts only columns that exist on the form", async () => {
    const scope = nock(WEBSITE)
      .get(`/zapier/actions/biz-form/${CLASSNAME}`)
      .matchHeader("authorization", AUTH_HEADER)
      .reply(200, JSON.stringify(FORM_DEFINITION_XML), { "Content-Type": "application/json" })
      .post(`/zapier/actions/biz-form/${CLASSNAME}`, {
        UserFirstName: "Ada",
        UserEmail: "ada@example.test",
        Subscribe: true,
      })
      .matchHeader("authorization", AUTH_HEADER)
      .matchHeader("accept", "application/json")
      .reply(200, { id: 123 });

    const result = await appTester(create.operation.perform, {
      authData: authData(),
      inputData: {
        classname: CLASSNAME,
        UserFirstName: "Ada",
        UserEmail: "ada@example.test",
        Subscribe: true,
        NotAFormColumn: "dropped",
        ContactUsID: 99,
      },
    });

    expect(result).toEqual({ id: 123 });
    expect(scope.isDone()).toBe(true);
  });
});
