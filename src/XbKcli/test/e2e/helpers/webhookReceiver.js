const http = require("http");
const crypto = require("crypto");

/**
 * Stand-in for hooks.zapier.com: a local HTTP listener Xperience posts webhook payloads to.
 * Each subscription gets its own path so deliveries can be attributed to a trigger.
 */
class WebhookReceiver {
  constructor() {
    this.deliveries = []; // { hookId, body, headers, receivedAt }
    this.server = null;
    this.baseUrl = null;
  }

  async start(host = "127.0.0.1") {
    this.server = http.createServer((req, res) => {
      const chunks = [];
      req.on("data", (c) => chunks.push(c));
      req.on("end", () => {
        const raw = Buffer.concat(chunks).toString("utf8");
        let body;
        try {
          body = JSON.parse(raw);
        } catch {
          body = raw;
        }
        const hookId = req.url.replace(/^\/hook\//, "").split("?")[0];
        this.deliveries.push({ hookId, method: req.method, body, headers: req.headers, receivedAt: Date.now() });
        res.statusCode = 200;
        res.setHeader("Content-Type", "application/json");
        res.end(JSON.stringify({ status: "success" }));
      });
    });

    await new Promise((resolve) => this.server.listen(0, host, resolve));
    this.baseUrl = `http://${host}:${this.server.address().port}`;
    return this;
  }

  /** Returns a fresh, unique target URL to hand to performSubscribe as bundle.targetUrl. */
  newHookUrl() {
    const hookId = crypto.randomUUID();
    return { hookId, url: `${this.baseUrl}/hook/${hookId}` };
  }

  deliveriesFor(hookId) {
    return this.deliveries.filter((d) => d.hookId === hookId);
  }

  /** Resolves with the first delivery for hookId matching predicate, or rejects after timeoutMs. */
  waitFor(hookId, predicate = () => true, timeoutMs = 15000, pollMs = 200) {
    const started = Date.now();
    return new Promise((resolve, reject) => {
      const tick = () => {
        const match = this.deliveriesFor(hookId).find(predicate);
        if (match) return resolve(match);
        if (Date.now() - started > timeoutMs) {
          return reject(
            new Error(
              `No matching webhook delivery for ${hookId} within ${timeoutMs} ms. ` +
                `Received so far: ${JSON.stringify(this.deliveriesFor(hookId).map((d) => d.body)).slice(0, 2000)}`
            )
          );
        }
        setTimeout(tick, pollMs);
      };
      tick();
    });
  }

  /** Resolves true when no delivery matching predicate arrives within quietMs. */
  async assertSilent(hookId, predicate = () => true, quietMs = 4000) {
    await new Promise((r) => setTimeout(r, quietMs));
    return !this.deliveriesFor(hookId).some(predicate);
  }

  async stop() {
    if (!this.server) return;
    await new Promise((resolve) => this.server.close(resolve));
    this.server = null;
  }
}

module.exports = { WebhookReceiver };
