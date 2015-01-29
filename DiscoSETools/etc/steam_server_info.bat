@echo off
title Steam Server Info Script

rem echo Current directory: %cd%

set "CURRENT_DIR=%cd%"

if "x%2x" == "xx" goto setOnlyFirstArg
set "CMD_ARGS=%2"
goto setFirstArg

:setOnlyFirstArg
if "x%1x" == "xx" goto endScript
set "CMD_ARGS=%1"
goto runScript

:setFirstArg
if "x%1x" == "xx" goto endScript
set "SCRIPT_DIR=%1"

:runScript

%SCRIPT_DIR%\disco-mini-server\bin\disco-mini-server.bat "%CMD_ARGS%"

:endScript

echo Script terminated
cd "%CURRENT_DIR%"