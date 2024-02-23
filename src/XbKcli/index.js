const {
  config: authentication,
  befores = [],
  afters = [],
} = require('./authentication');


const getCatchXperienceWebhook = require("./triggers/catch_xperience_webhook");

const getObjectTypes = require('./triggers/dropdowns/getObjectTypes');
const getEventTypes = require('./triggers/dropdowns/getEventTypes');

module.exports = {
  // This is just shorthand to reference the installed dependencies you have.
  // Zapier will need to know these before we can upload.
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,

  authentication,

  beforeRequest: [...befores],

  afterResponse: [...afters],

  // If you want your trigger to show up, you better include it here!
  triggers: {
    [getCatchXperienceWebhook.key]: getCatchXperienceWebhook,

    [getObjectTypes.key]: getObjectTypes,
    [getEventTypes.key]: getEventTypes,
  },

  // If you want your searches to show up, you better include it here!
  searches: {},

  // If you want your creates to show up, you better include it here!
  creates: {},

  resources: {},
};
