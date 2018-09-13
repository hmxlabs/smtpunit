ECHO "Building SMTPUnit Nuget package"

msbuild SMTPUnit.sln /t:Clean;Rebuild /p:Configuration=Release
Nuget.exe pack smtpunit.nuspec
move *.nupkg .\Build\Output\Release
