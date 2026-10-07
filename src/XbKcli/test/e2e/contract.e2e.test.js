/* globals describe, it, expect, beforeAll, afterAll */
/**
 * E2E contract tests: the real Zapier app code (triggers, creates, dropdowns, auth)
 * against a live Xperience by Kentico instance, with a local HTTP listener standing
 * in for hooks.zapier.com. Covers the whole loop Zapier itself cannot exercise
 * locally for REST hooks: subscribe -> event in Xperience -> webhook delivery -> unsubscribe.
 *
 * Requires XBYK_URL and ZAPIER_API_KEY (see run.js / README.md).
 * Set E2E_REQUIRE_WORKFLOW=1 to fail (instead of skip) when the instance has no
 * content type with a workflow for the move-to-step trigger.
 */
const { WebhookReceiver } = require("./helpers/webhookReceiver");
const { App, appTester, authData, buildFormInput, payloadContains } = require("./helpers/xbyk");

const FORM_CLASS = "BizForm.DancingGoatContactUs";
const DELIVERY_TIMEOUT_MS = 20000;

const receiver = new WebhookReceiver();
const openSubscriptions = []; // [{ trigger, subscribeData }] to clean up in afterAll

const subscribe = async (trigger, inputData, hookUrl) => {
  const subscribeData = await appTester(trigger.operation.performSubscribe, {
    authData: authData(),
    inputData,
    targetUrl: hookUrl,
  });
  openSubscriptions.push({ trigger, subscribeData });
  return subscribeData;
};

const unsubscribe = async (trigger, subscribeData) => {
  const index = openSubscriptions.findIndex((s) => s.subscribeData === subscribeData);
  if (index >= 0) openSubscriptions.splice(index, 1);
  return appTester(trigger.operation.performUnsubscribe, { authData: authData(), subscribeData });
};

const insertFormRecord = async (marker) => {
  const fields = await appTester(App.creates.insertFormRecordAction.operation.inputFields[1], {
    authData: authData(),
    inputData: { classname: FORM_CLASS },
  });
  const { input, markerField } = buildFormInput(fields, FORM_CLASS, marker);
  const result = await appTester(App.creates.insertFormRecordAction.operation.perform, {
    authData: authData(),
    inputData: input,
  });
  return { result, markerField, input };
};

beforeAll(async () => {
  authData(); // fail fast with a readable message when env is missing
  await receiver.start();
});

afterAll(async () => {
  for (const { trigger, subscribeData } of [...openSubscriptions]) {
    try {
      await unsubscribe(trigger, subscribeData);
    } catch (e) {
      console.warn(`Cleanup: could not unsubscribe ${JSON.stringify(subscribeData)}: ${e.message}`);
    }
  }
  await receiver.stop();
});

describe("authentication", () => {
  it("accepts the configured API key", async () => {
    const response = await appTester(App.authentication.test, { authData: authData() });

    expect(response.status).toBe(200);
    expect(response.data).toHaveProperty("message");
    expect(response.data).toHaveProperty("channel");
  });

  it("rejects a wrong API key with the mapped message", async () => {
    await expect(
      appTester(App.authentication.test, { authData: authData({ apiKey: "definitely-wrong" }) })
    ).rejects.toThrow("The API Key you supplied is incorrect.");
  });
});

describe("dynamic dropdowns", () => {
  it("lists the allowed forms including the Dancing Goat contact form", async () => {
    const forms = await appTester(App.triggers.get_form_class_names.operation.perform, { authData: authData() });

    expect(forms.map((f) => f.id)).toContain(FORM_CLASS);
    expect(forms.every((f) => f.id && f.name)).toBe(true);
  });

  it("lists event log severities as I / W / E", async () => {
    const severities = await appTester(App.triggers.get_event_log_severity.operation.perform, { authData: authData() });

    expect(severities.map((s) => s.id).sort()).toEqual(["E", "I", "W"]);
  });

  it("lists content types that have a workflow as id/name pairs (empty allowed unless E2E_REQUIRE_WORKFLOW=1)", async () => {
    const types = await appTester(App.triggers.get_content_types_objects.operation.perform, { authData: authData() });

    if (process.env.E2E_REQUIRE_WORKFLOW === "1") expect(types.length).toBeGreaterThan(0);
    for (const t of types) expect(t).toEqual({ id: expect.any(String), name: expect.any(String) });
  });
});

describe("New Form Submission trigger performList", () => {
  it("returns a real submission (not the server's type-info placeholder) shaped like the webhook payload", async () => {
    const { result } = await insertFormRecord(`e2e-performlist-${Date.now()}`); // guarantees at least one record exists
    expect(result.id).toBeGreaterThan(0);

    const [sample] = await appTester(App.triggers.form_submission.operation.performList, {
      authData: authData(),
      inputData: { classname: FORM_CLASS },
    });

    expect(sample).toHaveProperty("FormInserted");
    expect(sample).not.toHaveProperty("FormUpdated");
    // ZapierTriggerService falls back to a type-info sample whose string columns are this placeholder.
    expect(Object.values(sample)).not.toContain("text or serialized content");
  });
});

describe("New Form Submission trigger (REST hook round trip)", () => {
  it("delivers a webhook for a new submission and stops after unsubscribe", async () => {
    const trigger = App.triggers.form_submission;
    const hook = receiver.newHookUrl();
    const control = receiver.newHookUrl(); // stays subscribed; proves events keep flowing after unsubscribe

    const subscribeData = await subscribe(trigger, { classname: FORM_CLASS }, hook.url);
    const controlSubscription = await subscribe(trigger, { classname: FORM_CLASS }, control.url);
    expect(subscribeData.triggerId).toBeGreaterThan(0);
    expect(controlSubscription.triggerId).not.toBe(subscribeData.triggerId);

    const marker = `e2e-hook-${Date.now()}`;
    const { markerField } = await insertFormRecord(marker);

    const delivery = await receiver.waitFor(hook.hookId, (d) => payloadContains(d.body, marker), DELIVERY_TIMEOUT_MS);
    expect(delivery.method).toBe("POST");
    expect(delivery.body[markerField]).toBe(marker);
    expect(delivery.body).toHaveProperty("FormInserted");

    // perform() is what Zapier runs on delivery: it must pass the payload through unchanged.
    const performed = await appTester(trigger.operation.perform, {
      authData: authData(),
      inputData: { classname: FORM_CLASS },
      cleanedRequest: delivery.body,
    });
    expect(performed).toEqual([delivery.body]);

    expect(await unsubscribe(trigger, subscribeData)).toBe(true);

    const afterMarker = `e2e-after-unsubscribe-${Date.now()}`;
    await insertFormRecord(afterMarker);
    // The control hook receiving the second submission proves the event fired and was delivered...
    await receiver.waitFor(control.hookId, (d) => payloadContains(d.body, afterMarker), DELIVERY_TIMEOUT_MS);
    // ...so the unsubscribed hook staying silent is meaningful.
    expect(await receiver.assertSilent(hook.hookId, (d) => payloadContains(d.body, afterMarker), 1000)).toBe(true);

    expect(await unsubscribe(trigger, controlSubscription)).toBe(true);
  });

  it("unsubscribe is idempotent for an already deleted trigger", async () => {
    const trigger = App.triggers.form_submission;
    const hook = receiver.newHookUrl();
    const subscribeData = await subscribe(trigger, { classname: FORM_CLASS }, hook.url);

    expect(await unsubscribe(trigger, subscribeData)).toBe(true);
    expect(await unsubscribe(trigger, subscribeData)).toBe(true);
  });
});

describe("New Event Log Entry trigger (REST hook round trip)", () => {
  it("delivers the Information REGISTER entry written when another trigger is registered", async () => {
    const trigger = App.triggers.event_log_create;
    const hook = receiver.newHookUrl();

    const subscribeData = await subscribe(trigger, { severity: ["I"] }, hook.url);
    expect(subscribeData.triggerId).toBeGreaterThan(0);

    // Registering any other trigger writes an Information "REGISTER" entry to the event log.
    const formTrigger = App.triggers.form_submission;
    const formSubscription = await subscribe(formTrigger, { classname: FORM_CLASS }, receiver.newHookUrl().url);

    const delivery = await receiver.waitFor(
      hook.hookId,
      (d) =>
        d.body &&
        d.body.EventType === "I" &&
        d.body.EventCode === "REGISTER" &&
        d.body.Source === "ZapierTriggerHandler" &&
        String(d.body.EventDescription).includes(FORM_CLASS), // the entry written for the form trigger, not our own
      DELIVERY_TIMEOUT_MS
    );
    expect(delivery.body).toHaveProperty("EventTime");
    expect(delivery.body).toHaveProperty("EventDescription");

    const [sample] = await appTester(trigger.operation.performList, { authData: authData(), inputData: {} });
    expect(sample).toHaveProperty("EventType");
    expect(sample).toHaveProperty("EventCode");

    expect(await unsubscribe(formTrigger, formSubscription)).toBe(true);
    expect(await unsubscribe(trigger, subscribeData)).toBe(true);
  });
});

describe("Content Moves to a Workflow Step trigger", () => {
  const requireWorkflow = process.env.E2E_REQUIRE_WORKFLOW === "1";

  it("subscribes, lists sample data and unsubscribes for the first content type with a custom workflow step (skipped when none exists)", async () => {
    const types = await appTester(App.triggers.get_content_types_objects.operation.perform, { authData: authData() });
    if (types.length === 0) {
      if (requireWorkflow) throw new Error("E2E_REQUIRE_WORKFLOW=1 but no content type has a workflow");
      console.warn("No content type with a workflow in this instance; move_to_step subscribe/performList not exercised.");
      return;
    }
    const objectType = types[0].id;
    const steps = await appTester(App.triggers.get_event_types.operation.perform, {
      authData: authData(),
      inputData: { objectType },
    });
    if (steps.length === 0) {
      if (requireWorkflow) throw new Error(`E2E_REQUIRE_WORKFLOW=1 but ${objectType} has no custom workflow step`);
      console.warn(`Content type ${objectType} has no custom workflow step; move_to_step subscribe/performList not exercised.`);
      return;
    }
    const eventType = steps[0].id;
    const trigger = App.triggers.move_to_step;

    const subscribeData = await subscribe(trigger, { objectType, eventType }, receiver.newHookUrl().url);
    expect(subscribeData.triggerId).toBeGreaterThan(0);

    // performList returns the static workflow sample from the server for custom steps; only its shape is checked.
    const [sample] = await appTester(trigger.operation.performList, {
      authData: authData(),
      inputData: { objectType, eventType },
    });
    expect(Object.keys(sample)).toEqual(
      expect.arrayContaining(["DisplayName", "ContentTypeName", "StepName", "OriginalStepName", "UserName", "AdminLink"])
    );

    expect(await unsubscribe(trigger, subscribeData)).toBe(true);
  });
});
