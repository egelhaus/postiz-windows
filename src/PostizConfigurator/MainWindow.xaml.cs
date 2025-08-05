using System.IO;
using System.Windows;
using System.Windows.Controls;
using Shared;

namespace PostizConfigurator;

public partial class MainWindow : Window
{
    private bool isInitialSetup = false;

    public MainWindow()
    {
        InitializeComponent();
        CheckInitialSetup();
    }

    private void CheckInitialSetup()
    {
        var postizFolder = EnvFileHelper.GetPostizFolderPath();
        var envPath = EnvFileHelper.GetEnvFilePath();
        var dockerComposePath = EnvFileHelper.GetDockerComposePath();

        isInitialSetup = !Directory.Exists(postizFolder) || !File.Exists(envPath) || !File.Exists(dockerComposePath);

        if (isInitialSetup)
        {
            ShowInitialSetupView();
        }
        else
        {
            ShowProviderManagementView();
        }
    }

    private void ShowInitialSetupView()
    {
        Title = "Postiz Configurator - Initial Setup";
        ContentFrame.Navigate(new InitialSetupPage());
    }

    private void ShowProviderManagementView()
    {
        Title = "Postiz Configurator - Social Media Providers";
        ContentFrame.Navigate(new ProvidersPage());
    }

    private void InitialSetupButton_Click(object sender, RoutedEventArgs e)
    {
        ShowInitialSetupView();
    }

    private void ProvidersButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProviderManagementView();
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateButton.IsEnabled = false;
            UpdateButton.Content = "Updating...";
            
            var result = MessageBox.Show(
                "This will update Postiz to the latest version by pulling the latest Docker images.\n\nThis may take a few minutes. Continue?",
                "Update Postiz",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                UpdateButton.Content = "Update";
                UpdateButton.IsEnabled = true;
                return;
            }

            var postizPath = EnvFileHelper.GetPostizFolderPath();
            
            if (!Directory.Exists(postizPath))
            {
                MessageBox.Show("Postiz is not installed. Please run initial setup first.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pull latest images and restart containers
            var (success, output) = await DockerHelper.RunDockerComposeAsync(postizPath, "pull");
            
            if (success)
            {
                // Restart with new images
                var (restartSuccess, restartOutput) = await DockerHelper.RunDockerComposeAsync(postizPath, "up -d --force-recreate");
                
                if (restartSuccess)
                {
                    MessageBox.Show(
                        "Postiz has been updated successfully!\n\nThe containers have been restarted with the latest images.",
                        "Update Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Images were updated, but failed to restart containers:\n\n{restartOutput}",
                        "Update Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    $"Failed to update Postiz images:\n\n{output}",
                    "Update Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Update failed: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            UpdateButton.Content = "Update";
            UpdateButton.IsEnabled = true;
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
