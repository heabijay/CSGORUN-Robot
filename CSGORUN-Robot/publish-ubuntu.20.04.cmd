@echo off
dotnet publish -c release -r ubuntu.20.04-x64 --self-contained false
dotnet publish -c release -r ubuntu.20.04-arm64 --self-contained false