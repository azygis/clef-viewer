dotnet restore ./backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj
dotnet publish ./backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj --no-restore --runtime linux-x64 --output ./publish/linux --self-contained -p:PublishTrimmed=true
dotnet publish ./backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj --no-restore --runtime win-x64 --output ./publish/win --self-contained -p:PublishTrimmed=true
