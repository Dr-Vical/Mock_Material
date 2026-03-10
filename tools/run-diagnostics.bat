@echo off
title RswareDesign Diagnostics
echo.
echo  Starting RswareDesign Diagnostics...
echo.
powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0diagnostics.ps1" -AppDir "%~dp0.."
