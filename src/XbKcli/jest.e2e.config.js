/**
 * End-to-end contract tests: run the real Zapier app code against a live
 * Xperience by Kentico instance. Start them through `npm run test:e2e`
 * (test/e2e/run.js), which prepares the process environment. See test/e2e/README.md.
 */
module.exports = {
  testEnvironment: "node",
  roots: ["<rootDir>/test/e2e"],
  testMatch: ["**/*.e2e.test.js"],
  testTimeout: 60000,
};
