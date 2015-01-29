@echo off
title Daily SE Backup Archive

echo Current directory: %cd%

set t=%time:~0,2%%time:~3,2%%time:~6,2%
set d=%date:~6,4%-%date:~0,2%-%date:~3,2%

if "x%1x" == "xx" goto step1
set d=%1

:step1

set "COMMAND=7z.exe"
set "CURRENT_DIR=%cd%"
set "BACKUP_DIR=%CURRENT_DIR%"
set "ARCHIVE_DIR=%CURRENT_DIR%"
set "BACKUP_PREFIX_NAME=%2"

if "x%3x" == "xx" goto step2
set BACKUP_DIR=%3
cd "%BACKUP_DIR%"

:step2

if "x%4x" == "xx" goto step3
set ARCHIVE_DIR=%4

:step3

echo Backup directory: %BACKUP_DIR%
echo Archive save directory: %ARCHIVE_DIR%
echo Backup prefix name: %BACKUP_PREFIX_NAME%
echo Archive date: %d%

if exist %BACKUP_PREFIX_NAME%_%d%* goto step4
echo Backup files not found
goto endScript

:step4

if not exist "%ARCHIVE_DIR%\%BACKUP_PREFIX_NAME%_%d%.7z" goto step5
echo Archive file already exist
goto endScript

:step5

%COMMAND% a "%ARCHIVE_DIR%\%BACKUP_PREFIX_NAME%_%d%.7z" "%BACKUP_PREFIX_NAME%_%d%*"

:endScript

echo Script terminated
cd "%CURRENT_DIR%"