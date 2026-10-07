#!/usr/bin/env node
/**
 * Launcher for the E2E contract suite (`npm run test:e2e`).
 *
 * Prepares the *process* environment before Jest starts, because Jest gives each
 * test file a sandboxed copy of process.env and Node's TLS layer only reads the real one:
 *  - loads XBYK_URL / ZAPIER_API_KEY from a local `.env` (git-ignored) if present,
 *  - falls back to ASPNETCORE_URLS for XBYK_URL (what the GitHub E2E workflow sets),
 *  - disables TLS verification for https://localhost only (self-signed Kestrel dev certificate),
 * then runs Jest in band and exits with its exit code.
 */
const path = require("path");
const { spawnSync } = require("child_process");

const zapier = require("zapier-platform-core");

zapier.tools.env.inject(path.join(__dirname, "..", "..", ".env"));

const env = { ...process.env };

if (!env.XBYK_URL && env.ASPNETCORE_URLS) {
  env.XBYK_URL = env.ASPNETCORE_URLS.split(";")[0];
}

if (env.XBYK_URL && /^https:\/\/(localhost|127\.0\.0\.1)(:\d+)?\/?$/i.test(env.XBYK_URL)) {
  env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
}

const jestBin = require.resolve("jest/bin/jest");
const result = spawnSync(
  process.execPath,
  [jestBin, "--config", path.join(__dirname, "..", "..", "jest.e2e.config.js"), "--runInBand", ...process.argv.slice(2)],
  { stdio: "inherit", env }
);

process.exit(result.status ?? 1);
