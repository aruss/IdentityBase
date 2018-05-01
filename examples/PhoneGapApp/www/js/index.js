// Cordova app -----------------------------------------------------------------
// -----------------------------------------------------------------------------

var userManager = new Oidc.UserManager({
    authority: 'http://localhost:5000',
    client_id: 'phonegapclient',
    redirect_uri: 'http://localhost:3000',
    post_logout_redirect_uri: 'http://localhost:3000',
    response_type: "id_token token",
    scope: "openid profile api1",
    filterProtocolClaims: true,
    loadUserInfo: true,
    automaticSilentRenew: true
});

Oidc.Log.logger = console;

userManager.events.addAccessTokenExpiring(function () {

    console.log("token expiring...");
});

userManager.events.addUserLoaded(function (user) {

    var json = JSON.stringify(user, null, 4);
    console.log('user loaded', json);
    $('#user-result').html(json);
    $('#public').hide();
    $('#private').show();
});

userManager.events.addSilentRenewError(function (error) {

    console.error('error while renewing the access token', error);
});

userManager.events.addUserSignedOut(function () {

    alert('The user has signed out');

    $('#private').hide();
    $('#public').show();
});

userManager.signinRedirectCallback();

function initialize() {

    document.addEventListener('deviceready', this.onDeviceReady, false);
};

function onDeviceReady() {

    $('#loading').hide();
    $('#deviceready').show();

    userManager.getUser().then(function (user) {

        if (user) {

            var json = JSON.stringify(user, null, 4);
            console.log('user loaded', json);
            $('#user-result').html(json);
            $('#public').hide();
            $('#private').show();
        }
    });

    $('#signout').click(function (e) {

        e.preventDefault();
        userManager.signoutRedirect();
    });

    $('#signout').click(function (e) {

        e.preventDefault();
        userManager.signoutRedirect();
    });

    $('#getapidata').click(function (e) {

        e.preventDefault();
        getJson('http://localhost:3721/identity').then(function(response) {

            var json = JSON.stringify(response.result, null, 4);
            console.log('api data', json);
            $('#getapidata-result').html(json);
        });
    });

    $('#signin').click(function (e) {

        e.preventDefault();
        userManager.signinRedirect();
    });
};



function getJson(uri) {

    return new Promise(function (resolve, reject) {

        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {

            if (xhr.readyState === 4) {

                if (xhr.status === 401) {

                    reject(xhr);
                } else if (xhr.status >= 500 && xhr.status < 600) {

                    reject(xhr);
                } else {
                    try {

                        xhr.result = JSON.parse(xhr.responseText);
                        resolve(xhr);
                    } catch (e) {

                        reject(e);
                    }
                }
            }
        };

        xhr.open('GET', uri, true);
        xhr.uri = uri;
        xhr.timeout = 120000;
        xhr.withCredentials = false;

        xhr.setRequestHeader('Accept', 'application/json');
        xhr.setRequestHeader('Content-Type', 'application/json');

        userManager.getUser().then(function (user) {

            xhr.setRequestHeader('Authorization', 'Bearer ' + user.access_token);
            xhr.send(null);
        });
    });
}


/*


    var elm = document.getElementById('deviceready');

    elm.querySelector('.listening').setAttribute('style', 'display:none;');
    elm.querySelector('.received').setAttribute('style', 'display:block;');

    if (user) {

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

            userManager.signoutRedirect()
                .catch(function (error) {
                    console.error('error while signing out user', error);
                });

            return false;
        }, false);
    } else {

        document.getElementById('signin').addEventListener('click', function () {

            userManager.signinRedirect();
            mgr.signinPopup()
                .catch(function (error) {

                    console.error('error while logging in through the popup', error);
                });

                return false;
            }, false);
*/