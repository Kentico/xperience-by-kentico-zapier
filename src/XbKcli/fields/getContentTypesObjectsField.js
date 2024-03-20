function getContentTypesObjectsField(extras) {
  return Object.assign(
    {
      label: "Content type",
      key: "objectType",
      required: true,
      type: "string",
      dynamic: "get_content_types_objects.id.name",
      altersDynamicFields: true,
    },
    extras || {}
  );
}

module.exports = getContentTypesObjectsField;
