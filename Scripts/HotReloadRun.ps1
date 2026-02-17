# Starts dotnet watch with hot reload for development, plus Sass watchers
$ErrorActionPreference = 'Stop'
Set-Location "$PSScriptRoot/.."
$env:ASPNETCORE_ENVIRONMENT = 'Development'

# Start Sass watchers as background jobs
$sassJob1 = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    npx sass --watch Comjustinspicer.CMS/Views/Shared/Components/ContentZone/edit.scss:Comjustinspicer.CMS/wwwroot/css/content-zone-edit.css --no-source-map
}
$sassJob2 = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    npx sass --watch Comjustinspicer.Web/Views/Shared/site.scss:Comjustinspicer.Web/wwwroot/css/site.css --no-source-map
}

try {
    # Use dotnet watch to run the project with hot reload for both C# and Razor files
    dotnet watch run --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
}
finally {
    # Clean up Sass watchers on exit
    $sassJob1, $sassJob2 | Stop-Job -PassThru | Remove-Job
}
