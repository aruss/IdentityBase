// Cordova app -----------------------------------------------------------------
// -----------------------------------------------------------------------------

function initialize() {

    initializeAuth();
    document.addEventListener('deviceready', this.onDeviceReady, false);
};

function onDeviceReady() {

    var elm = document.getElementById('deviceready');

    elm.querySelector('.listening').setAttribute('style', 'display:none;');
    elm.querySelector('.received').setAttribute('style', 'display:block;');

    if (isAuthorized()) {

        elm.querySelector('.public').setAttribute('style', 'display:none;');
        elm.querySelector('.private').setAttribute('style', 'display:block;');

        document.getElementById('getapidata').addEventListener('click', function () {

            getApiResults(function(err, xhr) {

                document.getElementById('getapidata-result').innerHTML =
                    JSON.stringify(xhr.result, null, 4);
            });
            return false;
        }, false);

        document.getElementById('getprofile').addEventListener('click', function () {

            getProfile(function(err, xhr) {

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
                authorize();
            });
            return false;
        }, false);
    }
};

function getApiResults(cb) {

    getJson('http://localhost:3721/identity', function (err, xhr) {

        cb(err, xhr);
    });
}

// http client -----------------------------------------------------------------
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

var identityUri = 'http://localhost:5000';
var clientId = 'phonegapclient';
var redirectUri = 'http://localhost:3000';
var responseType = 'id_token token';
var scope = 'api1 openid';

function getProfile(cb) {

    getJson(identityUri + '/connect/userinfo', function (err, xhr) {

        cb(err, xhr);
    });
}

function initializeAuth() {
    clearAuthData();
    if (window.location.hash.indexOf('id_token') > -1) {
        authorizeCallback();
    } else {
        authorize();
    }
}

function urlBase64Decode(str) {
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

function getDataFromToken(token) {
    var data = {};
    if (typeof token !== 'undefined') {
        var encoded = token.split('.')[1];
        data = JSON.parse(urlBase64Decode(encoded));
    }
    return data;
}

function isAuthorized() {
    return localStorage.getItem('authToken') !== '';
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

function authorize() {
    console.log('AuthorizedController time to log on');

    var nonce = 'N' + Math.random() + '' + Date.now();
    var state = Date.now() + '' + Math.random();

    localStorage.setItem('authNonce', nonce);
    localStorage.setItem('authStateControl', state);

    console.log('AuthorizedController created. adding myautostate: ' +
        localStorage.getItem('authStateControl'));

    var uri =
        identityUri + '/connect/authorize?' +
        'response_type=' + encodeURIComponent(responseType) + '&' +
        'client_id=' + encodeURIComponent(clientId) + '&' +
        'redirect_uri=' + encodeURIComponent(redirectUri) + '&' +
        'scope=' + encodeURIComponent(scope) + '&' +
        'nonce=' + encodeURIComponent(nonce) + '&' +
        'state=' + encodeURIComponent(state);

    console.log(uri);

    window.location = uri;
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

function authorizeCallback() {
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

        if (result.state !== localStorage.getItem('authStateControl')) {
            console.log('AuthorizedCallback incorrect state');
        } else {

            token = result.access_token;
            id_token = result.id_token

            var dataIdToken = getDataFromToken(id_token);
            console.log(dataIdToken);

            // validate nonce
            if (dataIdToken.nonce !== localStorage.getItem('authNonce')) {
                console.log('AuthorizedCallback incorrect nonce');
            } else {
                localStorage.setItem('authNonce', '');
                localStorage.setItem('authStateControl', '');

                authResponseIsValid = true;
                console.log('AuthorizedCallback state and nonce validated, returning access token');
            }
        }
    }

    if (authResponseIsValid) {
        setAuthData(token, id_token);
        console.log(localStorage.getItem('authToken'));

    } else {
        clearAuthData();
    }
}