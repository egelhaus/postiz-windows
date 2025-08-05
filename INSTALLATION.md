# Postiz Windows - Installation Guide

## 📋 Prerequisites

Before installing Postiz Windows, ensure you have:

1. **Windows 10/11** (64-bit)
2. **Docker Desktop** installed and running
   - Download from: https://www.docker.com/products/docker-desktop/
   - Make sure it's running before proceeding with Postiz setup

## 🚀 Installation Methods

### Method 1: Using the Installer (Recommended)

1. **Download or Build the Installer**:
   ```powershell
   # If you have the source code, build it first:
   dotnet build src/PostizWindows.sln --configuration Release
   
   # IMPORTANT: Publish the applications as self-contained executables:
   dotnet publish src/PostizApp/PostizApp.csproj --configuration Release --runtime win-x64 --self-contained true --output publish/PostizApp
   dotnet publish src/PostizConfigurator/PostizConfigurator.csproj --configuration Release --runtime win-x64 --self-contained true --output publish/PostizConfigurator
   ```

2. **Run the Installer**:
   ```powershell
   # Navigate to the installer location
   .\src\PostizInstaller\bin\Release\net8.0-windows\PostizInstaller.exe
   ```
   
   **⚠️ Important**: The installer requires administrator privileges and will prompt for UAC elevation when launched.

3. **Follow the Installation Wizard**:
   - Choose installation directory (default: `C:\Program Files\Postiz`)
   - Click "Install" to install both applications
   - Wait for installation to complete

4. **Launch Applications**:
   - **Desktop Shortcuts**: Find "Postiz" and "Postiz Configurator" on your desktop
   - **Start Menu**: Look for "Postiz" folder in Start Menu

### Method 2: Manual Installation (Advanced Users)

1. **Build the Applications**:
   ```powershell
   dotnet build src/PostizWindows.sln --configuration Release
   ```

2. **Copy Built Applications** to your preferred location:
   ```
   PostizApp.exe      → from src\PostizApp\bin\Release\net8.0-windows\
   PostizConfigurator.exe → from src\PostizConfigurator\bin\Release\net8.0-windows\
   ```

3. **Create Shortcuts** manually (optional)

## ⚙️ Initial Setup

After installation, you need to configure Postiz:

### 1. Run Postiz Configurator

- **From Desktop**: Double-click "Postiz Configurator"
- **From Start Menu**: Start Menu → Postiz → Postiz Configurator
- **From File**: Run `PostizConfigurator.exe`

### 2. Initial Docker Setup

1. **Check Docker Status**:
   - The configurator will automatically check if Docker Desktop is running
   - If not running, start Docker Desktop and click "Refresh Status"

2. **Run Docker Setup**:
   - Click "Setup with Docker" button
   - Wait for the process to complete (may take 5-10 minutes on first run)
   - The configurator will:
     - Create `%USERPROFILE%\Postiz` folder
     - Generate `.env` configuration file
     - Create `docker-compose.yml`
     - Download and start Postiz Docker containers

3. **Setup Complete**:
   - You'll see a success message when done
   - Postiz will be available at http://localhost:5000

### 3. Configure Social Media Providers (Optional)

1. **In Postiz Configurator**, click "Social Media" tab
2. **Select a Provider** from the list (X/Twitter, LinkedIn, Reddit, etc.)
3. **Enter Credentials**:
   - Client ID
   - Client Secret
   - Additional fields (if required)
4. **Save & Deploy** - This will automatically restart containers with new settings

## 🎯 Using Postiz

### Main Postiz Application

1. **Launch Postiz**:
   - **From Desktop**: Double-click "Postiz"
   - **From Start Menu**: Start Menu → Postiz → Postiz
   - **From File**: Run `PostizApp.exe`

2. **Application Features**:
   - **Native Windows Interface**: Embedded web interface at localhost:5000
   - **Auto-Start Detection**: Automatically checks if containers are running
   - **Quick Actions**: Home, Refresh, and Settings buttons
   - **Status Monitoring**: Real-time connection status

### Configurator Features

The **Postiz Configurator** provides:

- **🔧 Initial Setup**: First-time Docker installation and configuration
- **🔗 Social Media Providers**: Configure API keys for 14+ platforms
- **🔄 Update**: Pull latest Postiz Docker images (NEW!)
- **📊 Status Monitoring**: Check Docker and container status

## 🔄 Updating Postiz

To update Postiz to the latest version:

1. **Open Postiz Configurator**
2. **Click "Update" button** in the top menu
3. **Confirm Update** when prompted
4. **Wait for Process** to complete (downloads latest Docker images)
5. **Restart Complete** - Postiz will restart with latest version

The update process:
- Pulls latest Docker images from GitHub Container Registry
- Restarts containers with new images
- Preserves all your configuration and data

## 📁 File Locations

### Application Files
- **Installation Directory**: `C:\Program Files\Postiz` (default)
- **Desktop Shortcuts**: `%USERPROFILE%\Desktop\`
- **Start Menu**: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Postiz\`

### Configuration Files
- **Postiz Data**: `%USERPROFILE%\Postiz\`
  - `.env` - Environment variables and API keys
  - `docker-compose.yml` - Docker configuration
  - `config/` - Application configuration
  - `uploads/` - User uploads
  - `db/` - PostgreSQL database
  - `redis/` - Redis cache

## 🛠️ Troubleshooting

### Installation Issues

**Problem**: "Executable is not compatible with OS" error when launching applications
- **Solution**: Ensure you have .NET 8.0 Runtime installed
- **Download**: https://dotnet.microsoft.com/download/dotnet/8.0
- **Required**: .NET 8.0 Desktop Runtime (x64)

**Problem**: Applications not found after installation
- **Check**: Verify files were copied to installation directory (default: `C:\Program Files\Postiz`)
- **Solution**: Try reinstalling with administrator privileges
- **Check**: Look for shortcuts on Desktop and Start Menu → Postiz

**Problem**: Configurator won't launch after installation
- **Solution**: Navigate to installation folder and run `PostizConfigurator.exe` directly
- **Check**: Ensure all DLL files are present in the installation directory

### Docker Issues
- **Error**: "Docker Desktop is not running"
  - **Solution**: Start Docker Desktop and wait for it to fully load
- **Error**: "Failed to start containers"
  - **Solution**: Check Docker Desktop is working, restart it if needed

### Port Conflicts
- **Error**: "Port 5000 already in use"
  - **Solution**: Stop other applications using port 5000, or modify `docker-compose.yml`

### Container Issues
- **Problem**: Postiz not loading
  - **Solution**: Use Configurator → Update to restart containers
- **Problem**: Changes not reflected
  - **Solution**: Use "Save & Deploy" button to restart containers

### Access Issues
- **Problem**: Can't access Postiz interface
  - **Solution**: 
    1. Check containers are running in Docker Desktop
    2. Visit http://localhost:5000 directly in browser
    3. Use Configurator to restart containers

## 🔧 Advanced Configuration

### Custom Docker Settings
Edit `%USERPROFILE%\Postiz\docker-compose.yml` to:
- Change ports
- Modify volume mappings
- Add custom environment variables

### Manual Container Management
```powershell
# Navigate to Postiz directory
cd "%USERPROFILE%\Postiz"

# Start containers
docker compose up -d

# Stop containers
docker compose down

# View logs
docker compose logs

# Update images
docker compose pull
docker compose up -d --force-recreate
```

## 📞 Support

If you encounter issues:
1. Check Docker Desktop is running and healthy
2. Try the Update button in Configurator
3. Check the GitHub repository for issues: https://github.com/egelhaus/postiz-windows
4. Review Docker Desktop logs for container errors

---

**🎉 Enjoy using Postiz on Windows!**
