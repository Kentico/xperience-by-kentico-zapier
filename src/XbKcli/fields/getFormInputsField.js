const getFormSchema = require("../utils/getFormSchema");
const getSimpleField = require("./getSimpleField");

async function getFormInputsField(z, bundle, classname) {
  const schema = await getFormSchema(z, bundle, classname);

  // getSimpleField returns undefined for column types Zapier cannot map (e.g. binary, content item reference).
  // Optional ones are left out; a required one would make every insert fail on the server, so say so up front.
  const unsupportedRequired = schema.filter((field) => !getSimpleField(field) && !field.allowempty);
  if (unsupportedRequired.length > 0) {
    const columns = unsupportedRequired.map((field) => `${field.column} (${field.columntype})`).join(", ");
    throw new z.errors.Error(
      `Form ${classname} has required fields that Zapier cannot fill: ${columns}. Make them optional in Xperience by Kentico or use a different form.`,
      "UnsupportedRequiredFormField",
      400
    );
  }

  const fields = schema.map(getSimpleField).filter(Boolean);

  // Sort by column
  fields.sort((a, b) => a.key.localeCompare(b.key));

  return fields;
}

module.exports = getFormInputsField;
