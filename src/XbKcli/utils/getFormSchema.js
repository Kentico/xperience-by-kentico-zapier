const parseString = require("xml2js").parseString;

async function getFormSchema(z, bundle, classname) {
  let retVal = [];
  if (!classname) return retVal;
  function makeField(fieldDefinition) {
    const fieldAttrs = fieldDefinition["$"];
    let fieldProps = fieldDefinition.properties;
    let field = {
      column: fieldAttrs.column,
      columntype: fieldAttrs.columntype,
      columnsize: fieldAttrs.columnsize,
      isPK: fieldAttrs.isPK || false,
      allowempty: fieldAttrs.allowempty || false,
      visible: fieldAttrs.visible && fieldAttrs.visible === "true",
      system: fieldAttrs.system && fieldAttrs.system === "true",
    };
    if (fieldProps) {
      fieldProps = fieldProps[0];
      (field.defaultvalue = fieldProps.defaultvalue
        ? fieldProps.defaultvalue[0]
        : undefined),
        (field.fieldcaption = fieldProps.fieldcaption
          ? fieldProps.fieldcaption[0]
          : undefined),
        (field.explanationtext = fieldProps.explanationtext
          ? fieldProps.explanationtext[0]
          : undefined),
        (field.fielddescription = fieldProps.fielddescription
          ? fieldProps.fielddescription[0]
          : undefined);
    }

    return field;
  }

  const options = {
    url: `${bundle.authData.website}/zapier/actions/biz-form/${classname}`,
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  };
  const response = await z.request(options);
  const classFormDefinition = z.JSON.parse(response.content);

  parseString(classFormDefinition, function (err, json) {
    const fields = json.form.field;
    if (fields && fields.length > 0) {
      retVal = fields.map(makeField);
    }
  });
  retVal = retVal.filter(
    (f) => !f.isPK && f.columntype !== "guid" && !f.system
  );
  return retVal;
}

module.exports = getFormSchema;
