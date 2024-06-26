async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/data/workflow-steps/${bundle.inputData.objectType}`,
    method: "GET",
  };

  let eventTypes = await z.request(options);
  return z.JSON.parse(eventTypes.content);
}

module.exports = {
  key: "get_event_types",
  noun: "Event type",
  display: {
    label: "Event type",
    description: "Gets supported event types",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "event",
      name: "om.contact",
    },
    outputFields: [
      {
        key: "name",
        label: "Event type display name",
        type: "string",
      },
      {
        key: "id",
        label: "Event type codename",
        type: "string",
      },
    ],
  },
};
