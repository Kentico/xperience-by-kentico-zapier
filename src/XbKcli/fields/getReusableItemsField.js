function getReusableItemsField(extras) {
  return Object.assign(
    {
      label: "Reusable item",
      key: "reusableItemId",
      required: true,
      type: "string",
      dynamic: "get_reusable_items.id.name",
    },
    extras || {}
  );
}

module.exports = getReusableItemsField;
