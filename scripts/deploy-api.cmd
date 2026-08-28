@echo off
REM Deploy ONLY the Api service. Double-click to run.
REM Extra flags are passed through, e.g.:  deploy-api.cmd -SkipBuild
powershell -ExecutionPolicy Bypass -File "%~dp0Deploy-Backend.ps1" -Service api %*
echo.
pause
