# ServiceBase.IdentityServer

ServiceBase.IdentityServer is a [Secure Token Service (STS)](https://en.wikipedia.org/wiki/Security_token_service) based on [IdentityServer4](https://github.com/IdentityServer/IdentityServer4) and [MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot). It contains all the self service features for end customers to create and manage an user account.

[![Build status](https://ci.appveyor.com/api/projects/status/0kld9s4sm8b50930/branch/master?svg=true)](https://ci.appveyor.com/project/aruss81994/servicebase-identityserver/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/aruss/ServiceBase.IdentityServer/badge.svg?branch=master)](https://coveralls.io/github/aruss/ServiceBase.IdentityServer?branch=master)
[![Join the chat at https://gitter.im/ServiceBase/Lobby](https://badges.gitter.im/ServiceBase/Lobby.svg)](https://gitter.im/ServiceBase/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Features

* #### Single Sign-on / Sign-out
  Single sign-on (and out) over multiple application types.
* #### Authentication as a Service
  Centralized login logic and workflow for all of your applications (web, native, mobile, services).
* #### Access Control for APIs
  Issue access tokens for APIs for various types of clients, e.g. server to server, web applications, SPAs and native/mobile apps.
* #### Federation Gateway
  Support for external identity providers like Azure Active Directory, Google, Facebook etc. This shields your applications from the details of how to connect to these external providers.
* #### Theming
  Support for custom themes, you can change the default [Bootstrap](http://getbootstrap.com/) styles or create completely new UI by writing your own Razor views.
* #### Event Messaging


#### Upcoming features
* Authentication API
* Multitenancy
* Administration dashboard
* Localization
* Configuration wizzard
* Two-Factor authentication
* SMS OTP authentication
* MySQL support
* Docker support
* Event Messaging Support for pache Kafka, RabbitMQ
* Switch DI Container to Autofac
* Plugin support

### Requirements

* MSSQL or PostgreSQL (Can also run in memory only)
* SMTP or SendGrid
* HTTPS

### Platform

IdentityBase is built against ASP.NET Core 1.1 using the RTM tooling that ships with Visual Studio 2017. This is the only configuration we support on the issue tracker.

### How to build

* [Install](https://www.microsoft.com/net/download/core#/current) .NET Core 1.1
* Run `sh ./build.sh`

### Docker support

After you run the `build.sh` script navigate to `./docker` directory and run `sh ./build-identitybase-public.sh` then you can start the container by running `docker run -it --rm -p 5000:5000 servicebase/identitybase` it will start the IdentiServiceBase.IdentityServer on port `http://localhost:5000`

### Acknowledgements

ServiceBase.IdentityServer is built using the following great open source projects

* [ASP.NET Core](https://github.com/aspnet)
* [Json.Net](http://www.newtonsoft.com/json)
* [XUnit](https://xunit.github.io/)
* [Fluent Assertions](http://www.fluentassertions.com/)
* [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
* [BrockAllen.MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot)
