const zapier = require('zapier-platform-core');

// Use this to make test calls into your app:
const App = require('../../index');
const appTester = zapier.createAppTester(App);
// read the `.env` file into the environment, if available
zapier.tools.env.inject();

describe('triggers.catch_xperience_webhook', () => {
  it('should run', async () => {
    const bundle = { 
      inputData: {
        eventType: "test",
        name: "name 123456",
        objectType: "BizForm.DancingGoatContactUs",
      },
      authData: {
      website: "https://nf54t8h6-26547.euw.devtunnels.ms",
      apiKey: 'ApiKey secret',
    },
    targetUrl: "www.test.cz"
   };

    const results = await appTester(App.triggers['catch_xperience_webhook'].operation.performSubscribe, bundle);
    //expect(results).toBeDefined();



    //const results2 = await appTester(App.triggers['get_object_types'].operation.perform, bundle);
    // TODO: add more assertions
  });
});
