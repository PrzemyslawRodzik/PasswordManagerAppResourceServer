
## General info
The PasswordManagerAppServer project contains the APIs, database, and other core infrastructure items needed for the "backend" of [web](https://github.com/PrzemyslawRodzik/PasswordManagerApp) and [mobile](https://github.com/PrzemyslawRodzik/XamarinPasswordManagerApp) application. The server act as authentication and resource server. After successful authentication for certain time [JSON Web Token](https://jwt.io/) is generated for specific user to give him access to resources(their stored data).

The server project is written in C# using .NET Core 3.1 with ASP.NET Core. The application can be developed, built and run cross-platform on Windows, and Linux distribution(*macOS not tested*).

---

## Table of contents
* [General info](#general-info)
* [Configuration](#configuration)
* [Setup](#setup)
    * [Requirements](#requirements)
    * [Build](#to-run-this-project)
* [Usage](#usage)

---

## Configuration
 You can customize MySQL database and email credentials in *appsettings.json* file. </br>
 Moreover you are able to change database to sqlite. To do so you'll need to comment line with mysql, then uncomment sqlite line.
<img src="https://i.ibb.co/6mMgdzY/image.png" alt="mobileVault" title="mobile vault" width="548"/>
 
 ---
 
## Setup
### Requirements

- [.NET Core 3.1 SDK](https://www.microsoft.com/net/download/core)
- [MySQL](https://github.com/mysql/mysql-server)

#### To run this project:
* run [mysql server](https://github.com/mysql/mysql-server) on port 3306 logged as root
* restore, build and run using commands:

```
cd PasswordManagerAppServer
dotnet run
```

---

## Usage

#### Next move is up to you

* Configure [PasswordManagerApp](https://github.com/PrzemyslawRodzik/PasswordManagerApp) to store your data in web vault

![web vault](https://i.ibb.co/Gsx518L/image.png)
* Configure [XamarinPasswordManagerApp](https://github.com/PrzemyslawRodzik/XamarinPasswordManagerApp) to save your passwords in mobile application





