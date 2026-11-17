@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\CollectPlaytestFeedback.ps1" -Root "%~dp0."
if errorlevel 1 (
    echo Feedback collection failed. Please send BUILD_INFO.txt, playtest.log, prior playtest-*.log and screenshots manually.
) else (
    echo Review the feedback ZIP before sending it to the developer.
)
pause
