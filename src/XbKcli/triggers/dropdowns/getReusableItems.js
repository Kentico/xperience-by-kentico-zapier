const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/${ClassContentTypeType.REUSABLE}/${bundle.inputData.reusableClassname}/${bundle.inputData.languageName}`,
    method: "GET",
  };

  let reusableItems = await z.request(options);

  return z.JSON.parse(reusableItems.content);
}

module.exports = {
  key: "get_reusable_items",
  noun: "Reusable item selector",
  display: {
    label: "Reusable item",
    description: "Gets supported reusable items",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "DancingGoat.Coffee",
      name: "Boston coffee place",
    },
    outputFields: [
      {
        key: "id",
        label: "Classname",
        type: "string",
      },
      {
        key: "name",
        label: "Display name",
        type: "string",
      },
    ],
  },
};
