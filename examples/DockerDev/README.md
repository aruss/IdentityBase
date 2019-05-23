# Handy container setup for development environment

you can run this one in order to access local instance of identitybase via
identitybase.local host in order to write end to end tests.

Before you start it add following hostnames to your host file.

```host
127.0.0.1 identitybase.local
127.0.0.1 auth.identitybase.local
127.0.0.1 api1.identitybase.local
```

and run `docker-compose -f nginx.yml up` to run the reverse proxy. (does not work on windows)

You can now access your local running instances via http://identitybase.local

Make sure you host machine does not run anything on port 80