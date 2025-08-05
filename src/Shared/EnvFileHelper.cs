using System.Text;
using System.Text.RegularExpressions;

namespace Shared;

public static class EnvFileHelper
{
    public static string GetPostizFolderPath()
    {
        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userFolder, "Postiz");
    }

    public static string GetEnvFilePath()
    {
        return Path.Combine(GetPostizFolderPath(), ".env");
    }

    public static string GetDockerComposePath()
    {
        return Path.Combine(GetPostizFolderPath(), "docker-compose.yml");
    }

    public static void EnsurePostizFolderExists()
    {
        var postizFolder = GetPostizFolderPath();
        if (!Directory.Exists(postizFolder))
        {
            Directory.CreateDirectory(postizFolder);
        }
    }

    public static Dictionary<string, string> ReadEnvFile(string filePath)
    {
        var envVars = new Dictionary<string, string>();
        
        if (!File.Exists(filePath))
            return envVars;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            var match = Regex.Match(line, @"^([^=]+)=(.*)$");
            if (match.Success)
            {
                var key = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();
                
                // Remove quotes if present
                if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                envVars[key] = value;
            }
        }
        
        return envVars;
    }

    public static void WriteEnvFile(string filePath, Dictionary<string, string> envVars)
    {
        var lines = new List<string>();
        
        // Add header
        lines.Add("# Changing Settings");
        lines.Add($"JWT_SECRET=\"{envVars.GetValueOrDefault("JWT_SECRET", GenerateJwtSecret())}\"");
        lines.Add("");
        
        // Add social media settings
        lines.Add("# Social Media Settings");
        
        var socialMediaProviders = new[]
        {
            ("X_API_KEY", "X_API_SECRET"),
            ("LINKEDIN_CLIENT_ID", "LINKEDIN_CLIENT_SECRET"),
            ("REDDIT_CLIENT_ID", "REDDIT_CLIENT_SECRET"),
            ("GITHUB_CLIENT_ID", "GITHUB_CLIENT_SECRET"),
            ("BEEHIIVE_API_KEY", "BEEHIIVE_PUBLICATION_ID"),
            ("THREADS_APP_ID", "THREADS_APP_SECRET"),
            ("FACEBOOK_APP_ID", "FACEBOOK_APP_SECRET"),
            ("YOUTUBE_CLIENT_ID", "YOUTUBE_CLIENT_SECRET"),
            ("TIKTOK_CLIENT_ID", "TIKTOK_CLIENT_SECRET"),
            ("PINTEREST_CLIENT_ID", "PINTEREST_CLIENT_SECRET"),
            ("DRIBBBLE_CLIENT_ID", "DRIBBBLE_CLIENT_SECRET"),
            ("DISCORD_CLIENT_ID", "DISCORD_CLIENT_SECRET"),
            ("SLACK_ID", "SLACK_SECRET"),
            ("MASTODON_CLIENT_ID", "MASTODON_CLIENT_SECRET")
        };

        foreach (var (key1, key2) in socialMediaProviders)
        {
            if (key1 == "BEEHIIVE_API_KEY")
            {
                lines.Add($"{key1}=\"{envVars.GetValueOrDefault(key1, "")}\"");
                lines.Add($"{key2}=\"{envVars.GetValueOrDefault(key2, "")}\"");
            }
            else
            {
                lines.Add($"{key1}=\"{envVars.GetValueOrDefault(key1, "")}\"");
                lines.Add($"{key2}=\"{envVars.GetValueOrDefault(key2, "")}\"");
            }
        }
        
        // Add special cases
        lines.Add($"DISCORD_BOT_TOKEN_ID=\"{envVars.GetValueOrDefault("DISCORD_BOT_TOKEN_ID", "")}\"");
        lines.Add($"SLACK_SIGNING_SECRET=\"{envVars.GetValueOrDefault("SLACK_SIGNING_SECRET", "")}\"");
        lines.Add($"MASTODON_URL=\"{envVars.GetValueOrDefault("MASTODON_URL", "https://mastodon.social")}\"");
        
        lines.Add("");
        lines.Add("");
        
        // Add static settings
        lines.Add("# Static Settings");
        lines.Add($"MAIN_URL=\"{envVars.GetValueOrDefault("MAIN_URL", "http://localhost:5000")}\"");
        lines.Add($"FRONTEND_URL=\"{envVars.GetValueOrDefault("FRONTEND_URL", "http://localhost:5000")}\"");
        lines.Add($"NEXT_PUBLIC_BACKEND_URL=\"{envVars.GetValueOrDefault("NEXT_PUBLIC_BACKEND_URL", "http://localhost:5000/api")}\"");
        lines.Add($"BACKEND_INTERNAL_URL=\"{envVars.GetValueOrDefault("BACKEND_INTERNAL_URL", "http://localhost:3000")}\"");
        lines.Add($"DATABASE_URL=\"{envVars.GetValueOrDefault("DATABASE_URL", "postgresql://postiz-user:postiz-password@localhost:5432/postiz-db-local")}\"");
        lines.Add($"REDIS_URL=\"{envVars.GetValueOrDefault("REDIS_URL", "redis://localhost:6379")}\"");
        lines.Add($"STORAGE_PROVIDER=\"{envVars.GetValueOrDefault("STORAGE_PROVIDER", "local")}\"");
        lines.Add($"API_LIMIT={envVars.GetValueOrDefault("API_LIMIT", "180")} # The limit of the public API hour limit");
        lines.Add($"IS_GENERAL=\"{envVars.GetValueOrDefault("IS_GENERAL", "true")}\" # required for now");
        
        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    private static string GenerateJwtSecret()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 64)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
