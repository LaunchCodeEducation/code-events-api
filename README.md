> ~/.docker/envs/coding_events.env

``
MYSQL_DATABASE=coding_events
MYSQL_USER=coding_events
MYSQL_PASSWORD=Learn2code!
MYSQL_ROOT_PASSWORD=password
``

``
docker run -d -p 3306:3306 --env-file ~/.docker/envs/coding-events.env --name coding-events-db mysql
``

``
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=coding_events;User=coding_events;Password=Learn2code\!"
``

``
dotnet ef database update
``

> create migration, run from project root

``
dotnet ef migrations add <ENTITY_NAME>Table -o Data/Migrations/
``

``
dotnet dev-certs https --trust
``