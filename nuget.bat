@ECHO OFF
SETLOCAL
SET VERSION=%1

nuget.exe pack Yapper\Yapper.csproj -Version "0.2.0-beta" -Build -OutputDirectory nupack -Properties Configuration=Release
