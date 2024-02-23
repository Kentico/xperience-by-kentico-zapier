
async function execute(z, bundle) {

    const options = {
        url: `${bundle.authData.website}/zapier/data/objects`,
        method: 'GET'
    };

    let objectTypes = await z.request(options);

    return z.JSON.parse(objectTypes.content);
}


module.exports = {
    key: 'get_object_types',
    noun: 'Object type',
    display: {
        label: 'Object type',
        description: 'Gets supported object types',
        hidden: true,
    },
    operation: {
        type: 'polling',
        perform: execute,
        sample: {
            id: 'Contact',
            name: 'om.contact',
        },
        outputFields: [
            {
                key: 'name',
                label: 'Display name',
                type: 'string',
            },
            {
                key: 'id',
                label: 'Object type codename',
                type: 'string',
            }
        ]
    }
};