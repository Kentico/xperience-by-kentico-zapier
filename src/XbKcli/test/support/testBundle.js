/**
 * Shared test fixtures for the Zapier CLI app.
 * No real host is contacted: unit tests mock HTTP with nock.
 */
const WEBSITE = "https://xbyk.example.test";
const API_KEY = "unit-test-api-key";
const AUTH_HEADER = `XbyKZapierApiKey ${API_KEY}`;
const TARGET_URL = "https://hooks.zapier.com/hooks/standard/1/abc/";

const authData = () => ({ website: WEBSITE, apiKey: API_KEY });

/** Minimal form definition as returned by GET /zapier/actions/biz-form/{classname} (JSON-encoded XML). */
const FORM_DEFINITION_XML =
  "<form>" +
  '<field column="ContactUsID" columntype="integer" isPK="true" />' +
  '<field column="FormInserted" columntype="datetime" system="true" />' +
  '<field column="FormUpdated" columntype="datetime" system="true" />' +
  '<field column="ItemGuid" columntype="guid" />' +
  '<field column="UserFirstName" columntype="text" columnsize="200" visible="true">' +
  "<properties><fieldcaption>First name</fieldcaption><fielddescription>Your first name</fielddescription></properties>" +
  "</field>" +
  '<field column="UserEmail" columntype="text" columnsize="200" visible="true">' +
  "<properties><fieldcaption>Email</fieldcaption></properties>" +
  "</field>" +
  '<field column="UserMessage" columntype="longtext" allowempty="true" visible="true">' +
  "<properties><fieldcaption>Message</fieldcaption><explanationtext>Optional</explanationtext></properties>" +
  "</field>" +
  '<field column="Subscribe" columntype="boolean" allowempty="true" visible="true">' +
  "<properties><defaultvalue>false</defaultvalue></properties>" +
  "</field>" +
  '<field column="Attachment" columntype="binary" allowempty="true" visible="true" />' +
  "</form>";

module.exports = { WEBSITE, API_KEY, AUTH_HEADER, TARGET_URL, authData, FORM_DEFINITION_XML };
