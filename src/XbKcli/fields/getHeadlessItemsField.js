function getHeadlessItemsField(extras) {
  return Object.assign(
    {
      label: "Headless item",
      key: "headlessItemId",
      required: true,
      type: "string",
      dynamic: "get_headless_items.id.name",
    },
    extras || {}
  );
}

module.exports = getHeadlessItemsField;
