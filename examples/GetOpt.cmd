@echo off
set name=%~n0
set conf=Debug
set fw=net6.0
set exe=%~dp0%name%\bin\%conf%\%fw%\%name%.exe
%exe% %*
IF %ERRORLEVEL% EQU 9009 (
    dotnet build %~dp0%name%
    dotnet test %~dp0%name%
    %exe% %*
)
