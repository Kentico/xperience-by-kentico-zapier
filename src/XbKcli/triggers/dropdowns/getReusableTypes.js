async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/actions/types/reusable`,
    method: "GET",
  };

  let reusableTypes = await z.request(options);

  return z.JSON.parse(reusableTypes.content);
}

module.exports = {
  key: "get_reusable_types",
  noun: "Reusable type",
  display: {
    label: "Reusable type",
    description: "Gets supported reusable types",
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
