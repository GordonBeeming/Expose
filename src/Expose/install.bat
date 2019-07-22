@ECHO OFF

cd /d %~dp0
powershell.exe -ExecutionPolicy Unrestricted .\%~n0.ps1

echo done
pause