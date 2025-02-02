using Newtonsoft.Json;

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}

public class SpotifyUserInfo
{
    public string DisplayName { get; set; }
    public string ProfileImageUrl { get; set; }
}