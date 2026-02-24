$ErrorActionPreference = "Stop"
$target = $env:CARGO_DIST_TARGET
switch ($target) {
  "x86_64-unknown-linux-gnu" {
    $rid = "linux-x64"
    $exe = "Dataset2Sql"
  }
  "x86_64-pc-windows-msvc" {
    $rid = "win-x64"
    $exe = "Dataset2Sql.exe"
  }
  default {
    throw "Unsupported CARGO_DIST_TARGET: $target"
  }
}
$outDir = "target/dist-build/$target"
dotnet publish src/Dataset2Sql/Dataset2Sql.csproj `
  --configuration Release `
  --runtime $rid `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:PublishReadyToRun=true `
  /p:EnableCompressionInSingleFile=true `
  /p:DebugType=embedded `
  /p:DebugSymbols=true `
  -o $outDir
Copy-Item "$outDir/$exe" "./$exe" -Force