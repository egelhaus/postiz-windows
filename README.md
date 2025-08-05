# Postiz Windows

A Windows application suite for installing and managing Postiz, a social media management platform.

## Overview

This repository contains three Windows applications:

1. **PostizInstaller** - Main installer that sets up both Postiz and Postiz Configurator on the user's machine
2. **PostizApp** - The main Postiz application with embedded WebView2 that provides a native desktop experience for the Postiz web interface
3. **PostizConfigurator** - Configuration tool for initial Docker setup and social media provider management

## Features

### PostizInstaller
- Installs both Postiz and Postiz Configurator applications
- Creates desktop and Start Menu shortcuts
- Registers applications in Windows Add/Remove Programs
- Simple wizard-style interface

### PostizApp
- Embedded WebView2 control displaying Postiz at http://localhost:5000
- Native Windows application feel
- Automatic Docker container status checking
- Integration with Postiz Configurator
- Custom toolbar with navigation controls

### PostizConfigurator
- **Initial Setup**: Checks Docker Desktop installation and sets up Postiz containers
- **Provider Management**: Configure social media providers (X/Twitter, LinkedIn, Reddit, GitHub, etc.)
- Automatic Docker container redeployment after configuration changes
- User-friendly interface for managing API keys and secrets

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Docker Desktop (for Postiz functionality)

## Build Instructions

1. Open `src/PostizWindows.sln` in Visual Studio 2022
2. Restore NuGet packages
3. Build the solution in Release mode
4. Applications will be built in their respective bin/Release directories

## Installation Process

1. Run `PostizInstaller.exe`
2. Select installation directory
3. Click "Install" to install both applications
4. Launch Postiz Configurator for initial setup
5. Choose Docker setup method
6. Configure social media providers as needed
7. Launch Postiz application to access the web interface

## Docker Integration

The application uses Docker Compose to run Postiz with the following services:
- **Postiz**: Main application container (ghcr.io/gitroomhq/postiz-app:latest)
- **PostgreSQL**: Database container
- **Redis**: Cache container

All configuration is managed through:
- `%USERPROFILE%\Postiz\.env` - Environment variables
- `%USERPROFILE%\Postiz\docker-compose.yml` - Docker Compose configuration

## Social Media Providers

Supported providers include:
- X (Twitter)
- LinkedIn
- Reddit
- GitHub
- Beehiiv
- Threads
- Facebook
- YouTube
- TikTok
- Pinterest
- Dribbble
- Discord
- Slack
- Mastodon

Each provider requires Client ID and Client Secret configuration through the Postiz Configurator.

## Project Structure

```
src/
├── PostizWindows.sln
├── PostizInstaller/          # Main installer application
├── PostizApp/               # Main Postiz application with WebView2
├── PostizConfigurator/      # Configuration and setup tool
└── Shared/                  # Shared utilities and helpers
    ├── DockerHelper.cs      # Docker command utilities
    ├── EnvFileHelper.cs     # Environment file management
    └── SocialMediaProvider.cs # Provider definitions
```

## Development

The solution uses:
- **WPF** for UI framework
- **WebView2** for embedded web content in PostizApp
- **.NET 8.0** as target framework
- **Shared library** for common functionality across applications

## License

This project is licensed under the same terms as the main Postiz project.