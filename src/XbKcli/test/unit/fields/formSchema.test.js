/* globals describe, it, expect */
const nock = require("nock");
const zapier = require("zapier-platform-core");

const App = require("../../../index");
const getFormSchema = require("../../../utils/getFormSchema");
const getFormInputsField = require("../../../fields/getFormInputsField");
const { WEBSITE, AUTH_HEADER, authData, FORM_DEFINITION_XML } = require("../../support/testBundle");

const appTester = zapier.createAppTester(App);

const CLASSNAME = "BizForm.DancingGoatContactUs";

const mockFormDefinition = () =>
  nock(WEBSITE)
    .get(`/zapier/actions/biz-form/${CLASSNAME}`)
    .matchHeader("authorization", AUTH_HEADER)
    .reply(200, JSON.stringify(FORM_DEFINITION_XML), { "Content-Type": "application/json" });

describe("utils.getFormSchema", () => {
  it("returns an empty schema without a request when no class name is given", async () => {
    const schema = await appTester((z, bundle) => getFormSchema(z, bundle, undefined), { authData: authData() });
    expect(schema).toEqual([]);
  });

  it("parses the form XML and drops primary key, guid and system columns", async () => {
    const scope = mockFormDefinition();

    const schema = await appTester((z, bundle) => getFormSchema(z, bundle, CLASSNAME), { authData: authData() });

    expect(schema.map((f) => f.column)).toEqual(["UserFirstName", "UserEmail", "UserMessage", "Subscribe", "Attachment"]);
    expect(schema[0]).toMatchObject({
      column: "UserFirstName",
      columntype: "text",
      columnsize: "200",
      visible: true,
      fieldcaption: "First name",
      fielddescription: "Your first name",
    });
    expect(schema[0].system).toBeFalsy();
    expect(schema[0].isPK).toBe(false);
    expect(schema[2]).toMatchObject({ allowempty: "true", explanationtext: "Optional" });
    expect(schema[3]).toMatchObject({ defaultvalue: "false" });
    expect(scope.isDone()).toBe(true);
  });
});

describe("fields.getFormInputsField", () => {
  it("builds Zapier input fields sorted by key and leaves out unmappable column types", async () => {
    mockFormDefinition();

    const fields = await appTester((z, bundle) => getFormInputsField(z, bundle, CLASSNAME), { authData: authData() });

    expect(fields.map((f) => f.key)).toEqual(["Subscribe", "UserEmail", "UserFirstName", "UserMessage"]);
    expect(fields.find((f) => f.key === "Attachment")).toBeUndefined();
    expect(fields.find((f) => f.key === "UserFirstName")).toMatchObject({
      label: "First name",
      type: "string",
      required: true,
      helpText: "Your first name",
    });
    expect(fields.find((f) => f.key === "UserMessage")).toMatchObject({ type: "text", required: false });
    expect(fields.find((f) => f.key === "Subscribe")).toMatchObject({ type: "boolean", default: "false" });
  });

  it("fails with a clear error when a required field has a column type Zapier cannot map", async () => {
    nock(WEBSITE)
      .get(`/zapier/actions/biz-form/${CLASSNAME}`)
      .matchHeader("authorization", AUTH_HEADER)
      .reply(
        200,
        JSON.stringify(FORM_DEFINITION_XML.replace('column="Attachment" columntype="binary" allowempty="true"', 'column="Attachment" columntype="binary"')),
        { "Content-Type": "application/json" }
      );

    await expect(appTester((z, bundle) => getFormInputsField(z, bundle, CLASSNAME), { authData: authData() })).rejects.toThrow(
      /required fields that Zapier cannot fill: Attachment \(binary\)/
    );
  });
});
