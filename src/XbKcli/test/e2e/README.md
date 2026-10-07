# E2E contract tests

These tests run the real Zapier app code (`src/XbKcli`) through `createAppTester`
against a live Xperience by Kentico instance. A local HTTP listener plays the role
of `hooks.zapier.com`, so the full REST hook loop is covered without a Zapier account:

1. `performSubscribe` registers a trigger in Xperience with the listener's URL,
2. the test causes the event (for example inserts a form record via the
   *Insert Form Record* action),
3. Xperience posts the webhook to the listener and the payload is asserted,
4. `performUnsubscribe` removes the trigger and a second event must stay silent.

`zapier invoke` cannot do this for hook triggers, which is why this suite exists.

## Prerequisites

- A running instance with `AddKenticoZapier()` (the `examples/DancingGoat` app),
  reachable at `XBYK_URL` (for example `https://localhost:14070`).
- An API key. Generate one in the admin UI (Configuration > Zapier > API key) or,
  for automation, insert one directly into the database **before the app starts**
  (the key hash is cached for an hour once read):

  ```powershell
  cd scripts
  $key = ./New-ZapierApiKey.ps1   # prints and returns the plaintext key
  ```

## Run

```bash
cd src/XbKcli
XBYK_URL=https://localhost:14070 ZAPIER_API_KEY=<key> npm run test:e2e
```

Or put both values in `src/XbKcli/.env` (git-ignored). `ASPNETCORE_URLS` is used as a
fallback for `XBYK_URL`, which is what the GitHub E2E workflow sets.

`npm run test:e2e` goes through `test/e2e/run.js`, which prepares the process
environment and then runs Jest in band. For `https://localhost` it sets
`NODE_TLS_REJECT_UNAUTHORIZED=0` for the Jest process only, because Kestrel's
development certificate is self-signed (Jest sandboxes `process.env` per test file,
so this cannot be done from inside a test).

Set `E2E_REQUIRE_WORKFLOW=1` to fail instead of skip when the instance has no content
type with a custom workflow step (the move-to-step trigger is otherwise only covered
by its unit tests and the dropdown call).

## What it leaves behind

Form records inserted by the tests stay in the `BizForm.DancingGoatContactUs` form
(their text fields start with `e2e-`). Triggers are removed in `afterAll`.
