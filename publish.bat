rd /s /q Publish
dotnet publish src\ManagementPortal\ManagementPortal.csproj -o ../../Publish
copy install.bat .\Publish
copy uninstall.bat .\Publish
cd publish
git clone https://github.com/DotNetTechLab/xtermjs-binary
cd xtermjs-binary
rd /s /q .git
cd ..
cd ..
