
@echo off
setlocal enabledelayedexpansion


set GAMEPATH[0]=c:\Users\User\Games\Kerbal Space Program 1.4.2 rus
set GAMEPATH[1]=c:\Users\User\Games\Kerbal Space Program 1.4.2 eng

set MODNAME=SpeedUnitAnnex

REM // copy dll and version to Repository/GameData
xcopy "%1%2" "GameData\%MODNAME%\Plugins\" /Y
xcopy "%MODNAME%.version" "GameData\%MODNAME%\" /Y 

REM // copy Repository/GameData to GAMEPATH
REM mkdir "%GAMEPATH%\GameData\%MODNAME%"

set "x=0" 
:SymLoop 

if defined GAMEPATH[%x%] ( 
   call echo %%GAMEPATH[%x%]%% 
   set /a "x+=1"
   GOTO :SymLoop 
)
echo "The length of the array is" %x%

for /l %%n in (0,1,%x%) do ( 
   xcopy "GameData\%MODNAME%" "!GAMEPATH[%%n]!\GameData\%MODNAME%\" /Y /S /I
)