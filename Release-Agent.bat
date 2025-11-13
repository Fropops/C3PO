@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { . .\Release-C3PO.ps1; Release-C3PO -Target Agent }"
PAUSE
