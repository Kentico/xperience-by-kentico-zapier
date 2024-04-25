const {
  config: authentication,
  befores = [],
  afters = [],
} = require("./authentication");

const getFormSubmissionTrigger = require("./triggers/form_submission_trigger");
const getEventLogCreateTrigger = require("./triggers/event_log_create_trigger");
const getMoveToStepTrigger = require("./triggers/move_to_step_trigger");

const getEventTypes = require("./triggers/dropdowns/getEventTypes");
const getEventLogSeverity = require("./triggers/dropdowns/getEventLogSeverity");

const insertFormRecordAction = require("./actions/insertFormRecordAction");

const getFormClassNames = require("./triggers/dropdowns/getFormClassNames");
const getContentTypesObjects = require("./triggers/dropdowns/getContentTypesObjects");

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
    [getFormSubmissionTrigger.key]: getFormSubmissionTrigger,
    [getEventLogCreateTrigger.key]: getEventLogCreateTrigger,
    [getMoveToStepTrigger.key]: getMoveToStepTrigger,

    [getEventTypes.key]: getEventTypes,
    [getFormClassNames.key]: getFormClassNames,
    [getContentTypesObjects.key]: getContentTypesObjects,
    [getEventLogSeverity.key]: getEventLogSeverity,
  },

  // If you want your searches to show up, you better include it here!
  searches: {},

  // If you want your creates to show up, you better include it here!
  creates: {
    [insertFormRecordAction.key]: insertFormRecordAction,
  },

  resources: {},
};
