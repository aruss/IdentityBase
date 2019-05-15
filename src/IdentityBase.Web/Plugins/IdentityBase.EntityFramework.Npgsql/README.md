# IdentityBase.EntityFramework.Npgsql

PostgreSQL data provider for IdentityBase.

### Requirements 

- PostgreSQL

### Used libraries

- EntityFramework https://docs.microsoft.com/en-us/ef/
- Npgsql PostgreSQL data provider https://docs.microsoft.com/en-us/ef/core/providers/npgsql/

### Create migration files

Change the project type to "Console Application" and compile it with DEBUG flag
then run following command. After that change it back to "Class Library".

```sh
dotnet ef migrations add init --context MigrationDbContext
```

### Start postgres server by running following docker compose file, https://hub.docker.com/_/postgres/

```yaml
version: "2.0"
services:
  postgres:
    container_name: idbase-dev-postgres
    image: postgres:9.6.9-alpine
    restart: unless-stopped
    ports:
      - 5432:5432
      - 5433:5433
    environment:
      - POSTGRES_DB=identitybase
      - POSTGRES_PASSWORD=dev
      - POSTGRES_USER=dev
```