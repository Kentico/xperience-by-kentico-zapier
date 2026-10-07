/* globals afterEach, afterAll */
/**
 * Registered through jest `setupFilesAfterEnv` so every unit test file gets the same
 * nock lifecycle. nock patches Node's http layer process-wide; doing this per file
 * leaks the first file's interceptor into the next file sharing the worker.
 */
const nock = require("nock");

nock.disableNetConnect();

afterEach(() => {
  nock.cleanAll();
});

afterAll(() => {
  nock.cleanAll();
  nock.restore();
});
