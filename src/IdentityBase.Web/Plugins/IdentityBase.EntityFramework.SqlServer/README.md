# IdentityBase.EntityFramework.SqlServer

Microsoft SQL Server data provider for IdentityBase.

### Requirements 

- SQL Server Express
    - Database named `IdentityBase` with login (username: `dev`, password `dev`) as `db_owner`

### Used libraries

- EntityFramework 7 https://docs.microsoft.com/en-us/ef/
- Microsoft SQL Server data provider https://docs.microsoft.com/en-us/ef/core/providers/sql-server/

### Create migration files, for development only 

```sh
dotnet ef migrations add init --context MigrationDbContext
```

