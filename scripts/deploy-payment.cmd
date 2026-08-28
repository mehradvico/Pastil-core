@echo off
REM Deploy ONLY the Payment service. Double-click to run.
REM Extra flags are passed through, e.g.:  deploy-payment.cmd -SkipBuild
powershell -ExecutionPolicy Bypass -File "%~dp0Deploy-Backend.ps1" -Service payment %*
echo.
pause
