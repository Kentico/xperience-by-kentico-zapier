function getReusableTypesField(extras) {
  return Object.assign(
    {
      label: "Reusable type",
      key: "reusableClassname",
      required: true,
      type: "string",
      dynamic: "get_reusable_types.id.name",
    },
    extras || {}
  );
}

module.exports = getReusableTypesField;
