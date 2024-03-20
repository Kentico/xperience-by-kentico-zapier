const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/types/${ClassContentTypeType.HEADLESS}/${bundle.inputData.headlessChannelId}`,
    method: "GET",
  };

  let headlessTypes = await z.request(options);

  return z.JSON.parse(headlessTypes.content);
}

module.exports = {
  key: "get_headless_types",
  noun: "Headless type",
  display: {
    label: "Headless type",
    description: "Gets supported headless types",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "DancingGoat.Coffee",
      name: "Coffee",
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
