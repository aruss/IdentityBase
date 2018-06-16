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
  <!--<a  target="_blank" href="https://coveralls.io/github/IdentityBaseNet/IdentityBase?branch=master">
      <img src="https://coveralls.io/repos/github/IdentityBaseNet/IdentityBase/badge.svg?branch=master" alt="Coverage Status">
  </a>-->
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

## Getting started

IdentityBase is available on [Docker Hub](https://hub.docker.com/r/identitybasenet/identitybase/) alongside with [Github Releases](https://github.com/IdentityBaseNet/IdentityBase/releases)

To get started with IdentityBase see the [getting started](/docs/getting-started.md) documentation.

## Development

### Platform

IdentityBase is built against ASP.NET Core 2.1 using the tooling that ships with Visual Studio 2017 on Windows 10. This is the only configuration we support on the issue tracker.

### Clone

```sh
git clone -q --branch=develop https://github.com/IdentityBaseNet/IdentityBase.git
cd IdentityBase
git submodule update --init
```

If you find after cloning the repository that some files are checked
out or marked for deletion make sure to run this command.

```sh
git config --global core.longpaths true
```

Then clone the repository again.

### Requirements

* [Visual Studio 2017](https://www.visualstudio.com/de/vs/community)
* [.NET Core 2.1](https://www.microsoft.com/net/download/core#/current)
* [Docker](https://www.docker.com/docker-windows)

### How to build

You can build all three version on windows.

#### Windows version

```cmd
build-windows.bat
```

It will publish the application in `./build/win7-x64/identitybase-latest` directory, switch in
that direcotry and run `run.bat` to start the IdentityServer on Windows.

#### Linux version

```cmd
build-linux.bat
```
It will publish the application in `./build/linux-x64/identitybase-latest` directory, switch in
that direcotry and run `chmod +x && ./run` to start the IdentityServer on Linux.

#### Docker image

```cmd
build-docker.bat
```

This command will create a Docker image `identitybasenet/identitybase` with `latest` tag.

## Acknowledgements

IdentityBase is built using the following great open source projects

* [ASP.NET Core](https://github.com/aspnet)
* [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
* [BrockAllen.MembershipReboot](https://github.com/brockallen/BrockAllen.MembershipReboot)
* [Json.Net](http://www.newtonsoft.com/json)
* [XUnit](https://xunit.github.io/)
* [Fluent Assertions](http://www.fluentassertions.com/)
* [Serilog](https://serilog.net/)