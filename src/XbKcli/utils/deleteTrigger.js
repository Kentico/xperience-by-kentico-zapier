async function deleteTrigger(z, bundle) {
  const webhook = bundle.subscribeData;
  const options = {
    url: `${bundle.authData.website}/zapier/triggers/${webhook.triggerId}`,
    method: "DELETE",
    headers: {
      Accept: "application/json",
    },
  };

  const response = await z.request(options);

  return response.status === 200;
}

module.exports = deleteTrigger;
