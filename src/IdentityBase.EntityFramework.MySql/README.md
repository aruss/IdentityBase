# IdentityBase.EntityFramework.MySql

MySQL data provider for IdentityBase.

### Requirements 

- MySQL
- MariaDB

### Used libraries

- EntityFramework 7 https://docs.microsoft.com/en-us/ef/
- Pomelo MySQL data provider https://docs.microsoft.com/en-us/ef/core/providers/pomelo/

### Create migration files

```sh
dotnet ef migrations add init --context MigrationDbContext
```

### Start mysql server by running following docker compose file 

```yaml
version: "2.0"

services:
  mysql-svc:
    container_name: idbase-dev-mysql
    image: mysql:5.7
    restart: always
    environment:
      - MYSQL_ROOT_PASSWORD: root
      - MYSQL_DATABASE: identitybase
      - MYSQL_USER: dev
      - MYSQL_PASSWORD: dev
```
