@ECHO OFF

@REM %~dp0run.ps1' default-build %*
@REM ~\.dotnet\x64

PowerShell -NoProfile -NoLogo -ExecutionPolicy unrestricted -Command "[System.Threading.Thread]::CurrentThread.CurrentCulture = ''; [System.Threading.Thread]::CurrentThread.CurrentUICulture = '';& '%~dp0run.ps1' default-build %*; exit $LASTEXITCODE"
