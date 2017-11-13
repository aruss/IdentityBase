<h1 align="center">
  <br>
  <img src="https://github.com/IdentityBaseNet/IdentityBase/raw/develop/docs/icon.png" alt="IdentityBase" width="200">
  <br>
  IdentityBase
  <br>
</h1>

<h4 align="center">IdentityBase is a <a  target="_blank"  href="https://en.wikipedia.org/wiki/Security_token_service">Secure Token Service (STS)</a> built on top of <a href="https://github.com/IdentityServer/IdentityServer4" target="_blank">IdentityServer4</a>.</h4>

<p align="center">
  <a target="_blank" href="https://ci.appveyor.com/project/aruss81994/identitybase">
    <img src="https://ci.appveyor.com/api/projects/status/fub9f3dhuctubpxr?svg=true" alt="Build status">
  </a>
  <a  target="_blank" href="https://coveralls.io/github/IdentityBaseNet/IdentityBase?branch=master">
      <img src="https://coveralls.io/repos/github/IdentityBaseNet/IdentityBase/badge.svg?branch=master" alt="Coverage Status">
  </a>
</p>

<p align="center">
IdentityBase is sponsored by <a  target="_blank" href="http://netzkern.de">netzkern AG</a>  
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
* **Database Support for**
   - Microsoft SQL Server, LocalDB
   - PostgreSQL
   - MySQL
   - MariaDB
   - InMemory

#### Upcoming features
* Authentication API
* Multitenancy
* Administration dashboard (WIP: [UiBase](https://github.com/aruss/UiBase))
* Localization
* Configuration wizzard
* Two-Factor authentication
* SMS OTP authentication
* Event Messaging Support for Apache Kafka, RabbitMQ
* Plugin support

### Platform

IdentityBase is built against ASP.NET Core 2.0 using the tooling that ships with Visual Studio 2017. This is the only configuration we support on the issue tracker.

### How to build

* [Install](https://www.microsoft.com/net/download/core#/current) .NET Core 2.0
* Use Visual Studio 2017 to build it

### Docker support

You can either build it from source code by running the `build.sh` script in `./docker` directory or just start it from [Docker Hub](https://hub.docker.com/r/identitybasenet/identitybase/)
 
  `docker run -it --rm -p 5000:5000 identitybasenet/identitybase`

### Acknowledgements

IdentityBase is built using the following great open source projects

* [ASP.NET Core](https://github.com/aspnet)
* [Json.Net](http://www.newtonsoft.com/json)
* [XUnit](https://xunit.github.io/)
* [Autofac](https://autofac.org/)
* [Fluent Assertions](http://www.fluentassertions.com/)
* [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
* [BrockAllen.MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot)
