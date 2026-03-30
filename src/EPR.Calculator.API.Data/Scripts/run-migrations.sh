#!/bin/bash

if [[ -s $1 ]]; then
  /opt/mssql-tools/bin/sqlcmd -S $SERVER,$PORT -U $USER -P $PASSWORD -d $DATABASE -i "$1" -I -b -V 10 # Stop execution with issues severity level 10+ - including informational warnings
else
  echo The file "$1" is empty. No update has been triggered.
fi
