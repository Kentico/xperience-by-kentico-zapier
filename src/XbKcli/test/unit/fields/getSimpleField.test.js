/* globals describe, it, expect */
const getSimpleField = require("../../../fields/getSimpleField");

describe("fields.getSimpleField", () => {
  const base = { column: "Col", allowempty: false };

  it.each([
    ["text", "string"],
    ["guid", "string"],
    ["longtext", "text"],
    ["richtexthtml", "text"],
    ["integer", "integer"],
    ["longinteger", "integer"],
    ["decimal", "number"],
    ["double", "number"],
    ["float", "number"],
    ["datetime", "datetime"],
    ["boolean", "boolean"],
  ])("maps Xperience column type %s to Zapier field type %s", (columntype, type) => {
    const field = getSimpleField({ ...base, columntype });
    expect(field).toMatchObject({ key: "Col", type });
  });

  it("uses caption, description and default value when present", () => {
    const field = getSimpleField({
      ...base,
      columntype: "text",
      fieldcaption: "First name",
      fielddescription: "Your given name",
      defaultvalue: "Ada",
    });

    expect(field).toEqual({
      key: "Col",
      label: "First name",
      required: true,
      helpText: "Your given name",
      default: "Ada",
      type: "string",
    });
  });

  it("falls back to column name, explanation text and empty default", () => {
    const field = getSimpleField({ ...base, columntype: "text", allowempty: true, explanationtext: "Hint" });

    expect(field).toMatchObject({ label: "Col", required: false, helpText: "Hint", default: "" });
  });

  it("gives datetime fields without default a current timestamp", () => {
    const field = getSimpleField({ ...base, columntype: "datetime" });

    expect(typeof field.default).toBe("string"); // locale-formatted, so only presence is asserted
    expect(field.default.length).toBeGreaterThan(0);
  });
});
