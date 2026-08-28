@echo off
REM Deploy ONLY the File service. Double-click to run.
REM Extra flags are passed through, e.g.:  deploy-file.cmd -SkipBuild
powershell -ExecutionPolicy Bypass -File "%~dp0Deploy-Backend.ps1" -Service file %*
echo.
pause
