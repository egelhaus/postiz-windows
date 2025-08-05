namespace Shared;

public class SocialMediaProvider
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ClientIdKey { get; set; } = string.Empty;
    public string ClientSecretKey { get; set; } = string.Empty;
    public string? AdditionalKey { get; set; }
    public string? AdditionalKeyDisplayName { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? AdditionalValue { get; set; }
}

public static class SocialMediaProviders
{
    public static List<SocialMediaProvider> GetAllProviders()
    {
        return new List<SocialMediaProvider>
        {
            new() { Name = "X", DisplayName = "X (Twitter)", ClientIdKey = "X_API_KEY", ClientSecretKey = "X_API_SECRET" },
            new() { Name = "LinkedIn", DisplayName = "LinkedIn", ClientIdKey = "LINKEDIN_CLIENT_ID", ClientSecretKey = "LINKEDIN_CLIENT_SECRET" },
            new() { Name = "Reddit", DisplayName = "Reddit", ClientIdKey = "REDDIT_CLIENT_ID", ClientSecretKey = "REDDIT_CLIENT_SECRET" },
            new() { Name = "GitHub", DisplayName = "GitHub", ClientIdKey = "GITHUB_CLIENT_ID", ClientSecretKey = "GITHUB_CLIENT_SECRET" },
            new() { Name = "Beehiiv", DisplayName = "Beehiiv", ClientIdKey = "BEEHIIVE_API_KEY", ClientSecretKey = "BEEHIIVE_PUBLICATION_ID" },
            new() { Name = "Threads", DisplayName = "Threads", ClientIdKey = "THREADS_APP_ID", ClientSecretKey = "THREADS_APP_SECRET" },
            new() { Name = "Facebook", DisplayName = "Facebook", ClientIdKey = "FACEBOOK_APP_ID", ClientSecretKey = "FACEBOOK_APP_SECRET" },
            new() { Name = "YouTube", DisplayName = "YouTube", ClientIdKey = "YOUTUBE_CLIENT_ID", ClientSecretKey = "YOUTUBE_CLIENT_SECRET" },
            new() { Name = "TikTok", DisplayName = "TikTok", ClientIdKey = "TIKTOK_CLIENT_ID", ClientSecretKey = "TIKTOK_CLIENT_SECRET" },
            new() { Name = "Pinterest", DisplayName = "Pinterest", ClientIdKey = "PINTEREST_CLIENT_ID", ClientSecretKey = "PINTEREST_CLIENT_SECRET" },
            new() { Name = "Dribbble", DisplayName = "Dribbble", ClientIdKey = "DRIBBBLE_CLIENT_ID", ClientSecretKey = "DRIBBBLE_CLIENT_SECRET" },
            new() { 
                Name = "Discord", 
                DisplayName = "Discord", 
                ClientIdKey = "DISCORD_CLIENT_ID", 
                ClientSecretKey = "DISCORD_CLIENT_SECRET",
                AdditionalKey = "DISCORD_BOT_TOKEN_ID",
                AdditionalKeyDisplayName = "Bot Token"
            },
            new() { 
                Name = "Slack", 
                DisplayName = "Slack", 
                ClientIdKey = "SLACK_ID", 
                ClientSecretKey = "SLACK_SECRET",
                AdditionalKey = "SLACK_SIGNING_SECRET",
                AdditionalKeyDisplayName = "Signing Secret"
            },
            new() { 
                Name = "Mastodon", 
                DisplayName = "Mastodon", 
                ClientIdKey = "MASTODON_CLIENT_ID", 
                ClientSecretKey = "MASTODON_CLIENT_SECRET",
                AdditionalKey = "MASTODON_URL",
                AdditionalKeyDisplayName = "Mastodon URL"
            }
        };
    }
}
