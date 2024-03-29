const getLanguageNamesField = require("../fields/getLanguageNamesField");
const getClassContentTypeTypesField = require("../fields/getClassContentTypeTypesField");
const getReusableTypesField = require("../fields/getReusableTypesField");
const getReusableItemsField = require("../fields/getReusableItemsField");
const getWebsiteItemsField = require("../fields/getWebsiteItemsField");
const getWebsiteTypesField = require("../fields/getWebsiteTypesField");
const getWebsiteChannelsField = require("../fields/getWebsiteChannelsField");
const getHeadlessItemsField = require("../fields/getHeadlessItemsField");
const getHeadlessTypesField = require("../fields/getHeadlessTypesField");
const getHeadlessChannelsField = require("../fields/getHeadlessChannelsField");
const ClassContentTypeType = require("../utils/enums");

async function execute(z, bundle) {
  let url = "";
  const inputData = bundle.inputData;
  if (inputData.classContentTypeType == ClassContentTypeType.WEBSITE) {
    url = `${ClassContentTypeType.WEBSITE}/${inputData.websiteChannelId}/${inputData.pageId}/${inputData.languageName}`;
  } else if (inputData.classContentTypeType == ClassContentTypeType.REUSABLE) {
    url = `${ClassContentTypeType.REUSABLE}/${inputData.reusableItemId}/${inputData.languageName}`;
  } else if (inputData.classContentTypeType == ClassContentTypeType.HEADLESS) {
    url = `${ClassContentTypeType.HEADLESS}/${inputData.headlessChannelId}/${inputData.headlessItemId}/${inputData.languageName}`;
  }

  const options = {
    url: `${bundle.authData.website}/zapier/actions/publish/${url}`,
    method: "POST",
    headers: {
      Accept: "application/json",
    },
  };

  const response = await z.request(options);
  return z.JSON.parse(response.content);
}

async function makeFieldsForWebsite(z, bundle) {
  const res = [];
  res.push(
    getWebsiteChannelsField({
      required: true,
      altersDynamicFields: true,
    })
  );
  if (bundle.inputData.websiteChannelId != undefined) {
    res.push(
      getWebsiteTypesField({
        required: true,
        altersDynamicFields: true,
      })
    );
    if (
      bundle.inputData.languageName != undefined &&
      bundle.inputData.websiteClassname != undefined
    ) {
      res.push(
        getWebsiteItemsField({
          required: true,
          altersDynamicFields: true,
        })
      );
    }
  }
  return res;
}

async function makeFieldsForReusable(z, bundle) {
  const res = [];
  res.push(
    getReusableTypesField({
      required: true,
      altersDynamicFields: true,
    })
  );
  if (
    bundle.inputData.languageName != undefined &&
    bundle.inputData.reusableClassname != undefined
  ) {
    res.push(
      getReusableItemsField({
        required: true,
        altersDynamicFields: true,
      })
    );
  }
  return res;
}

async function makeFieldsForHeadless(z, bundle) {
  const res = [];
  res.push(
    getHeadlessChannelsField({
      required: true,
      altersDynamicFields: true,
    })
  );
  if (bundle.inputData.headlessChannelId != undefined) {
    res.push(
      getHeadlessTypesField({
        required: true,
        altersDynamicFields: true,
      })
    );
    if (
      bundle.inputData.languageName != undefined &&
      bundle.inputData.headlessClassname != undefined
    ) {
      res.push(
        getHeadlessItemsField({
          required: true,
          altersDynamicFields: true,
        })
      );
    }
  }
  return res;
}

const publishAction = {
  noun: "Publish action",
  display: {
    hidden: false,
    description: "Publishes a content type in Xbk",
    label: "Publish Content Type",
  },
  key: "publishAction",
  operation: {
    perform: execute,
    inputFields: [
      getClassContentTypeTypesField({
        required: true,
        altersDynamicFields: true,
      }),
      getLanguageNamesField({
        required: true,
        altersDynamicFields: true,
      }),
      async function (z, bundle) {
        if (
          bundle.inputData.classContentTypeType == ClassContentTypeType.WEBSITE
        ) {
          return await makeFieldsForWebsite(z, bundle);
        }
        if (
          bundle.inputData.classContentTypeType == ClassContentTypeType.REUSABLE
        ) {
          return await makeFieldsForReusable(z, bundle);
        }
        if (
          bundle.inputData.classContentTypeType == ClassContentTypeType.HEADLESS
        ) {
          return await makeFieldsForHeadless(z, bundle);
        }
      },
    ],
    sample: {
      classname: "BizForm.DancingGoatContactUs",
    },
  },
};

module.exports = publishAction;
