@echo off
rem Builds CircleToSearch.exe with the C# compiler that ships with Windows (.NET Framework 4.x).
rem No Visual Studio or SDK needed.  Usage: build.bat [--no-pause]
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo csc.exe ^(.NET Framework 4^) was not found on this PC.
  goto :fail
)

"%CSC%" /nologo /codepage:65001 /target:winexe /optimize+ /win32icon:src\app.ico /out:CircleToSearch.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll src\*.cs
if errorlevel 1 goto :fail

echo.
echo Built CircleToSearch.exe
if /i not "%~1"=="--no-pause" pause
exit /b 0

:fail
echo.
echo Build failed.
if /i not "%~1"=="--no-pause" pause
exit /b 1
