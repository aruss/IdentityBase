# IdentityBase.EntityFramework.SqlServer

Microsoft SQL Server data provider for IdentityBase.

### Requirements 

- SQL Server Express
- Database named `IdentityBase` with login (username: `dev`, password `dev`) as `db_owner`

### Used libraries

- EntityFramework https://docs.microsoft.com/en-us/ef/
- Microsoft SQL Server data provider https://docs.microsoft.com/en-us/ef/core/providers/sql-server/

### Create migration files

Change the project type to "Console Application" and compile it with DEBUG flag
then run following command. After that change it back to "Class Library".

```sh
dotnet ef migrations add init --context MigrationDbContext
```

### Start postgres server by running following docker compose file, https://hub.docker.com/r/microsoft/mssql-server-linux/

```yaml
version: "2.0"
services:
  mssql:
    container_name: idbase-dev-mssql
    restart: unless-stopped
    image: microsoft/mssql-server-linux:2017-latest
    ports:
      - 1433:1433
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=fancyStrong(!)Password
```