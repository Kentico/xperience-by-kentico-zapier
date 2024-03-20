const triggerNoun = 'Catch Contact'

const performHook = async (z, bundle) => {
  return [bundle.cleanedRequest];
};

const performSubscribe = async (z, bundle) => {
  const hook = {
      'Name': bundle.inputData.name,
      'ZapierUrl': bundle.targetUrl,
  };
  const options = {
      url: `${bundle.authData.website}/zapier/triggers/contactcreate`,
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
      url: `${bundle.authData.website}/zapier/triggers/contactcreate/${webhook.triggerId}`,
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
      url: `${bundle.authData.website}/zapier/object/OM.Contact/Create`,
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
  key: 'contact_create',
  noun: triggerNoun,

  display: {
    label: triggerNoun,
    description: 'Triggers when a new contact is created.'
  },

  operation: {
      type: 'hook',
      inputFields: [
        {
            label: 'Webhook name',
            helpText: 'Enter a webhook name which will appear in the Xperience by Kentico admin UI.',
            key: 'name',
            type: 'string',
            required: true
        },
    ],

      perform: performHook,
      performSubscribe,
      performUnsubscribe,
      performList: getFallbackData,

      sample: sampleObj
    }
};
