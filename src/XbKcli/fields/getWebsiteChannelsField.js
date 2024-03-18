function getWebsiteChannelsField(extras) {
  return Object.assign(
    {
      label: "Website channel",
      key: "websiteChannelId",
      required: true,
      type: "string",
      dynamic: "get_website_channels.id.name",
    },
    extras || {}
  );
}

module.exports = getWebsiteChannelsField;
