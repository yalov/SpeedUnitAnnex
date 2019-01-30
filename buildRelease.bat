
@echo off

rem Put the following text into the Post-build event command line:
rem without the "rem":

rem start /D $(SolutionDir) /WAIT buildDeploy.bat  $(TargetDir) $(TargetFileName)
rem  
rem if $(ConfigurationName) == Release (
rem  
rem start /D $(SolutionDir) /WAIT buildRelease.bat $(TargetDir) $(TargetFileName)
rem  
rem )

rem Set variables here

set MODNAME=SpeedUnitAnnex
set LICENSE=SpeedUnitAnnex-License.txt
set CHANGELOG=ChangeLog.md

if "%LICENSE%" NEQ "" xcopy %LICENSE% GameData\%MODNAME% /Y /I
if "%README%"  NEQ "" xcopy %README%  GameData\%MODNAME% /Y /I
if "%CHANGELOG%"  NEQ "" xcopy %CHANGELOG%  GameData\%MODNAME% /Y /I


set RELEASESDIR=Releases
set ZIP="c:\Program Files\7-zip\7z.exe"

set VERSIONFILE=%MODNAME%.version
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
del tmp.version
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%

echo Version:  %VERSION%

set FILE="%RELEASESDIR%\%MODNAME%-v%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData

pause