const getFormClassNamesField = require("../fields/getFormClassNamesField");
const getFormInputsField = require("../fields/getFormInputsField");
const getFormSchema = require("../utils/getFormSchema");

async function execute(z, bundle) {
  const schema = await getFormSchema(z, bundle, bundle.inputData.classname);
  const input = JSON.parse(JSON.stringify(bundle.inputData));
  const keys = schema.map((c) => c.column);
  for (const prop in input) {
    if (!keys.includes(prop)) {
      delete input[prop];
    }
  }

  const options = {
    url: `${bundle.authData.website}/zapier/actions/biz-form/${bundle.inputData.classname}`,
    method: "POST",
    headers: {
      Accept: "application/json",
    },
    body: input,
  };

  const response = await z.request(options);
  return z.JSON.parse(response.content);
}

const insertFormRecordAction = {
  noun: "Insert form record action",
  display: {
    hidden: false,
    description: "Inserts record into an XbyK form",
    label: "Insert Form Record",
  },
  key: "insertFormRecordAction",
  operation: {
    perform: execute,
    inputFields: [
      getFormClassNamesField({ required: true, altersDynamicFields: true }),
      async function (z, bundle) {
        return await getFormInputsField(z, bundle, bundle.inputData.classname);
      },
    ],
    sample: {
      classname: "BizForm.DancingGoatContactUs",
    },
  },
};

module.exports = insertFormRecordAction;
