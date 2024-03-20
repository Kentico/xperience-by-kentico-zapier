async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/languages`,
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  };

  let classnames = await z.request(options);
  return z.JSON.parse(classnames.content);
}

module.exports = {
  key: "get_language_names",
  noun: "Language name selector",
  display: {
    label: "Language name",
    description: "Gets supported languages",
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
        label: "Language shortcut",
        type: "string",
      },
      {
        key: "name",
        label: "Language name",
        type: "string",
      },
    ],
  },
};
