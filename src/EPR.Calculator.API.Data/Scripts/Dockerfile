FROM mcr.microsoft.com/mssql-tools

# Install dos2unix to fix Windows line endings
USER root
RUN apt-get update && apt-get install -y dos2unix && rm -rf /var/lib/apt/lists/*

# Create a non-root user and define a home directory
RUN useradd -m -d /home/nonroot -s /bin/bash nonroot

# Create the directory for non-root user and set it as the working directory
RUN mkdir -p /home/nonroot/sql
WORKDIR /home/nonroot/sql

# Copy migration files and script, setting ownership to non-root
COPY --chown=nonroot migrations.sql .
COPY --chown=nonroot seed.sql .
COPY --chown=nonroot run-migrations.sh .

# Convert Windows line endings to Unix and make script executable
RUN dos2unix run-migrations.sh migrations.sql seed.sql \
    && chmod +x run-migrations.sh

# Switch to non-root user
USER nonroot

# Run migration script as default command
CMD ["/bin/bash", "/home/nonroot/sql/run-migrations.sh", "/home/nonroot/sql/migrations.sql", "/home/nonroot/sql/seed.sql"]