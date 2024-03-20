const {
  config: authentication,
  befores = [],
  afters = [],
} = require("./authentication");

const getCatchXperienceWebhook = require("./triggers/catch_xperience_webhook");

const getObjectTypes = require("./triggers/dropdowns/getObjectTypes");
const getEventTypes = require("./triggers/dropdowns/getEventTypes");

const insertFormRecordAction = require("./actions/insertFormRecordAction");
const publishAction = require("./actions/publishAction");
const moveToStepAction = require("./actions/moveToStepAction");

const getFormClassNames = require("./triggers/dropdowns/getFormClassNames");
const getLanguageNames = require("./triggers/dropdowns/getLanguageNames");
const getClassContentTypeTypes = require("./triggers/dropdowns/getClassContentTypeTypes");
const getReusableTypes = require("./triggers/dropdowns/getReusableTypes");
const getReusableItems = require("./triggers/dropdowns/getReusableItems");
const getWebsiteTypes = require("./triggers/dropdowns/getWebsiteTypes");
const getWebsiteItems = require("./triggers/dropdowns/getWebsiteItems");
const getWebsiteChannels = require("./triggers/dropdowns/getWebsiteChannels");
const getHeadlessTypes = require("./triggers/dropdowns/getHeadlessTypes");
const getHeadlessItems = require("./triggers/dropdowns/getHeadlessItems");
const getHeadlessChannels = require("./triggers/dropdowns/getHeadlessChannels");
const getStepsForItem = require("./triggers/dropdowns/getStepsForItem");

module.exports = {
  // This is just shorthand to reference the installed dependencies you have.
  // Zapier will need to know these before we can upload.
  version: require("./package.json").version,
  platformVersion: require("zapier-platform-core").version,

  authentication,

  beforeRequest: [...befores],

  afterResponse: [...afters],

  // If you want your trigger to show up, you better include it here!
  triggers: {
    [getCatchXperienceWebhook.key]: getCatchXperienceWebhook,

    [getObjectTypes.key]: getObjectTypes,
    [getEventTypes.key]: getEventTypes,
    [getFormClassNames.key]: getFormClassNames,
    [getLanguageNames.key]: getLanguageNames,
    [getClassContentTypeTypes.key]: getClassContentTypeTypes,
    [getReusableTypes.key]: getReusableTypes,
    [getWebsiteTypes.key]: getWebsiteTypes,
    [getWebsiteChannels.key]: getWebsiteChannels,
    [getWebsiteItems.key]: getWebsiteItems,
    [getHeadlessTypes.key]: getHeadlessTypes,
    [getHeadlessChannels.key]: getHeadlessChannels,
    [getHeadlessItems.key]: getHeadlessItems,
    [getReusableItems.key]: getReusableItems,
    [getStepsForItem.key]: getStepsForItem,
  },

  // If you want your searches to show up, you better include it here!
  searches: {},

  // If you want your creates to show up, you better include it here!
  creates: {
    [insertFormRecordAction.key]: insertFormRecordAction,
    [publishAction.key]: publishAction,
    [moveToStepAction.key]: moveToStepAction,
  },

  resources: {},
};
