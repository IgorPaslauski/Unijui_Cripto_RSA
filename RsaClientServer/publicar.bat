@echo off
echo Publicando ServerApp e ClientApp como executaveis standalone...
echo.

dotnet publish ServerApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/ServerApp
if %errorlevel% neq 0 (
    echo Erro ao publicar ServerApp
    pause
    exit /b 1
)

dotnet publish ClientApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/ClientApp
if %errorlevel% neq 0 (
    echo Erro ao publicar ClientApp
    pause
    exit /b 1
)

echo.
echo Concluido! Os executaveis estao em:
echo   - publish\ServerApp\ServerApp.exe
echo   - publish\ClientApp\ClientApp.exe
echo.
echo Voce pode copiar essas pastas para qualquer PC Windows (64 bits) e rodar sem instalar o .NET.
echo.
pause
