## Database migrations image

CI builds this folder into the `<branch>-<build>-database-migrations` image, which applies `migrations.sql` (the idempotent EF Core migrations script - see the repository README for how to regenerate it) with sqlcmd. Only the files in this folder are available to the Docker build.

The deployment pipeline runs the image with these environment variables:

- `SERVER`, `PORT`, `USER`, `PASSWORD`, `DATABASE` (required) - the SQL Server, and the existing database to migrate.
- `TRUST_SERVER_CERTIFICATE=true` (optional) - skips validation of the server's TLS certificate. Only use it for SQL Servers with self-signed certificates, such as the local environment's; Azure SQL has a valid certificate.

The container stops at the first error and exits non-zero.

### Testing locally

With the local environment running, build the image and run it on the environment's Docker network against its `sql` container:

```shell
docker build -t database-migrations .
docker run --rm --network epr_net \
   -e SERVER=sql -e PORT=1433 -e USER=sa -e PASSWORD='<SA password>' -e DATABASE=paycal \
   -e TRUST_SERVER_CERTIFICATE=true \
   database-migrations
```
