using System.Text;
using Newtonsoft.Json;

public class SpotifyService
{
    private readonly string _clientId;
    private readonly string _clientSecret;

    public SpotifyService(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public async Task<TokenResponse> GetAccessTokenAsync(string code, string redirectUri)
    {
        using (var client = new HttpClient())
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
            });

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Spotify API hatası: {response.StatusCode}, {result}");
            }

            return JsonConvert.DeserializeObject<TokenResponse>(result);
        }
    }

    public async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        using (var client = new HttpClient())
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            });

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Spotify API hatası: {response.StatusCode}, {result}");
            }

            return JsonConvert.DeserializeObject<TokenResponse>(result);
        }
    }

    public async Task<int> GetFollowerCountAsync(string accessToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://api.spotify.com/v1/me");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Spotify API hatası: {response.StatusCode}, {result}");
            }

            var userInfo = JsonConvert.DeserializeObject<dynamic>(result);
            return userInfo.followers.total;
        }
    }
    public async Task<SpotifyUserInfo> GetUserInfoAsync(string accessToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://api.spotify.com/v1/me");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Spotify API hatası: {response.StatusCode}, {result}");
            }

            var userInfo = JsonConvert.DeserializeObject<dynamic>(result);
            return new SpotifyUserInfo
            {
                DisplayName = userInfo.display_name,
                ProfileImageUrl = userInfo.images.Count > 0 ? (string)userInfo.images[0].url : null
            };
        }

    }
}