function getSimpleField(field) {
  function getField(field, extra) {
    const base = {
      key: field.column,
      label: `${field.fieldcaption ? field.fieldcaption : field.column}`,
      required: !field.allowempty,
      helpText: field.fielddescription || field.explanationtext,
      default:
        field.defaultvalue && typeof field.defaultvalue === "string"
          ? field.defaultvalue
          : "",
    };

    // Add extras
    let retVal = Object.assign(base, extra);

    // Add current datetime if is datetime field with no default value
    if (retVal.type === "datetime" && retVal.default === "") {
      retVal.default = new Date().toLocaleString();
    }

    return retVal;
  }

  switch (field.columntype) {
    case "text":
    case "guid":
      return getField(field, { type: "string" });

    case "longtext":
    case "richtexthtml":
      return getField(field, { type: "text" });

    case "integer":
    case "longinteger":
      return getField(field, { type: "integer" });

    case "decimal":
    case "double":
    case "float":
      return getField(field, { type: "number" });

    case "datetime":
      return getField(field, { type: "datetime" });

    case "boolean":
      return getField(field, { type: "boolean" });
  }
}

module.exports = getSimpleField;
