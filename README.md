<h1 align="center">
  <br>
  <img src="https://github.com/IdentityBaseNet/IdentityBase/raw/develop/docs/icon.png" alt="IdentityBase" width="200">
  <br>
  IdentityBase
  <br>
</h1>

<h4 align="center">IdentityBase is a Identity and Access Control solution built on top of <a href="http://identityserver.io/" target="_blank">IdentityServer</a>. <br/>It provides Single Sign On & Token Based Authentication with many other features right out of the box.</h4>

<p align="center">
  <a target="_blank" href="https://ci.appveyor.com/project/aruss81994/identitybase">
    <img src="https://ci.appveyor.com/api/projects/status/fub9f3dhuctubpxr?svg=true" alt="Build status">
  </a>
  <a  target="_blank" href="https://coveralls.io/github/IdentityBaseNet/IdentityBase?branch=master">
      <img src="https://coveralls.io/repos/github/IdentityBaseNet/IdentityBase/badge.svg?branch=master" alt="Coverage Status">
  </a>
</p>

### Features

* **Single Sign-on / Sign-out**
  Single sign-on (and out) over multiple application types.
* **Authentication as a Service**
  Centralized login logic and workflow for all of your applications (web, native, mobile, services).
* **Access Control for APIs**
  Issue access tokens for APIs for various types of clients, e.g. server to server, web applications, SPAs and native/mobile apps.
* **Federation Gateway**
  Support for external identity providers like Azure Active Directory, Google, Facebook etc. This shields your applications from the details of how to connect to these external providers.
* **Theming**
  Support for custom themes, you can change the default [Bootstrap](http://getbootstrap.com/) styles or create completely new UI by writing your own Razor views.
* **Localization**
  Localization support for UI, E-Mail and SMS templates.
* **HTTP API**
  Manage user invitations and change users E-Mail and Password directly from your relying party app.
* **Plugin Support**
  Modular Architecture allows to add custom plugins and/or replace default parts of IdentityBase.
* **Database Support for**
   - Microsoft SQL Server, LocalDB
   - PostgreSQL
   - MySQL
   - MariaDB
   - InMemory (Recommended only for testing)

#### Upcoming features

For upcoming features see [Issues](https://github.com/IdentityBaseNet/IdentityBase/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement) with `enhancement` tag.

### Platform

IdentityBase is built against ASP.NET Core 2.0 using the tooling that ships with Visual Studio 2017. This is the only configuration we support on the issue tracker.

### How to build

* [Install](https://www.microsoft.com/net/download/core#/current) .NET Core 2.0
* Use Visual Studio 2017 to build it

### Docker support

You can either build it from source code by running the `build.sh` script in `./docker` directory or just start it from [Docker Hub](https://hub.docker.com/r/identitybasenet/identitybase/)

  `docker run -it --rm -p 5000:5000 identitybasenet/identitybase`

It will start a IdentityBase with in memory store, with default client configuration and dummy users `alice@localhost` and `bob@localhost` (password is the email).

See `./samples` folder and/or [IdentityServer examples repository](https://github.com/IdentityServer/IdentityServer4.Samples) for client samples.

### Acknowledgements

IdentityBase is built using the following great open source projects

* [ASP.NET Core](https://github.com/aspnet)
* [Json.Net](http://www.newtonsoft.com/json)
* [XUnit](https://xunit.github.io/)
* [Fluent Assertions](http://www.fluentassertions.com/)
* [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
* [BrockAllen.MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot)


<hr/>
<p align="center">
IdentityBase is sponsored by <a  target="_blank" href="http://netzkern.de">netzkern AG</a>
</p>