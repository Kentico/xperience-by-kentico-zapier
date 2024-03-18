function getHeadlessTypesField(extras) {
  return Object.assign(
    {
      label: "Headless type",
      key: "headlessClassname",
      required: true,
      type: "string",
      dynamic: "get_headless_types.id.name",
      altersDynamicFields: true,
    },
    extras || {}
  );
}

module.exports = getHeadlessTypesField;
