#!/usr/bin/env bash

set -euo pipefail

if ! command -v reportgenerator >/dev/null 2>&1; then
    echo "Installing ReportGenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
fi

rm -rf EPR.Calculator.Service.Function.UnitTests/TestResults
rm -rf EPR.Calculator.API.UnitTests/TestResults
rm -rf EPR.Calculator.API.IntegrationTests/TestResults
rm -rf coveragereport

dotnet test \
  --collect:"XPlat Code Coverage" \
  --settings coverage.runsettings

reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:"Html"

echo "Open coveragereport/index.html"
