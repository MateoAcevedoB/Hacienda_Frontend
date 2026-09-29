#!/usr/bin/env bash

set -e

cd "$(dirname "$0")"

echo "== 1) Restore =="
dotnet restore Hacienda_TOBE.sln

echo "== 2) Build =="
dotnet build Hacienda_TOBE.sln --no-restore

echo "== 3) Test (104 tests previos + tests nuevos de FASE 4C-2) =="
dotnet test Bib_Hacienda.Tests/Bib_Hacienda.Tests.csproj --no-build

echo "== 4) Cobertura GLOBAL (sin exclusiones, comparable con el 41%/33% inicial) =="
dotnet test Bib_Hacienda.Tests/Bib_Hacienda.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/global

echo "== 5) Cobertura de LÓGICA DE NEGOCIO (excluye Bib_Hacienda.Infraestructura) =="
dotnet test Bib_Hacienda.Tests/Bib_Hacienda.Tests.csproj \
  --settings Bib_Hacienda.Tests/coverlet.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/negocio

echo "== 6) Instalar ReportGenerator (si no está instalado) =="
if ! command -v reportgenerator &> /dev/null; then
  dotnet tool install -g dotnet-reportgenerator-globaltool
  export PATH="$PATH:$HOME/.dotnet/tools"
fi

echo "== 7) Generar reporte HTML de lógica de negocio =="
COVERAGE_FILE=$(find ./TestResults/negocio -name "coverage.cobertura.xml" | head -1)
reportgenerator \
  -reports:"$COVERAGE_FILE" \
  -targetdir:"./CoverageReport_Negocio" \
  -reporttypes:Html

echo "== 8) Generar reporte HTML global (comparación) =="
COVERAGE_FILE_GLOBAL=$(find ./TestResults/global -name "coverage.cobertura.xml" | head -1)
reportgenerator \
  -reports:"$COVERAGE_FILE_GLOBAL" \
  -targetdir:"./CoverageReport_Global" \
  -reporttypes:Html

echo ""
echo "Listo. Abrir ./CoverageReport_Negocio/index.html para ver el % de"
echo "cobertura de lógica de negocio (debe ser >= 80% líneas)."
