function getEventLogSeverityField(extras) {
  return Object.assign(
    {
      label: "Severity",
      key: "severity",
      required: true,
      type: "string",
      dynamic: "get_event_log_severity.id.name",
      list: true,
    },
    extras || {}
  );
}

module.exports = getEventLogSeverityField;
