# Applies the idempotent EF Core migrations script (migrations.sql) using sqlcmd.
#
# The CI pipeline (epr-webapps-code-deploy-templates) builds this image with this folder as the build context, so only
# the files alongside this Dockerfile are available - hence running the generated script rather than an EF Core bundle.
FROM ubuntu:24.04

# Install sqlcmd (mssql-tools18) and ODBC Driver 18 from Microsoft's package repository. Don't go back to the old
# mcr.microsoft.com/mssql-tools image: it's unmaintained, and its unixODBC 2.3.1 fails with 'SqlState 24000, Invalid
# cursor state' after a '(0 rows affected)' result, silently skipping the rest of the script.
# - ca-certificates stays installed so that the driver can validate the server's TLS certificate.
# - The driver needs libgssapi-krb5-2 but doesn't declare it, so install it explicitly (otherwise removing curl removes it).
# - Finally, fail the build if the driver has any unresolved shared libraries, rather than when the migrations run.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -fsSL -o /tmp/packages-microsoft-prod.deb https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends mssql-tools18 libgssapi-krb5-2 \
    && apt-get purge -y --auto-remove curl \
    && rm -rf /var/lib/apt/lists/* \
    && ! ldd /opt/microsoft/msodbcsql18/lib64/libmsodbcsql-*.so* | grep 'not found'

WORKDIR /migrations
COPY migrations.sql run-migrations.sh ./

# The base image's built-in unprivileged user.
USER ubuntu

CMD ["/bin/bash", "/migrations/run-migrations.sh", "/migrations/migrations.sql"]
