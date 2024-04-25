const deleteTrigger = require("../utils/deleteTrigger");
const getEventLogSeverityField = require("../fields/getEventLogSeverityField");

const triggerNoun = "New Event Log Entry";

const performHook = async (z, bundle) => {
  return [bundle.cleanedRequest];
};

const performSubscribe = async (z, bundle) => {
  const hook = {
    ZapierUrl: bundle.targetUrl,
    Severity: JSON.parse(JSON.stringify(bundle.inputData.severity)),
  };
  const options = {
    url: `${bundle.authData.website}/zapier/triggers/eventlogcreate`,
    params: {
      format: "json",
    },
    method: "POST",
    headers: {
      Accept: "application/json",
    },
    body: hook,
  };

  const response = await z.request(options);

  return z.JSON.parse(response.content);
};

const performUnsubscribe = async (z, bundle) => {
  return await deleteTrigger(z, bundle);
};

const getFallbackData = async (z, bundle) => {
  const options = {
    url: `${bundle.authData.website}/zapier/triggers/eventlogcreate`,
    method: "GET",
    params: {
      topN: 1,
      format: "json",
    },
    headers: {
      Accept: "application/json",
    },
  };

  const response = await z.request(options);
  const json = z.JSON.parse(response.content);

  if (!json) return [sampleObj];

  return [json];
};

const sampleObj = {
  EventID: "0",
};

module.exports = {
  // see here for a full list of available properties:
  // https://github.com/zapier/zapier-platform/blob/main/packages/schema/docs/build/schema.md#triggerschema
  key: "event_log_create",
  noun: triggerNoun,

  display: {
    label: triggerNoun,
    description: "Triggers when a new event log is inserted.",
  },

  operation: {
    type: "hook",

    // `inputFields` defines the fields a user could provide
    // Zapier will pass them in as `bundle.inputData` later. They're optional.
    inputFields: [getEventLogSeverityField()],

    perform: performHook,
    performSubscribe,
    performUnsubscribe,
    performList: getFallbackData,

    // In cases where Zapier needs to show an example record to the user, but we are unable to get a live example
    // from the API, Zapier will fallback to this hard-coded sample. It should reflect the data structure of
    // returned records, and have obvious placeholder values that we can show to any user.
    sample: sampleObj,
  },
};
