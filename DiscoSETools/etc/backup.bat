@echo off

set "CURRENT_DIR=%cd%"
set "BACKUP_SCRIPT_DIR=%CURRENT_DIR%"

if "x%1x" == "xx" goto step1
set BACKUP_SCRIPT_DIR=%1
cd "%BACKUP_SCRIPT_DIR%"

:step1

%BACKUP_SCRIPT_DIR%\creative_backup.ffs_batch
%BACKUP_SCRIPT_DIR%\survival_backup.ffs_batch

cd "%CURRENT_DIR%"

echo Backup created!