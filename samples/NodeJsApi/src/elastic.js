'use strict';

const ElasticSearch = require('elasticsearch');

exports.register = function (server, options, next) {

    const client = new ElasticSearch.Client(options);

    client.ping({
        requestTimeout: Infinity,
        hello: 'elasticsearch!'
    }, function (error) {
        if (error) {

            console.log('elasticsearch cluster is down!');
        } else {

            console.log('connected to elasticsearch cluster');

            return next();
        }
    });

    // server.plugins.elastic.client === client
    server.expose('client', client);
};

exports.register.attributes = {
    name: 'elastic'
};