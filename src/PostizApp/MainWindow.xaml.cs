using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Shared;

namespace PostizApp;

public partial class MainWindow : Window
{
    private WebView2? webView;
    private bool isDockerRunning = false;

    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await CheckDockerStatus();
        await InitializeWebView();
    }

    private async Task CheckDockerStatus()
    {
        try
        {
            isDockerRunning = await DockerHelper.IsDockerRunningAsync();
            
            if (!isDockerRunning)
            {
                ShowDockerNotRunningMessage();
                return;
            }

            // Check if Postiz containers are running
            var postizPath = EnvFileHelper.GetPostizFolderPath();
            if (Directory.Exists(postizPath))
            {
                var (success, output) = await DockerHelper.RunDockerComposeAsync(postizPath, "ps --services --filter status=running");
                if (!success || !output.Contains("postiz"))
                {
                    ShowPostizNotRunningMessage();
                }
            }
            else
            {
                ShowConfigurationNeededMessage();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error checking Docker status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task InitializeWebView()
    {
        try
        {
            webView = new WebView2();
            
            // Set up WebView2
            var environment = await CoreWebView2Environment.CreateAsync();
            await webView.EnsureCoreWebView2Async(environment);
            
            // Configure WebView2
            webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
            webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            webView.CoreWebView2.Settings.IsWebMessageEnabled = false;
            
            // Handle navigation events
            webView.CoreWebView2.NavigationStarting += WebView_NavigationStarting;
            webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
            
            // Add WebView2 to the main grid
            MainGrid.Children.Add(webView);
            Grid.SetRow(webView, 1);
            
            // Navigate to Postiz
            if (isDockerRunning)
            {
                webView.CoreWebView2.Navigate("http://localhost:5000");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        LoadingIndicator.Visibility = Visibility.Visible;
        StatusText.Text = "Loading...";
    }

    private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        LoadingIndicator.Visibility = Visibility.Collapsed;
        
        if (e.IsSuccess)
        {
            StatusText.Text = "Connected to Postiz";
        }
        else
        {
            StatusText.Text = "Failed to connect to Postiz";
            ShowConnectionErrorMessage();
        }
    }

    private void ShowDockerNotRunningMessage()
    {
        var result = MessageBox.Show(
            "Docker Desktop is not running. Please start Docker Desktop and try again.\n\nWould you like to open Postiz Configurator to set up Docker?",
            "Docker Not Running",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            OpenPostizConfigurator();
        }
    }

    private void ShowPostizNotRunningMessage()
    {
        var result = MessageBox.Show(
            "Postiz containers are not running. Would you like to start them?\n\nThis may take a few minutes on first launch.",
            "Postiz Not Running",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result == MessageBoxResult.Yes)
        {
            StartPostizContainers();
        }
    }

    private void ShowConfigurationNeededMessage()
    {
        var result = MessageBox.Show(
            "Postiz has not been configured yet. Would you like to open Postiz Configurator to set it up?",
            "Configuration Needed",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result == MessageBoxResult.Yes)
        {
            OpenPostizConfigurator();
        }
    }

    private void ShowConnectionErrorMessage()
    {
        MessageBox.Show(
            "Failed to connect to Postiz. Please make sure Postiz is running and try refreshing.",
            "Connection Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private async void StartPostizContainers()
    {
        try
        {
            StatusText.Text = "Starting Postiz...";
            LoadingIndicator.Visibility = Visibility.Visible;

            var postizPath = EnvFileHelper.GetPostizFolderPath();
            var (success, output) = await DockerHelper.RunDockerComposeAsync(postizPath, "up -d");

            if (success)
            {
                StatusText.Text = "Postiz started successfully";
                // Wait a moment for services to be ready, then navigate
                await Task.Delay(5000);
                webView?.CoreWebView2.Navigate("http://localhost:5000");
            }
            else
            {
                StatusText.Text = "Failed to start Postiz";
                MessageBox.Show($"Failed to start Postiz containers:\n{output}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = "Error starting Postiz";
            MessageBox.Show($"Error starting Postiz: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }
    }

    private void OpenPostizConfigurator()
    {
        try
        {
            var configuratorPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
                "PostizConfigurator.exe");

            if (File.Exists(configuratorPath))
            {
                Process.Start(configuratorPath);
            }
            else
            {
                MessageBox.Show("Postiz Configurator not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open Postiz Configurator: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        webView?.CoreWebView2.Reload();
    }

    private void ConfigButton_Click(object sender, RoutedEventArgs e)
    {
        OpenPostizConfigurator();
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        webView?.CoreWebView2.Navigate("http://localhost:5000");
    }
}
