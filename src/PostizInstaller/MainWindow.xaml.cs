using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace PostizInstaller;

public partial class MainWindow : Window
{
    private string? installPath;
    private readonly string tempExtractionPath;

    public MainWindow()
    {
        InitializeComponent();
        tempExtractionPath = Path.Combine(Path.GetTempPath(), "PostizInstaller");
        
        // Set default installation path
        installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Postiz");
        InstallPathTextBox.Text = installPath;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select installation directory",
            SelectedPath = installPath ?? ""
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            installPath = dialog.SelectedPath;
            InstallPathTextBox.Text = installPath;
        }
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(installPath))
        {
            System.Windows.MessageBox.Show("Please select an installation directory.", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            InstallButton.IsEnabled = false;
            InstallButton.Content = "Installing...";
            ProgressBar.Visibility = Visibility.Visible;
            StatusText.Text = "Starting installation...";

            await PerformInstallation();

            StatusText.Text = "✅ Installation completed successfully!";
            System.Windows.MessageBox.Show(
                "Postiz has been installed successfully!\n\n" +
                "You can now find:\n" +
                "• Postiz - Main application\n" +
                "• Postiz Configurator - Configuration tool\n\n" +
                "Both applications are available in your Start Menu and Desktop.",
                "Installation Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Ask if user wants to launch configurator
            var result = System.Windows.MessageBox.Show(
                "Would you like to launch Postiz Configurator now to set up Postiz?",
                "Launch Configurator",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LaunchConfigurator();
            }

            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Installation failed";
            System.Windows.MessageBox.Show($"Installation failed: {ex.Message}", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            InstallButton.Content = "Install";
            InstallButton.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private async Task PerformInstallation()
    {
        // Create installation directory
        StatusText.Text = "Creating installation directory...";
        Directory.CreateDirectory(installPath!);

        // Extract embedded applications (simulated - in real scenario, these would be embedded resources)
        StatusText.Text = "Extracting application files...";
        await ExtractApplicationFiles();

        // Create shortcuts
        StatusText.Text = "Creating shortcuts...";
        CreateShortcuts();

        // Register applications
        StatusText.Text = "Registering applications...";
        RegisterApplications();

        await Task.Delay(1000); // Simulate work
    }

    private async Task ExtractApplicationFiles()
    {
        // Copy from pre-published self-contained executables
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
        
        var postizAppSourcePath = Path.Combine(solutionDir, "publish", "PostizApp");
        var configuratorSourcePath = Path.Combine(solutionDir, "publish", "PostizConfigurator");
        
        var postizAppPath = Path.Combine(installPath!, "PostizApp");
        var configuratorPath = Path.Combine(installPath!, "PostizConfigurator");
        
        Directory.CreateDirectory(postizAppPath);
        Directory.CreateDirectory(configuratorPath);

        StatusText.Text = "Copying PostizApp files...";
        // Copy PostizApp files
        if (Directory.Exists(postizAppSourcePath))
        {
            await CopyDirectoryAsync(postizAppSourcePath, postizAppPath);
        }
        else
        {
            throw new DirectoryNotFoundException($"PostizApp publish output not found at: {postizAppSourcePath}. Please run 'dotnet publish' for both applications first.");
        }

        StatusText.Text = "Copying PostizConfigurator files...";
        // Copy PostizConfigurator files  
        if (Directory.Exists(configuratorSourcePath))
        {
            await CopyDirectoryAsync(configuratorSourcePath, configuratorPath);
        }
        else
        {
            throw new DirectoryNotFoundException($"PostizConfigurator publish output not found at: {configuratorSourcePath}. Please run 'dotnet publish' for both applications first.");
        }
    }

    private async Task CopyDirectoryAsync(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        
        // Copy all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, true);
            await Task.Delay(10); // Small delay to show progress
        }
        
        // Copy all subdirectories recursively
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dir);
            var destSubDir = Path.Combine(destDir, dirName);
            await CopyDirectoryAsync(dir, destSubDir);
        }
    }

    private void CreateShortcuts()
    {
        // Create desktop shortcuts
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        CreateShortcut(
            Path.Combine(desktopPath, "Postiz.lnk"),
            Path.Combine(installPath!, "PostizApp", "PostizApp.exe"),
            "Postiz - Social Media Management");
        
        CreateShortcut(
            Path.Combine(desktopPath, "Postiz Configurator.lnk"),
            Path.Combine(installPath!, "PostizConfigurator", "PostizConfigurator.exe"),
            "Postiz Configurator - Setup and Configuration");

        // Create Start Menu shortcuts
        var startMenuPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
            "Programs", "Postiz");
        
        Directory.CreateDirectory(startMenuPath);
        
        CreateShortcut(
            Path.Combine(startMenuPath, "Postiz.lnk"),
            Path.Combine(installPath!, "PostizApp", "PostizApp.exe"),
            "Postiz - Social Media Management");
        
        CreateShortcut(
            Path.Combine(startMenuPath, "Postiz Configurator.lnk"),
            Path.Combine(installPath!, "PostizConfigurator", "PostizConfigurator.exe"),
            "Postiz Configurator - Setup and Configuration");
    }

    private void CreateShortcut(string shortcutPath, string targetPath, string description)
    {
        try
        {
            // Use Windows Script Host to create proper .lnk files
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType != null)
            {
                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell != null)
                {
                    var shortcut = shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = targetPath;
                    shortcut.Description = description;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                    shortcut.Save();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to create shortcut {shortcutPath}: {ex.Message}");
            // Fallback: create a simple batch file
            try
            {
                var batchPath = shortcutPath.Replace(".lnk", ".bat");
                File.WriteAllText(batchPath, $"@echo off\nstart \"\" \"{targetPath}\"\n");
            }
            catch
            {
                // If all else fails, ignore the shortcut
            }
        }
    }

    private void RegisterApplications()
    {
        try
        {
            // Register in Windows Registry for Add/Remove Programs
            using var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Postiz");
            key.SetValue("DisplayName", "Postiz - Social Media Management");
            key.SetValue("DisplayVersion", "1.0.0");
            key.SetValue("Publisher", "Postiz");
            key.SetValue("InstallLocation", installPath);
            key.SetValue("UninstallString", Path.Combine(installPath!, "Uninstall.exe"));
            key.SetValue("DisplayIcon", Path.Combine(installPath!, "PostizApp", "PostizApp.exe"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to register application: {ex.Message}");
        }
    }

    private void LaunchConfigurator()
    {
        try
        {
            var configuratorPath = Path.Combine(installPath!, "PostizConfigurator", "PostizConfigurator.exe");
            if (File.Exists(configuratorPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = configuratorPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(configuratorPath)
                };
                Process.Start(startInfo);
            }
            else
            {
                System.Windows.MessageBox.Show($"Configurator not found at: {configuratorPath}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to launch configurator: {ex.Message}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
