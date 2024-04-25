const deleteTrigger = require("../utils/deleteTrigger");

const triggerNoun = "New Form Submission";

const getFormClassNamesField = require("../fields/getFormClassNamesField");

const performHook = async (z, bundle) => {
  return [bundle.cleanedRequest];
};

const performSubscribe = async (z, bundle) => {
  const hook = {
    ObjectType: bundle.inputData.classname,
    ZapierUrl: bundle.targetUrl,
  };
  const options = {
    url: `${bundle.authData.website}/zapier/triggers/formsubmission`,
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
    url: `${bundle.authData.website}/zapier/triggers/formsubmission/${bundle.inputData.classname}`,
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
  FormInserted: "2012-12-12",
};

module.exports = {
  // see here for a full list of available properties:
  // https://github.com/zapier/zapier-platform/blob/main/packages/schema/docs/build/schema.md#triggerschema
  key: "form_submission",
  noun: triggerNoun,

  display: {
    label: triggerNoun,
    description: "Triggers when a new form submission is created.",
  },

  operation: {
    type: "hook",

    // `inputFields` defines the fields a user could provide
    // Zapier will pass them in as `bundle.inputData` later. They're optional.
    inputFields: [getFormClassNamesField()],

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
