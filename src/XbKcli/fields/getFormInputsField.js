const getFormSchema = require("../utils/getFormSchema");
const getSimpleField = require("./getSimpleField");

async function getFormInputsField(z, bundle, classname) {
  const schema = await getFormSchema(z, bundle, classname);
  const fields = schema.map(getSimpleField);

  // Sort by columnn
  fields.sort((a, b) => a.key.localeCompare(b.key));

  return fields;
}

module.exports = getFormInputsField;
