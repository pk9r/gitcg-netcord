@echo off
REM Build script for GitCG NetCord MainApp Docker package
REM This script builds the Docker image for the MainApp

setlocal

set PROJECT_NAME=gitcg-netcord-mainapp
set IMAGE_TAG=latest
set DOCKERFILE_PATH=src\Gitcg.NetCord.MainApp\Dockerfile

echo 🚀 Building Docker package for GitCG NetCord MainApp...

REM Check if Docker is available
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not installed or not available in PATH
    exit /b 1
)

REM Build the Docker image
echo 📦 Building Docker image: %PROJECT_NAME%:%IMAGE_TAG%
docker build -f "%DOCKERFILE_PATH%" -t "%PROJECT_NAME%:%IMAGE_TAG%" .

if %errorlevel% equ 0 (
    echo ✅ Successfully built Docker image: %PROJECT_NAME%:%IMAGE_TAG%
    echo 🔍 Image details:
    docker image ls | findstr "%PROJECT_NAME%"
    echo.
    echo 🚀 To run the container:
    echo    docker run -p 8080:8080 -p 8081:8081 %PROJECT_NAME%:%IMAGE_TAG%
    echo.
    echo 📋 Or use docker compose:
    echo    docker compose up -d
) else (
    echo ❌ Failed to build Docker image
    exit /b 1
)

endlocal