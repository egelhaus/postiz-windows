using System.Windows;
using System.Windows.Controls;
using Shared;

namespace PostizConfigurator;

public partial class ProvidersPage : Page
{
    private List<SocialMediaProvider> providers = new();
    private Dictionary<string, string> currentEnvVars = new();

    public ProvidersPage()
    {
        InitializeComponent();
        LoadProviders();
    }

    private void LoadProviders()
    {
        providers = SocialMediaProviders.GetAllProviders();
        currentEnvVars = EnvFileHelper.ReadEnvFile(EnvFileHelper.GetEnvFilePath());

        // Update provider data with current values
        foreach (var provider in providers)
        {
            provider.ClientId = currentEnvVars.GetValueOrDefault(provider.ClientIdKey, "");
            provider.ClientSecret = currentEnvVars.GetValueOrDefault(provider.ClientSecretKey, "");
            if (!string.IsNullOrEmpty(provider.AdditionalKey))
            {
                provider.AdditionalValue = currentEnvVars.GetValueOrDefault(provider.AdditionalKey, "");
            }
        }

        ProvidersListBox.ItemsSource = providers;
    }

    private void ProvidersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProvidersListBox.SelectedItem is SocialMediaProvider provider)
        {
            ShowProviderDetails(provider);
        }
        else
        {
            HideProviderDetails();
        }
    }

    private void ShowProviderDetails(SocialMediaProvider provider)
    {
        ProviderDetailsPanel.Visibility = Visibility.Visible;
        
        ProviderNameText.Text = provider.DisplayName;
        ClientIdTextBox.Text = provider.ClientId;
        ClientSecretTextBox.Password = provider.ClientSecret;
        
        if (!string.IsNullOrEmpty(provider.AdditionalKey))
        {
            AdditionalPanel.Visibility = Visibility.Visible;
            AdditionalLabel.Content = $"{provider.AdditionalKeyDisplayName}:";
            AdditionalTextBox.Text = provider.AdditionalValue ?? "";
        }
        else
        {
            AdditionalPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void HideProviderDetails()
    {
        ProviderDetailsPanel.Visibility = Visibility.Collapsed;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProvidersListBox.SelectedItem is not SocialMediaProvider provider)
            return;

        try
        {
            SaveButton.IsEnabled = false;
            SaveButton.Content = "Saving...";
            StatusText.Text = "Saving configuration...";

            // Update provider values
            provider.ClientId = ClientIdTextBox.Text.Trim();
            provider.ClientSecret = ClientSecretTextBox.Password.Trim();
            if (!string.IsNullOrEmpty(provider.AdditionalKey))
            {
                provider.AdditionalValue = AdditionalTextBox.Text.Trim();
            }

            // Update environment variables
            currentEnvVars[provider.ClientIdKey] = provider.ClientId;
            currentEnvVars[provider.ClientSecretKey] = provider.ClientSecret;
            if (!string.IsNullOrEmpty(provider.AdditionalKey))
            {
                currentEnvVars[provider.AdditionalKey] = provider.AdditionalValue ?? "";
            }

            // Write to file
            EnvFileHelper.WriteEnvFile(EnvFileHelper.GetEnvFilePath(), currentEnvVars);

            StatusText.Text = "Redeploying Docker containers...";

            // Redeploy Docker containers
            var postizPath = EnvFileHelper.GetPostizFolderPath();
            var (success, output) = await DockerHelper.RunDockerComposeAsync(postizPath, "up -d --force-recreate");

            if (success)
            {
                StatusText.Text = $"✅ {provider.DisplayName} configuration saved and deployed successfully!";
                MessageBox.Show(
                    $"{provider.DisplayName} has been configured successfully!\n\nThe Postiz containers have been redeployed with the new settings.",
                    "Configuration Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusText.Text = "❌ Failed to redeploy containers";
                MessageBox.Show(
                    $"Configuration was saved, but failed to redeploy containers:\n\n{output}",
                    "Deployment Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Refresh the list to show updated status
            LoadProviders();
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Save failed";
            MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.Content = "Save & Deploy";
            SaveButton.IsEnabled = true;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProvidersListBox.SelectedItem is SocialMediaProvider provider)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to clear the configuration for {provider.DisplayName}?",
                "Clear Configuration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClientIdTextBox.Text = "";
                ClientSecretTextBox.Password = "";
                AdditionalTextBox.Text = "";
            }
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadProviders();
        StatusText.Text = "Configuration refreshed";
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will update Postiz to the latest version by pulling new Docker images and restarting containers.\n\nThis may take several minutes. Continue?",
            "Update Postiz",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            // Disable the button and show progress
            var updateButton = sender as Button;
            if (updateButton != null)
            {
                updateButton.IsEnabled = false;
                updateButton.Content = "Updating...";
            }

            StatusText.Text = "Stopping Postiz containers...";

            var postizPath = EnvFileHelper.GetPostizFolderPath();
            
            // Stop containers
            var (stopSuccess, stopOutput) = await DockerHelper.RunDockerComposeAsync(postizPath, "down");
            if (!stopSuccess)
            {
                throw new Exception($"Failed to stop containers: {stopOutput}");
            }

            StatusText.Text = "Pulling latest Docker images...";

            // Pull latest images
            var (pullSuccess, pullOutput) = await DockerHelper.RunDockerComposeAsync(postizPath, "pull");
            if (!pullSuccess)
            {
                throw new Exception($"Failed to pull images: {pullOutput}");
            }

            StatusText.Text = "Starting updated containers...";

            // Start containers with new images
            var (startSuccess, startOutput) = await DockerHelper.RunDockerComposeAsync(postizPath, "up -d");
            if (!startSuccess)
            {
                throw new Exception($"Failed to start containers: {startOutput}");
            }

            StatusText.Text = "✅ Postiz updated successfully!";
            MessageBox.Show(
                "Postiz has been updated to the latest version!\n\nThe containers are now running with the latest images.",
                "Update Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Update failed";
            MessageBox.Show($"Failed to update Postiz: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Re-enable the button
            var updateButton = sender as Button;
            if (updateButton != null)
            {
                updateButton.Content = "Update Postiz";
                updateButton.IsEnabled = true;
            }
        }
    }
}
