#!/usr/bin/env bash
set -euo pipefail

case "${CARGO_DIST_TARGET:-}" in
  x86_64-unknown-linux-gnu)
    rid="linux-x64"
    exe="Dataset2Sql"
    ;;
  x86_64-pc-windows-msvc)
    rid="win-x64"
    exe="Dataset2Sql.exe"
    ;;
  *)
    echo "Unsupported CARGO_DIST_TARGET: ${CARGO_DIST_TARGET:-unset}" >&2
    exit 1
    ;;
esac

out_dir="target/dist-build/${CARGO_DIST_TARGET}"

dotnet publish src/Dataset2Sql/Dataset2Sql.csproj \
  --configuration Release \
  --runtime "${rid}" \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  /p:PublishReadyToRun=true \
  /p:EnableCompressionInSingleFile=true \
  /p:DebugType=embedded \
  /p:DebugSymbols=true \
  -o "${out_dir}"

cp "${out_dir}/${exe}" "./${exe}"
