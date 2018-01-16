echo off
if "%1"=="" goto :NoInput

if exist OEMDATA.EXE oemdata /SETDATA=essent.bin /TYPE=LOGO /ID=%1

goto :Exit

:NoInput
echo Please Input Right ProjectID.

:Exit echo on