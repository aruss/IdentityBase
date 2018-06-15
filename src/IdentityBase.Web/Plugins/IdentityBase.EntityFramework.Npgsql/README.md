# IdentityBase.EntityFramework.Npgsql

PostgreSQL data provider for IdentityBase.

### Requirements 

- PostgreSQL

### Used libraries

- EntityFramework https://docs.microsoft.com/en-us/ef/
- Npgsql PostgreSQL data provider https://docs.microsoft.com/en-us/ef/core/providers/npgsql/

### Create migration files

```sh
dotnet ef migrations add init --context MigrationDbContext
```

### Start postgres server by running following docker compose file 

```yaml
version: "2.0"

services:
  postgres-svc:  
    image: postgres:9.6.3-alpine
    restart: always
    ports:
      - 5432:5432
      - 5433:5433
    environment:
      - POSTGRES_PASSWORD=root
```