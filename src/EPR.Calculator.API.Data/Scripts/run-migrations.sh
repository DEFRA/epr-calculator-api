#!/bin/bash

/opt/mssql-tools/bin/sqlcmd -S $SERVER,$PORT -U $USER -P $PASSWORD -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'$DATABASE') CREATE DATABASE [$DATABASE]" -I

if [[ -s $1 ]]; then
  /opt/mssql-tools/bin/sqlcmd -S $SERVER,$PORT -U $USER -P $PASSWORD -d $DATABASE -i "$1" -I
else
  echo The file "$1" is empty. No update has been triggered.
fi

if [[ -s $2 ]]; then
  /opt/mssql-tools/bin/sqlcmd -S $SERVER,$PORT -U $USER -P $PASSWORD -d $DATABASE -i "$2" -I
fi
