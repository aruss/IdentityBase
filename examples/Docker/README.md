# IdentityBase docker example

This example shows a simple setup where identity provider hides behind a reverse
[nginx](https://www.nginx.com/) as a reverse proxy and a
[ASP.NET MVC](https://dotnet.microsoft.com/apps/aspnet/mvc) website
authenticates by using a hybrid flow to authenticate. The MVC projects also
makes calls to a web service using either the user token or by using client
credentials.

To run this example you require [docker](https://www.docker.com/) installed on
your host machine. And dont forget to initialize the git submodules like descriped
in the main readme file.

Before you start it add following hostnames to your host file.

```host
127.0.0.1 identitybase.local
127.0.0.1 auth.identitybase.local
127.0.0.1 api1.identitybase.local
```

and run `docker-compose up` to build and run the example project.

If running in powershell you have to set the following environment var

    COMPOSE_CONVERT_WINDOWS_PATHS=1

After all the images are built and the container are started navigate to
http://identitybase.local

The test user credentials are `alice@localhost` and `bob@localhost`
the password is quals to user name.