'use strict';

const Joi = require('joi');

exports.register = function (server, options, next) {

    // https://blog.madewithlove.be/post/faceted-search-using-elasticsearch/
    server.route({
        method: 'GET',
        path: '/_search',
        config: {
            validate: {
                query: {
                    q: Joi.string().min(3).max(100),
                    t: Joi.number().min(1).max(50).default(20),
                    s: Joi.number().min(0).max(10000).default(0)
                }
            }
        },
        handler: (request, reply) => {

            const client = server.plugins.elastic.client;

            const term = request.query.q;
            const take = request.query.t; // take/size
            const skip = request.query.s; // skip/from

            var query = {
                "and": [{
                    "terms": {
                        "title": [term]
                    }
                }]
            };

            client.search({
                index: 'products',
                type: 'product',
                body: {
                    from: skip,
                    size: take,
                    "query": query,
                    "aggregations": {
                        "all_products": {
                            "global": {},
                            "aggregations": {
                                "countries": {
                                    "filter": query,
                                    "aggregations": {
                                        "filtered_countries": {
                                            "terms": {
                                                "field": "country"
                                            }
                                        }
                                    }
                                },
                                "categories": {
                                    "filter": query,
                                    "aggregations": {
                                        "filtered_categories": {
                                            "terms": {
                                                "field": "category"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }).then((resp) => {

                var response = {
                    take: take,
                    skip: skip,
                    total: resp.hits.total,
                    count: resp.hits.hits.length,
                    items: resp.hits.hits.map((v) => v._source),
                    facets: resp.aggregations
                };

                return reply(response);

            }, (err) => {

                return reply(err);
            });
        }
    });

    next();
};

exports.register.attributes = {
    name: 'search'
};