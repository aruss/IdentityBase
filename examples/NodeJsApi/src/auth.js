'use strict';

const Fs = require('fs');

module.exports = function (server, config) {

    var cert = Fs.readFileSync('./idsvr3test.pem').toString();

    server.auth.strategy('jwt', 'jwt', {
        key: cert,
        validateFunc: (decoded, request, callback) => {

            // Call the IdentityServer to verify token if required
            // Should do it anyways if using reference tokens
            // https://leastprivilege.com/2015/11/25/reference-tokens-and-introspection/  
            return callback(null, true);
        },
        verifyOptions: {
            algorithms: ['RS256']
        }
    });

    server.auth.default('jwt');
};