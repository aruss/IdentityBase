# IdentityBase infrastructure

This folder contains all the compose files for development use.
You can start all requred services like mssql, mysql, postgres, rabbitmq by running following command.
No need to install it all on your dev machine.

    docker-compose -f postgres.yml up

    docker-compose -f rabbitmq.yml up
