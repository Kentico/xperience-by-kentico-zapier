function getFormClassNamesField(extras) {
  return Object.assign(
    {
      label: "Form",
      key: "classname",
      type: "string",
      dynamic: "get_form_class_names.id.name",
    },
    extras || {}
  );
}

module.exports = getFormClassNamesField;
