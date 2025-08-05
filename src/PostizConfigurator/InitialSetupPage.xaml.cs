using System.IO;
using System.Windows;
using System.Windows.Controls;
using Shared;

namespace PostizConfigurator;

public partial class InitialSetupPage : Page
{
    public InitialSetupPage()
    {
        InitializeComponent();
        _ = CheckDockerStatus(); // Fire and forget since constructor can't be async
    }

    private async Task CheckDockerStatus()
    {
        var isDockerInstalled = await DockerHelper.IsDockerDesktopInstalledAsync();
        var isDockerRunning = await DockerHelper.IsDockerRunningAsync();

        DockerStatusText.Text = isDockerInstalled 
            ? (isDockerRunning ? "✅ Docker Desktop is installed and running" : "⚠️ Docker Desktop is installed but not running")
            : "❌ Docker Desktop is not installed";

        DockerSetupButton.IsEnabled = isDockerInstalled && isDockerRunning;
        
        if (!isDockerInstalled)
        {
            DockerInstructionsText.Text = "Please install Docker Desktop from https://docker.com/products/docker-desktop and restart this application.";
        }
        else if (!isDockerRunning)
        {
            DockerInstructionsText.Text = "Please start Docker Desktop and click 'Refresh Status'.";
        }
        else
        {
            DockerInstructionsText.Text = "Docker Desktop is ready. You can proceed with Docker setup.";
        }
    }

    private async void RefreshStatusButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshStatusButton.IsEnabled = false;
        RefreshStatusButton.Content = "Checking...";
        
        await CheckDockerStatus();
        
        RefreshStatusButton.Content = "Refresh Status";
        RefreshStatusButton.IsEnabled = true;
    }

    private async void DockerSetupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DockerSetupButton.IsEnabled = false;
            DockerSetupButton.Content = "Setting up...";
            ProgressBar.Visibility = Visibility.Visible;
            StatusText.Text = "Creating Postiz folder and configuration files...";

            // Create Postiz folder
            EnvFileHelper.EnsurePostizFolderExists();
            var postizFolder = EnvFileHelper.GetPostizFolderPath();

            // Create .env file
            var envVars = new Dictionary<string, string>();
            EnvFileHelper.WriteEnvFile(EnvFileHelper.GetEnvFilePath(), envVars);

            // Create docker-compose.yml
            await CreateDockerComposeFile();

            // Debug: Verify the generated file
            var dockerComposePath = EnvFileHelper.GetDockerComposePath();
            var generatedContent = await File.ReadAllTextAsync(dockerComposePath);
            
            StatusText.Text = "Starting Postiz containers... This may take a few minutes on first run.";

            // Start Docker containers
            var (success, output) = await DockerHelper.RunDockerComposeAsync(postizFolder, "up -d");

            if (success)
            {
                StatusText.Text = "✅ Postiz setup completed successfully!";
                MessageBox.Show(
                    "Postiz has been set up successfully!\n\n" +
                    "You can now:\n" +
                    "• Open the Postiz app to access the web interface\n" +
                    "• Use this configurator to add social media providers\n" +
                    "• Access Postiz at http://localhost:5000",
                    "Setup Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusText.Text = "❌ Failed to start Postiz containers";
                MessageBox.Show($"Failed to start Postiz containers:\n\nWorking Directory: {postizFolder}\nDocker Compose File: {dockerComposePath}\nCommand: docker compose up -d\n\nError Output:\n{output}", "Setup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Setup failed";
            MessageBox.Show($"Setup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            DockerSetupButton.Content = "Setup with Docker";
            DockerSetupButton.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private async Task CreateDockerComposeFile()
    {
        var dockerComposePath = EnvFileHelper.GetDockerComposePath();
        var postizFolderWindows = EnvFileHelper.GetPostizFolderPath().Replace("\\", "/");
        
        var dockerComposeTemplate = @"services:
  postiz:
    image: ghcr.io/gitroomhq/postiz-app:latest
    container_name: postiz
    restart: always
    env_file:
      - {POSTIZ_FOLDER}/.env
    volumes:
      - {POSTIZ_FOLDER}/config:/config/
      - {POSTIZ_FOLDER}/uploads:/uploads/
    ports:
      - 5000:5000
    networks:
      - postiz-network
    depends_on:
      postiz-postgres:
        condition: service_healthy
      postiz-redis:
        condition: service_healthy
 
  postiz-postgres:
    image: postgres:17-alpine
    container_name: postiz-postgres
    restart: always
    environment:
      POSTGRES_PASSWORD: postiz-password
      POSTGRES_USER: postiz-user
      POSTGRES_DB: postiz-db-local
    volumes:
      - {POSTIZ_FOLDER}/db:/var/lib/postgresql/data
    networks:
      - postiz-network
    healthcheck:
      test: pg_isready -U postiz-user -d postiz-db-local
      interval: 10s
      timeout: 3s
      retries: 3
      
  postiz-redis:
    image: redis:7.2
    container_name: postiz-redis
    restart: always
    healthcheck:
      test: redis-cli ping
      interval: 10s
      timeout: 3s
      retries: 3
    volumes:
      - {POSTIZ_FOLDER}/redis:/data
    networks:
      - postiz-network

networks:
  postiz-network:
    driver: bridge";

        var dockerComposeContent = dockerComposeTemplate.Replace("{POSTIZ_FOLDER}", postizFolderWindows);
        await File.WriteAllTextAsync(dockerComposePath, dockerComposeContent);
    }

    private void ManualSetupButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Manual setup is not yet implemented.\n\nPlease use Docker setup for now.",
            "Manual Setup",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
