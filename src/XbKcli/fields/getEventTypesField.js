function getEventTypesField(extras) {
    return Object.assign(
        {
            label: 'Event type',
            key: 'eventType',
            required: true,
            type: 'string',
            dynamic: 'get_event_types.id.name',
        },
        extras || {},
    );
}

module.exports = getEventTypesField;