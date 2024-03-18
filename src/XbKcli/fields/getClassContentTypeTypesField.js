function getClassContentTypeTypesField(extras) {
  return Object.assign(
    {
      label: "Content type",
      key: "classContentTypeType",
      required: true,
      type: "string",
      dynamic: "get_class_content_type_types.id.name",
    },
    extras || {}
  );
}

module.exports = getClassContentTypeTypesField;
