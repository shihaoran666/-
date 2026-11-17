@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\RotatePlaytestLog.ps1" -Root "%~dp0."
if errorlevel 1 (
    echo Could not preserve the previous playtest log. Close the game and try again.
    pause
    exit /b 1
)
start "" "%~dp0FoglightIslesDemo.exe" -interactive-review -logFile "%~dp0playtest.log"
