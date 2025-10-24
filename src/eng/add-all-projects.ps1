param(
    [string]$Solution = "Quasar.sln"
)

$projects = Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { $_.FullName }
dotnet sln $Solution add $projects

