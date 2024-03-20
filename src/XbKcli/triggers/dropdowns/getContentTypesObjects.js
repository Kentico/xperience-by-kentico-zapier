async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/triggers/contentobjects`,
    method: "GET",
  };

  let contentObjs = await z.request(options);

  return z.JSON.parse(contentObjs.content);
}

module.exports = {
  key: "get_content_types_objects",
  noun: "Content object",
  display: {
    label: "Content object",
    description: "Gets supported content objects",
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
