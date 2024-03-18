async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/types`,
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  };

  let classnames = await z.request(options);
  return z.JSON.parse(classnames.content);
}

module.exports = {
  key: "get_class_content_type_types",
  noun: "Content type selector",
  display: {
    label: "Type",
    description: "Gets content types",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "Reusable",
      name: "Reusable",
    },
    outputFields: [
      {
        key: "name",
        label: "Content type",
        type: "string",
      },
    ],
  },
};
