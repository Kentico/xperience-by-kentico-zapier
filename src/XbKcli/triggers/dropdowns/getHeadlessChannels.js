const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/${ClassContentTypeType.HEADLESS}/channels`,
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  };

  let classnames = await z.request(options);
  return z.JSON.parse(classnames.content);
}

module.exports = {
  key: "get_headless_channels",
  noun: "Headless channel selector",
  display: {
    label: "Headless channel",
    description: "Gets headless channels",
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
        label: "Headless channel identifier",
        type: "string",
      },
      {
        key: "name",
        label: "Headless channel",
        type: "string",
      },
    ],
  },
};
