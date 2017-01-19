'use strict';

const Hapi = require('hapi');
const config = require('./config');

const server = new Hapi.Server({
    app: {
        config: config
    }
});

server.connection({
    port: config.connection.port,
    routes: {
        cors: true
    }
});

const plugins = [{
        register: require('./elastic'),
        options: config.elastic
    },
    require('hapi-auth-jwt2'),
    require('./search')
];

server.register(plugins, (err) => {

    require('./auth')(server, config);

    server.start((err) => {

        if (err) {
            throw err;
        }

        console.log(`search server running at: ${server.info.uri}`);
    });
});