function getWebsiteTypesField(extras) {
  return Object.assign(
    {
      label: "Website type",
      key: "websiteClassname",
      required: true,
      type: "string",
      dynamic: "get_website_types.id.name",
      altersDynamicFields: true,
    },
    extras || {}
  );
}

module.exports = getWebsiteTypesField;
