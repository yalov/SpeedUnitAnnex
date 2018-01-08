
@echo off

set GAMEPATH=c:\Users\User\Games\Kerbal Space Program 1.3.1 eng
set MODNAME=SpeedUnitDash
echo %GAMEPATH%

REM // copy dll and version to Rep/GameData
copy /Y "%1%2" "GameData\%MODNAME%"\Plugins
copy /Y %MODNAME%.version GameData\%MODNAME%

REM // copy dll and version in Rep/GameData to GAMEPATH
mkdir "%GAMEPATH%\GameData\%MODNAME%"
xcopy /y /s  /i GameData\%MODNAME% "%GAMEPATH%\GameData\%MODNAME%"
