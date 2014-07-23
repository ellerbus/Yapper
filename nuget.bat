@ECHO OFF
SETLOCAL
SET VERSION=%1

nuget.exe pack Yapper\Yapper.csproj -Version "0.1.2-beta" -Build -OutputDirectory nupack -Properties Configuration=Release
