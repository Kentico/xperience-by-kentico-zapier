function getObjectTypesField(extras) {
    return Object.assign(
        {
            label: 'Object type',
            key: 'objectType',
            required: true,
            type: 'string',
            dynamic: 'get_object_types.id.name',
            altersDynamicFields: true,
        },
        extras || {},
    );
}

module.exports = getObjectTypesField;