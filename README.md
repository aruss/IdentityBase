# ServiceBase.IdentityServer

ServiceBase.IdentityServer is a [STS](https://en.wikipedia.org/wiki/Security_token_service) based on [IdentityServer 4](https://github.com/IdentityServer/IdentityServer4) and [MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot). It contains all the self service features for end customers to create and manage an user account.

[![Build status](https://ci.appveyor.com/api/projects/status/0kld9s4sm8b50930/branch/master?svg=true)](https://ci.appveyor.com/project/aruss81994/servicebase-identityserver/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/aruss/ServiceBase.IdentityServer/badge.svg?branch=master)](https://coveralls.io/github/aruss/ServiceBase.IdentityServer?branch=master)
[![Join the chat at https://gitter.im/ServiceBase/Lobby](https://badges.gitter.im/ServiceBase/Lobby.svg)](https://gitter.im/ServiceBase/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Features

* Single Sign On
* Authentication as a Service
* External Identity Providers for Social Logins
* Access Control for APIs
* Federation Gateway

### Requirements

* MSSQL or PostgreSQL
* tbd...

### Platform

IdentityBase is built against ASP.NET Core 1.1 using the RTM tooling that ships with Visual Studio 2017. This is the only configuration we support on the issue tracker.

### How to build

* [Install](https://www.microsoft.com/net/download/core#/current) .NET Core 1.1
* Run `sh ./build.sh`

### Acknowledgements

IdentityServer4 is built using the following great open source projects

* [ASP.NET Core](https://github.com/aspnet)
* [Json.Net](http://www.newtonsoft.com/json)
* [XUnit](https://xunit.github.io/)
* [Fluent Assertions](http://www.fluentassertions.com/)
* [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
* [BrockAllen.MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot)