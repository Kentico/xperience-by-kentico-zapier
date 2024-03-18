function getWebsiteItemsField(extras) {
  return Object.assign(
    {
      label: "Website item",
      key: "pageId",
      required: true,
      type: "string",
      dynamic: "get_website_items.id.name",
    },
    extras || {}
  );
}

module.exports = getWebsiteItemsField;
