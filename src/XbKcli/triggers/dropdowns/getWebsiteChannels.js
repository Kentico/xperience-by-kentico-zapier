const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/${ClassContentTypeType.WEBSITE}/channels`,
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  };

  let classnames = await z.request(options);
  return z.JSON.parse(classnames.content);
}

module.exports = {
  key: "get_website_channels",
  noun: "Website channel selector",
  display: {
    label: "Website channel",
    description: "Gets website channels",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "en",
      name: "English",
    },
    outputFields: [
      {
        key: "id",
        label: "Website channel identifier",
        type: "string",
      },
      {
        key: "name",
        label: "Website channel",
        type: "string",
      },
    ],
  },
};
