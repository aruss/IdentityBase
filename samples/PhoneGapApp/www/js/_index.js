// Configration ----------------------------------------------------------------
// -----------------------------------------------------------------------------

var identityUri = 'http://10.70.17.203:5000';
var clientId = 'phonegapclient';
var redirectUri = 'http://localhost';
var responseType = 'id_token token';
var scope = 'api1 openid';


var apiUri = 'http://10.70.17.203:3721';

var isCordova = typeof cordova !== 'undefined';

// dev hack
if (!isCordova) {

    identityUri = 'http://localhost:5000';
    redirectUri = 'http://localhost:3000';
    apiUri = 'http://localhost:3721';
}


// Cordova app -----------------------------------------------------------------
// -----------------------------------------------------------------------------

function initialize() {

    console.log('isCordova: ' + isCordova)
    if (isCordova) {

        document.addEventListener('deviceready', this.onDeviceReady, false);
    } else {

        onDeviceReady();
    }
};

function onDeviceReady() {

    if (isCordova) {

        window.open = cordova.InAppBrowser.open;
    } else {

        initializeAuth();
    }

    var elm = document.getElementById('deviceready');

    elm.querySelector('.listening').setAttribute('style', 'display:none;');
    elm.querySelector('.received').setAttribute('style', 'display:block;');

    if (isAuthorized()) {

        elm.querySelector('.public').setAttribute('style', 'display:none;');
        elm.querySelector('.private').setAttribute('style', 'display:block;');

        document.getElementById('getapidata').addEventListener('click', function () {

            getApiResults(function (err, xhr) {

                document.getElementById('getapidata-result').innerHTML =
                    JSON.stringify(xhr.result, null, 4);
            });
            return false;
        }, false);

        document.getElementById('getprofile').addEventListener('click', function () {

            getProfile(function (err, xhr) {

                document.getElementById('getprofile-result').innerHTML =
                    JSON.stringify(xhr.result, null, 4);
            });
            return false;
        }, false);

        document.getElementById('signout').addEventListener('click', function () {

            signout();
            return false;
        }, false);
    } else {

        document.getElementById('signin').addEventListener('click', function () {

            setTimeout(function () {

                authorizeAsync();
            });
            return false;
        }, false);
    }
};

// Loads data from resource api
function getApiResults(cb) {

    getJson(apiUri + '/identity', function (err, xhr) {

        cb(err, xhr);
    });
}

// HTTP client -----------------------------------------------------------------
// -----------------------------------------------------------------------------

function getJson(uri, cb) {

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {

        if (xhr.readyState === 4) {

            if (xhr.status === 401) {

                cb(new Error('unauthorized'), xhr);
            } else if (xhr.status >= 500 && xhr.status < 600) {

                cb(new Error('server error'), xhr);
            } else {
                try {

                    xhr.result = JSON.parse(xhr.responseText);
                    cb(null, xhr);
                } catch (e) {

                    cb(e, xhr);
                }
            }
        }
    };

    xhr.open('GET', uri, true);
    xhr.uri = uri;
    xhr.timeout = 120000;
    xhr.withCredentials = false;

    var token = localStorage.getItem('authToken');
    xhr.setRequestHeader('Authorization', 'Bearer ' + token);
    xhr.setRequestHeader('Accept', 'application/json');
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(null);
}

// Security Service ------------------------------------------------------------
// -----------------------------------------------------------------------------

// Loads profile from identity server
function getProfile(cb) {

    getJson(identityUri + '/connect/userinfo', function (err, xhr) {

        cb(err, xhr);
    });
}

// Decodes base 64 string
function base64Decode(str) {

    var output = str.replace('-', '+').replace('_', '/');
    switch (output.length % 4) {
        case 0:
            break;
        case 2:
            output += '==';
            break;
        case 3:
            output += '=';
            break;
        default:
            throw 'Illegal base64url string!';
    }

    return window.atob(output);
}

// Decodes and parses the token
function parseToken(token) {

    if (token) {

        var encoded = token.split('.')[1];
        return JSON.parse(base64Decode(encoded));
    }
}

// Checks if user is authenticated
function initializeAuthAsync() {

    return new Promise(function (resolve, reject) {

        clearAuthData();
        if (window.location.hash.indexOf('id_token') > -1) {

            authorizeCallback();
            resolve();
        } else {

            return authorizeAsync()
                .then(function () {

                    resolve();
                }, function () {

                    reject();
                });
        }
    });
}

// Checks if token is present, does not indicate if token is valid or session
// is still active
function isAuthorized() {

    return localStorage.getItem('authToken') !== '';
}

// Makes a authentication request
function authorizeAsync() {

    return new Promise(function (resolve, reject) {

        var nonce = 'N' + Math.random() + '' + Date.now();
        var state = Date.now() + '' + Math.random();
        var uri = identityUri + '/connect/authorize?' +
            'response_type=' + encodeURIComponent(responseType) + '&' +
            'client_id=' + encodeURIComponent(clientId) + '&' +
            'redirect_uri=' + encodeURIComponent(redirectUri) + '&' +
            'scope=' + encodeURIComponent(scope) + '&' +
            'nonce=' + encodeURIComponent(nonce) + '&' +
            'state=' + encodeURIComponent(state);

        localStorage.setItem('authNonce', nonce);
        localStorage.setItem('authStateControl', state);

        // In case of cordova use inappbrowser
        if (isCordova) {

            var authWindow = window.open(uri, '_blank', 'location=no,toolbar=no');
            authWindow.addEventListener('loadstart', function (e) {

                // url should look like http://localhost/#id_token=...
                if (e.url.indexOf(redirectUri + '/#') !== 0) {

                    return;
                }
                authWindow.close();

                // It might contain a error code
                var error = /\#error=(.+)$/.exec(e.url);
                if (error) {

                    alert('error: ' + error);
                    reject();
                } else {

                    authorizeCallback(e.url);
                    resolve();
                }
            });

            // In case of a browser just redirect
        } else {

            window.location = uri;
        }
    });
}

// handles authorization callback
function authorizeCallback(url) {

    var hash = (url || window.location.toString()).split('#')[1];

    var result = hash
        .split('&')
        .reduce(function (result, item) {

            var parts = item.split('=');
            result[parts[0]] = parts[1];
            return result;
        }, {});

    var responseIsValid = false;

    if (!result.error) {
        if (result.state === localStorage.getItem('authStateControl')) {

            var idTokenData = parseToken(result.id_token);

            // validate nonce
            if (idTokenData.nonce === localStorage.getItem('authNonce')) {

                localStorage.setItem('authNonce', '');
                localStorage.setItem('authStateControl', '');

                responseIsValid = true;
            } else {

                console.log('AuthorizedCallback incorrect nonce');
            }
        } else {

            console.log('AuthorizedCallback incorrect state');
        }
    } else {

        console.log(result.error);
    }

    if (responseIsValid) {

        setAuthData(result.access_token, result.id_token);
    } else {

        clearAuthData();
    }
}

function clearAuthData() {

    localStorage.setItem('authToken', '');
    localStorage.setItem('authIdToken', '');
}

function setAuthData(token, id_token) {

    if (localStorage.getItem('authToken') !== '') {

        localStorage.setItem('authToken', '');
    }

    localStorage.setItem('authToken', token);
    localStorage.setItem('authIdToken', id_token);
}
















function authorizeCallbackXXX(url) {

    console.log('AuthorizedController created, has hash');
    var hash = window.location.hash.substr(1);

    var result = hash.split('&').reduce(function (result, item) {
        var parts = item.split('=');
        result[parts[0]] = parts[1];
        return result;
    }, {});

    var token = '';
    var id_token = '';
    var authResponseIsValid = false;

    if (!result.error) {
        if (result.state === localStorage.getItem('authStateControl')) {

            token = result.access_token;
            id_token = result.id_token
            var idTokenData = parseToken(id_token);

            // validate nonce
            if (idTokenData.nonce === localStorage.getItem('authNonce')) {
                localStorage.setItem('authNonce', '');
                localStorage.setItem('authStateControl', '');

                authResponseIsValid = true;
                console.log('AuthorizedCallback state and nonce validated, returning access token');
            } else {

                console.log('AuthorizedCallback incorrect nonce');
            }
        } else {

            console.log('AuthorizedCallback incorrect state');
        }
    } else {

        console.log(result.error);
    }

    if (authResponseIsValid) {

        setAuthData(token, id_token);
        console.log(localStorage.getItem('authToken'));
    } else {
        clearAuthData();
    }
}

























function signout() {

    var idToken = localStorage.getItem('authIdToken');
    var state = Date.now() + "" + Math.random();

    var uri = identityUri + "/connect/endsession?" +
        "id_token_hint=" + idToken + "&" +
        "post_logout_redirect_uri=" + encodeURIComponent(redirectUri) + "&" +
        "state=" + encodeURIComponent(state);

    clearAuthData();

    window.location = uri;
}