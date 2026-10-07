const zapier = require("zapier-platform-core");

const App = require("../../../index");

const appTester = zapier.createAppTester(App);

function requireEnv(name) {
  const value = process.env[name];
  if (!value) {
    throw new Error(
      `${name} is not set. The E2E contract suite needs a running Xperience by Kentico instance with the Zapier ` +
        `integration enabled. Set XBYK_URL (e.g. https://localhost:14070) and ZAPIER_API_KEY ` +
        `(see scripts/New-ZapierApiKey.ps1), or put them in src/XbKcli/.env.`
    );
  }
  return value;
}

function authData(overrides = {}) {
  return {
    website: requireEnv("XBYK_URL").replace(/\/+$/, ""),
    apiKey: requireEnv("ZAPIER_API_KEY"),
    ...overrides,
  };
}

/**
 * Builds inputData for insertFormRecordAction from the live form schema:
 * every required field gets a value of the right type, `markerField` gets `marker`.
 */
function buildFormInput(fields, classname, marker) {
  const input = { classname };
  const textField = fields.find((f) => f.type === "string" || f.type === "text");
  if (!textField) {
    throw new Error(`Form ${classname} has no text field to carry the E2E marker`);
  }

  for (const field of fields) {
    if (!field.required && field.key !== textField.key) continue;
    switch (field.type) {
      case "string":
        input[field.key] = field.key.toLowerCase().includes("email") ? `e2e-${Date.now()}@example.test` : marker;
        break;
      case "text":
        input[field.key] = marker;
        break;
      case "integer":
        input[field.key] = 1;
        break;
      case "number":
        input[field.key] = 1.5;
        break;
      case "boolean":
        input[field.key] = true;
        break;
      case "datetime":
        input[field.key] = new Date().toISOString();
        break;
      default:
        input[field.key] = marker;
    }
  }
  input[textField.key] = marker;
  return { input, markerField: textField.key };
}

/** True when any value in the payload equals marker. */
function payloadContains(body, marker) {
  return body && typeof body === "object" && Object.values(body).some((v) => v === marker);
}

module.exports = { App, appTester, authData, buildFormInput, payloadContains };
