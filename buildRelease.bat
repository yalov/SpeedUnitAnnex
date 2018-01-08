
@echo off

set MODNAME=SpeedUnitDash
set LICENSE=SpeedUnitDash-License.txt
REM set README=ReadMe.txt

REM // copy dll and version to Rep/GameData
copy /Y "%1%2" "GameData\%MODNAME%"\Plugins
copy /Y %MODNAME%.version GameData\%MODNAME%
REM copy /Y ..\MiniAVC.dll GameData\%MODNAME%

if "%LICENSE%" NEQ "" copy /y  %LICENSE% GameData\%MODNAME%
if "%README%" NEQ "" copy /Y %README% GameData\%MODNAME%


set RELEASESDIR=releases
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
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%

echo Version:  %VERSION%

set FILE="%RELEASESDIR%\%MODNAME%-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData
