const triggerNoun = 'Catch Move to Step'

const getEventTypesField = require('../fields/getEventTypesField');
const getContentTypesObjectsField = require('../fields/getContentTypesObjectsField');


const performHook = async (z, bundle) => {
  return [bundle.cleanedRequest];
};

const performSubscribe = async (z, bundle) => {
  const hook = {
    'EventType': bundle.inputData.eventType,
    'Name': bundle.inputData.name,
    'ObjectType': bundle.inputData.objectType,
    'ZapierUrl': bundle.targetUrl,
  };
  const options = {
      url: `${bundle.authData.website}/zapier/triggers/movetostep`,
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
      url: `${bundle.authData.website}/zapier/triggers/movetostep/${webhook.triggerId}`,
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
      url: `${bundle.authData.website}/zapier/object/${bundle.inputData.objectType}/${bundle.inputData.eventType}`,
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
  key: 'move_to_step',
  noun: triggerNoun,

  display: {
    label: triggerNoun,
    description: 'Triggers when content item moves to a step.'
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
        async function makeFieldsForWebsite(z, bundle) {
            const res = [];
            res.push(
              getContentTypesObjectsField({
                required: true,
                altersDynamicFields: true,
              })
            );
            if (bundle.inputData.objectType != undefined) {
              res.push(
                getEventTypesField({
                  required: true,
                  altersDynamicFields: true,
                })
              );
              
            }
            return res;
          }
    ],

      perform: performHook,
      performSubscribe,
      performUnsubscribe,
      performList: getFallbackData,
      sample: sampleObj
    }
};
