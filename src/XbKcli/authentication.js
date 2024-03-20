'use strict';

// You want to make a request to an endpoint that is either specifically designed
// to test auth, or one that every user will have access to. eg: `/me`.
// By returning the entire request object, you have access to the request and
// response data for testing purposes. Your connection label can access any data
// from the returned response using the `json.` prefix. eg: `{{json.username}}`.
const test = (z, bundle) =>
{
  const websiteUrl = bundle.authData.website;

  let RgExp = new RegExp("^(?:[a-z]+:)?//", "i");
  if (!RgExp.test(websiteUrl)) {
    throw new z.errors.Error(
      'You did not provide the website url in the correct format.',
      'Error'
    );
  }

  return z.request({ url: `${bundle.authData.website}/auth/me` });
}
  

// This function runs after every outbound request. You can use it to check for
// errors or modify the response. You can have as many as you need. They'll need
// to each be registered in your index.js file.
const handleBadResponses = (response, z, bundle) => {
  switch (response.status) {
    case 200:
    case 201:
      return response;
    case 401:
      throw new z.errors.Error(
        'The API Key you supplied is incorrect.',
        'AuthenticationError',
        response.status
      );
    case 403:
      throw new z.errors.Error(
        `You are not authorized to perform this action.`,
        'AuthorizationError',
        response.status
      );
    case 404:
      throw new z.errors.Error(
        `You may have entered the wrong url. Url must be absolute and without trailing slash "/".`,
        'ResponseError',
        response.status
      );
    default:
      throw new z.errors.Error(
        `Response status ${response.status}. You may check the Xperience by Kentico Event Log.`,
        'ResponseError',
        response.status
      );
  }
};

// This function runs before every outbound request. You can have as many as you
// need. They'll need to each be registered in your index.js file.
const includeApiKey = (request, z, bundle) => {
  if (bundle.authData.apiKey) {
    // Use these lines to include the API key in the querystring
    // request.params = request.params || {};
    // request.params.api_key = bundle.authData.apiKey;

    // If you want to include the API key in the header instead, uncomment this:
    request.headers.Authorization = `XbyKZapierApiKey ${bundle.authData.apiKey}`;
  }

  return request;
};

module.exports = {
  config: {
    // "custom" is the catch-all auth type. The user supplies some info and Zapier can
    // make authenticated requests with it
    type: 'custom',

    // Define any input app's auth requires here. The user will be prompted to enter
    // this info when they connect their account.
    fields: [
      {
        label: 'Website base URL',
        key: 'website',
        type: 'string',
        required: true,
        helpText: 'The protocol and domain of your website without trailing slash eg. "https://docs.kentico.com"'
    },
      { 
        label: 'API Key',
        key: 'apiKey',
        type: 'string',
        required: true,
        helpText: 'Find the API Key in your Xperience by Kentico admin application by clicking on Configuration > Zapier > API Key',
      }
    ],

    // The test method allows Zapier to verify that the credentials a user provides
    // are valid. We'll execute this method whenever a user connects their account for
    // the first time.
    test,

    // This template string can access all the data returned from the auth test. If
    // you return the test object, you'll access the returned data with a label like
    // `{{json.X}}`. If you return `response.data` from your test, then your label can
    // be `{{X}}`. This can also be a function that returns a label. That function has
    // the standard args `(z, bundle)` and data returned from the test can be accessed
    // in `bundle.inputData.X`.
    connectionLabel: '{{bundle.authData.website}}',
  },
  befores: [includeApiKey],
  afters: [handleBadResponses],
};
