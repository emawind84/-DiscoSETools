@echo off
title Disable SE Task Scheduler

if x%1x == xx goto serviceNameEmpty
set SERVICE_NAME=%1
echo %SERVICE_NAME%
goto changeTask

:serviceNameEmpty
echo Service name empty
goto endScript

:changeTask
Schtasks.exe /Change /DISABLE /TN %SERVICE_NAME%

rem Pause so they can see what just happened

:endScript