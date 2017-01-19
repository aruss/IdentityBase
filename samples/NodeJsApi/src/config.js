'use strict';

const argv = require('minimist')(process.argv.slice(2));
const config = JSON.parse(require('safe-json-stringify')(require('config')));

// extend configuration
config.environment = argv.env || process.env.NODE_ENV || 'development';
config.isDevelopment = config.environment === 'development';
config.version = require('../package').version;

module.exports = config;