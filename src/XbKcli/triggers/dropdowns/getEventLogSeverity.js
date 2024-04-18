async function execute(z, bundle) {
  const options = {
    url: `${bundle.authData.website}/zapier/data/event-log-severity`,
    method: "GET",
  };

  let eventTypes = await z.request(options);
  return z.JSON.parse(eventTypes.content);
}

module.exports = {
  key: "get_event_log_severity",
  noun: "Event log entry severity",
  display: {
    label: "Event log entry severity",
    description: "Gets event log severities",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "warning",
      name: "Warning",
    },
    outputFields: [
      {
        key: "name",
        label: "Display name",
        type: "string",
      },
      {
        key: "id",
        label: "Name",
        type: "string",
      },
    ],
  },
};
