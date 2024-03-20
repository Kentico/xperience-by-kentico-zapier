const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/types/${ClassContentTypeType.WEBSITE}/${bundle.inputData.websiteChannelId}`,
    method: "GET",
  };

  let websiteTypes = await z.request(options);

  return z.JSON.parse(websiteTypes.content);
}

module.exports = {
  key: "get_website_types",
  noun: "Website type",
  display: {
    label: "Website type",
    description: "Gets supported website types",
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
