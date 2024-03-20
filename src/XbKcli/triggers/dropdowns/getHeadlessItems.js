const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/${ClassContentTypeType.HEADLESS}/${bundle.inputData.headlessClassname}/${bundle.inputData.headlessChannelId}/${bundle.inputData.languageName}`,
    method: "GET",
  };

  let headlessItems = await z.request(options);

  return z.JSON.parse(headlessItems.content);
}

module.exports = {
  key: "get_headless_items",
  noun: "Headless item selector",
  display: {
    label: "Headless item",
    description: "Gets supported headless items",
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
