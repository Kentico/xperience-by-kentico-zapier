/** Unit tests: fully offline, HTTP mocked with nock (lifecycle in test/support/nockSetup.js). */
module.exports = {
  testEnvironment: "node",
  roots: ["<rootDir>/test/unit"],
  testMatch: ["**/*.test.js"],
  setupFilesAfterEnv: ["<rootDir>/test/support/nockSetup.js"],
  testTimeout: 10000,
};
