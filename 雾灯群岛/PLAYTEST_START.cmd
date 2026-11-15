@echo off
setlocal
cd /d "%~dp0"
start "" "%~dp0FoglightIslesDemo.exe" -interactive-review -logFile "%~dp0playtest.log"
