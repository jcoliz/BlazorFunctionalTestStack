Push-Location ..\Blazor
dotnet build
Start-Job -Name uitestsbg -ScriptBlock { dotnet run } -WorkingDirectory $(Get-Location)
Pop-Location
