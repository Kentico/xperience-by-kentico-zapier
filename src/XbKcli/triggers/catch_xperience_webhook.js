const triggerNoun = 'Catch Xperience by Kentico Webhook'
const getObjectTypesField = require('../fields/getObjectTypesField');
const getEventTypesField = require('../fields/getEventTypesField');



const infoObjectFields = async (z, bundle) => {
  const options = {
    url: `${bundle.authData.website}/zapier/data/objects`,
    method: 'GET'
  };
  const response = await z.request(options);

  // Call response.throwForStatus() if you're using zapier-platform-core v9 or older

  // Should return an array like [{"key":"field_1"},{"key":"field_2"}]

  console.log(response.data);
  return response.data; // response.json if you're using core v9 or older
};

const eventFields = async (z, bundle) => {
  const options = {
    url: `${bundle.authData.website}/zapier/data/events`,
    method: 'GET'
  };
  const response = await z.request(options);

  // Call response.throwForStatus() if you're using zapier-platform-core v9 or older

  // Should return an array like [{"key":"field_1"},{"key":"field_2"}]
  console.log(response.data);
  return response.data; // response.json if you're using core v9 or older
};


// triggers on a new catchxperiencewebhook with a certain tag
const performHook = async (z, bundle) => {
  // const response = await z.request({
  //   url: 'https://nf54t8h6-26547.euw.devtunnels.ms/zapier/trigger',
  //   params: {
  //     tag: bundle.inputData.tagName
  //   }
  // });
  // this should return an array of objects
  // return response.data;

  // infoObjectFields(z, bundle);
  // eventFields(z, bundle);
  return [bundle.cleanedRequest];
};

const performSubscribe = async (z, bundle) => {
  const hook = {
      'EventType': bundle.inputData.eventType,
      'Name': bundle.inputData.name,
      'ObjectType': bundle.inputData.objectType,
      'ZapierUrl': bundle.targetUrl,
      'WebhookCreatedManually': false
  };
  const options = {
      url: `${bundle.authData.website}/zapier/trigger`,
      params: {
          format: 'json'
      },
      method: 'POST',
      headers: {
          'Accept': 'application/json'
      },
      body: hook
  };

  const response = await z.request(options);

  return z.JSON.parse(response.content);
}

const performUnsubscribe = async (z, bundle) => {
  // bundle.subscribeData contains the parsed response JSON from the subscribe
  // request made initially.
  const webhook = bundle.subscribeData;

  const options = {
      url: `${bundle.authData.website}/zapier/trigger/${webhook.triggerId}`,
      method: 'DELETE',
      headers: {
          'Accept': 'application/json'
      }
  };

  const response = await z.request(options);

  return response.status === 200;
}


const getFallbackData = async (z, bundle) => {
  const options = {
      url: `${bundle.authData.website}/zapier/object/${bundle.inputData.objectType}`,
      method: 'GET',
      params: {
          topN: 1,
          format: 'json',
      },
      headers: {
          'Accept': 'application/json'
      }
  };

  const response = await z.request(options);
  const json = z.JSON.parse(response.content);


  if(!json) return [sampleObj];

  return [json];
}


const sampleObj = {
  'AppId': "Xperience by Kentico",
};



module.exports = {
  // see here for a full list of available properties:
  // https://github.com/zapier/zapier-platform/blob/main/packages/schema/docs/build/schema.md#triggerschema
  key: 'catch_xperience_webhook',
  noun: triggerNoun,

  display: {
    label: triggerNoun,
    description: 'Triggers when a new catchxperiencebykenticowebhook is created.'
  },

  operation: {
      type: 'hook',

      // `inputFields` defines the fields a user could provide
      // Zapier will pass them in as `bundle.inputData` later. They're optional.
      inputFields: [
        {
            label: 'Webhook name',
            helpText: 'Enter a webhook name which will appear in the Xperience by Kentico admin UI.',
            key: 'name',
            type: 'string',
            required: true
        },
        getObjectTypesField(),
        getEventTypesField(),
    ],

      perform: performHook,
      performSubscribe,
      performUnsubscribe,
      performList: getFallbackData,

      // In cases where Zapier needs to show an example record to the user, but we are unable to get a live example
      // from the API, Zapier will fallback to this hard-coded sample. It should reflect the data structure of
      // returned records, and have obvious placeholder values that we can show to any user.
      sample: sampleObj
    }
};
