using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

public class AuthController : Controller
{
    private readonly SpotifyService _spotifyService;
    private readonly IMemoryCache _cache;

    public AuthController(SpotifyService spotifyService, IMemoryCache cache)
    {
        _spotifyService = spotifyService;
        _cache = cache;
    }

    public async Task<IActionResult> Callback(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            ViewBag.Error = "Authorization code alınamadı.";
            ViewBag.ShowLoginButton = true;
            return View();
        }

        var redirectUri = "https://localhost:7036/auth/callback";
        var tokenResponse = await _spotifyService.GetAccessTokenAsync(code, redirectUri);

        Console.WriteLine($"Token Response: {tokenResponse?.AccessToken}");

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            ViewBag.Error = "Access token alınamadı.";
            ViewBag.ShowLoginButton = true;
            return View();
        }
        _cache.Set("AccessToken", tokenResponse.AccessToken, TimeSpan.FromHours(1));
        _cache.Set("RefreshToken", tokenResponse.RefreshToken, TimeSpan.FromDays(30));
        _cache.Set("ExpiresAt", DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn), TimeSpan.FromHours(1));
        _cache.Remove("IsLoggedOut");

        var followerCount = await _spotifyService.GetFollowerCountAsync(tokenResponse.AccessToken);
        _cache.Set("FollowerCount", followerCount, TimeSpan.FromHours(1));

        return RedirectToAction("Index", "Home");
    }
}
