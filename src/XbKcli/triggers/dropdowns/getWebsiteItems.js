const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/${ClassContentTypeType.WEBSITE}/${bundle.inputData.websiteClassname}/${bundle.inputData.websiteChannelId}/${bundle.inputData.languageName}`,
    method: "GET",
  };

  let websiteItems = await z.request(options);

  return z.JSON.parse(websiteItems.content);
}

module.exports = {
  key: "get_website_items",
  noun: "Website item selector",
  display: {
    label: "Website item",
    description: "Gets supported website items",
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
