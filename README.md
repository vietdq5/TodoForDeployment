this project apply clean architecture, feature slice architecture and event driven design.
to run this project.
Please run docker in local and ensure that rabbitmq is ready by this command:
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management


docker run --name postgres-container -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=mydatabase -p 5432:5432 -d postgres
postgres://postgres:postgres@localhost:5432/mydatabase