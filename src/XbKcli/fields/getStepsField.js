function getStepsField(extras) {
    return Object.assign(
      {
        label: "Step name",
        key: "stepName",
        required: true,
        type: "string",
        dynamic: "get_steps_for_item.id.name",
      },
      extras || {}
    );
  }
  
  module.exports = getStepsField;