const ClassContentTypeType = require("../../utils/enums");

async function execute(z, bundle) {
  let type = "";

  if (bundle.inputData.classContentTypeType == ClassContentTypeType.WEBSITE) {
    type = bundle.inputData.websiteClassname;
  } else if (
    bundle.inputData.classContentTypeType == ClassContentTypeType.REUSABLE
  ) {
    type = bundle.inputData.reusableClassname;
  } else if (
    bundle.inputData.classContentTypeType == ClassContentTypeType.HEADLESS
  ) {
    type = bundle.inputData.headlessClassname;
  }

  const options = {
    url: `${bundle.authData.website}/zapier/actions/movetostep/steps/${type}`,
    method: "GET",
  };

  let result = await z.request(options);

  return z.JSON.parse(result.content);
}

module.exports = {
  key: "get_steps_for_item",
  noun: "Step selector",
  display: {
    label: "Step name",
    description: "Gets supported steps",
    hidden: true,
  },
  operation: {
    type: "polling",
    perform: execute,
    sample: {
      id: "DancingGoat.Coffee",
      name: "Boston coffee place",
    },
    outputFields: [
      {
        key: "id",
        label: "Step name",
        type: "string",
      },
      {
        key: "name",
        label: "Step display name",
        type: "string",
      },
    ],
  },
};
