/* globals describe, it, expect */

const zapier = require('zapier-platform-core');

const App = require('../index');
const appTester = zapier.createAppTester(App);

describe('custom auth', () => {
  it('passes authentication and returns json', async () => {
    const bundle = {
      authData: {
        website: 'https://nf54t8h6-26547.euw.devtunnels.ms',
        apiKey: 'ApiKey secret',
      },
    };

    const response = await appTester(App.authentication.test, bundle);
    expect(response.data).toHaveProperty('message');
  });

  it('fails on bad auth', async () => {
    const bundle = {
      authData: {
        website: 'https://nf54t8h6-26547.euw.devtunnels.ms',
        apiKey: 'ApiKey bad',
      },
    };

    try {
      await appTester(App.authentication.test, bundle);
    } catch (error) {
      expect(error.message).toContain('The API Key you supplied is incorrect');
      return;
    }
    throw new Error('appTester should have thrown');
  });
});
