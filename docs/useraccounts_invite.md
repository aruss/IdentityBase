# UserAccount Invitations

IdentityBase supports a user account invitation mechanism via HTTP API.

Setting `EnableUserInviteEndpoint` option to `true` enables following HTTP endpoints.

## Listing all invitations

You can get all created invitations that are not yet confrimed or canceled.

``skip``
    identifier of the client (optional).
``take``
    one or more registered scopes (optional)

**Example**


    GET /api/invitations?
        take=1&
        skip=0


    HTTP/1.1 200 OK
    Content-Type: application/json

    {
        "result": {
            "total": 3,
            "skip": 0,
            "take": 1,
            "sort": [
                {
                    "field": "createdAt",
                    "direction": "descending"
                }
            ],
            "items": [
                {
                    "id": "f542ed88-291a-43ed-a606-38326311642e",
                    "email": "jim@somehost.com",
                    "createdAt": "2017-06-14T10:34:11.5950478Z",
                    "createdBy": "49c89048-bcad-4b24-a738-ac43c72e58c1",
                    "verificationKeySentAt": "2017-06-14T10:34:11.5950478Z"
                }
            ]
        },
        "success": true
    }

## Creating a invitation

``email``
    email address of the user being invited (required)
``clientId``
    client_id of the application the user will be redirected after he confirms the invitation (required)
``returnUri``
    absolute return uri of the client (optional)

**Example**


    PUT /api/invitations
    Content-Type: application/json
    Authorization: Bearer <access_token>

    {
       "email": "jim@somehost.com",
       "clientId": "js_oidc",
       "returnUri: "http://localhost:8080"
    }


(URL encoding removed, and line breaks added for readability)

A successful response will return a status code of 200


    HTTP/1.1 200 OK
    Content-Type: application/json

    {
        "success": true
    }

An invalid request will return a 400


    {
        "success": false,
        "messages": [
            {
                "kind": "error",
                "message": "The ClientId field is required.",
                "field": "clientId"
            }
        ]
    }

## Removing invitations

``UserAccountId``
    Id of the user account (required)

**Example**


    DELETE /api/invitations/{UserAccountId}
    Authorization: Bearer <access_token>


A successful response will return a status code of 200


    HTTP/1.1 200 OK
    Content-Type: application/json

    {
        "success": true
    }
