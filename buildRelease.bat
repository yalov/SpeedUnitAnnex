@echo off
echo BUILDRELEASE.BAT

rem v4
rem Put the following text into the Post-build event command line:
rem without the "REM":

REM cd "$(SolutionDir)"
REM 
REM call  "$(SolutionDir)\buildDeploy.bat" "$(TargetDir)" "$(TargetFileName)"
REM 
REM if $(ConfigurationName) == Release (
REM call "$(SolutionDir)\buildRelease.bat" "$(TargetDir)" "$(TargetFileName)"
REM )

rem Set variables here

set MODNAME=SpeedUnitAnnex
set LICENSE=%MODNAME%-License.txt
set CHANGELOG=ChangeLog.md
set VERSIONFILE=%MODNAME%.version
set RELEASESDIR=Releases
set ZIP="c:\Program Files\7-zip\7z.exe"

if "%LICENSE%"   NEQ "" xcopy "%LICENSE%"   "GameData\%MODNAME%" /Y /I /Q
if "%CHANGELOG%" NEQ "" xcopy "%CHANGELOG%" "GameData\%MODNAME%" /Y /I /Q


REM The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
set JD=C:\ProgramData\chocolatey\lib\jq\tools\jq.exe

%JD%  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile
%JD%  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile
%JD%  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile
%JD%  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile

del tmpfile
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%

echo Version:  %VERSION%

set FILE="%RELEASESDIR%\%MODNAME%-v%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData | findstr /I "archive everything"

rem pause