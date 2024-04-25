const ClassType = require("../../utils/enums").ClassType;

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/data/types/${ClassType.FORM}`,
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  };

  let classnames = await z.request(options);
  return z.JSON.parse(classnames.content);
}

module.exports = {
  key: "get_form_class_names",
  noun: "Form class name",
  display: {
    label: "Form class name",
    description: "Gets supported form class names",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "BizForm.DancingGoatContactUs",
      name: "Contact Us",
    },
    outputFields: [
      {
        key: "id",
        label: "Form class name",
        type: "string",
      },
      {
        key: "name",
        label: "Form name",
        type: "string",
      },
    ],
  },
};
