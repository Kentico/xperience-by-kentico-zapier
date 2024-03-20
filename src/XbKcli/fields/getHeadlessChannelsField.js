function getHeadlessChannelsField(extras) {
  return Object.assign(
    {
      label: "Headless channel",
      key: "headlessChannelId",
      required: true,
      type: "string",
      dynamic: "get_headless_channels.id.name",
    },
    extras || {}
  );
}

module.exports = getHeadlessChannelsField;
