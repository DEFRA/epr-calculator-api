#!/bin/bash
# Applies a SQL migrations script with sqlcmd.
#
# Required environment variables: SERVER, PORT, USER, PASSWORD and DATABASE.
# Optional: TRUST_SERVER_CERTIFICATE=true skips validation of the server's TLS certificate, for SQL Servers with
# self-signed certificates such as the local env's container. Azure SQL has a valid certificate, so leave it unset there.
set -euo pipefail

script="${1:?Usage: run-migrations.sh <script.sql>}"

for name in SERVER PORT USER PASSWORD DATABASE; do
  if [[ -z "${!name:-}" ]]; then
    echo "The $name environment variable is required." >&2
    exit 1
  fi
done

if [[ ! -s "$script" ]]; then
  echo "The file '$script' is empty. No update has been triggered."
  exit 0
fi

# sqlcmd reads the password from SQLCMDPASSWORD, which keeps it off the command line.
export SQLCMDPASSWORD="$PASSWORD"

options=(
  -S "$SERVER,$PORT"
  -U "$USER"
  -d "$DATABASE"
  -i "$script"
  -I # QUOTED_IDENTIFIER ON, as EF Core expects.
  # Stop at the first error, exiting with its severity. Informational messages (severity 10 or less, such as
  # sp_rename's caution) are only printed.
  -b -V 10
)

if [[ "${TRUST_SERVER_CERTIFICATE:-false}" == "true" ]]; then
  options+=(-C)
fi

echo "Applying $script to database '$DATABASE' on '$SERVER,$PORT'"
exec /opt/mssql-tools18/bin/sqlcmd "${options[@]}"
