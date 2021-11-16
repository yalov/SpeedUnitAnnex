@echo off
echo "copy release script to all specified folders"

FOR %%a IN (
"..\CommNetAntennasConsumptor"
"..\CommNetAntennasExtension"
"..\CommNetAntennasInfo"
"..\CommunityPartsTitles"
"..\KVASS"
"..\ScienceLabInfo"
"..\UtilityWeight"
"..\WaterLaunchSites"
) DO (
	echo %%a
	xcopy "release*.py" %%a /Y /I /Q
)

pause
