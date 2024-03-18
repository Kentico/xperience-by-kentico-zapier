function getLanguageNamesField(extras) {
  return Object.assign(
    {
      label: "Language",
      key: "languageName",
      required: true,
      type: "string",
      dynamic: "get_language_names.id.name",
    },
    extras || {}
  );
}

module.exports = getLanguageNamesField;
