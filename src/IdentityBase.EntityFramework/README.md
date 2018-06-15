# IdentityBase.EntityFramework

Shared library for all EF based plugins.

### Create migration files

If adding new EF based plugin use following command to add migration files.

```sh
dotnet ef migrations add init --context MigrationDbContext
```