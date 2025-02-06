using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly SpotifyService _spotifyService;
    private readonly IMemoryCache _memoryCache;

    public HomeController(SpotifyService spotifyService, IMemoryCache memoryCache)
    {
        _spotifyService = spotifyService;
        _memoryCache = memoryCache;
    }

    public IActionResult Logout()
    {
        _memoryCache.Remove("AccessToken");
        _memoryCache.Remove("RefreshToken");
        _memoryCache.Remove("ExpiresAt");
        return RedirectToAction("Index");
    }

    public IActionResult Login()
    {
        var clientId = "84aab6d08ff54d38b5a01d7007ea7718";
        var redirectUri = "https://spotifyfollowertracker.up.railway.app/auth/callback";
        var scopes = "user-follow-read";
        var authUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope={scopes}";
        return Redirect(authUrl);
    }

    public async Task<IActionResult> Index()
    {
        if (!_memoryCache.TryGetValue("AccessToken", out string accessToken))
        {
            ViewBag.Error = "AccessToken bulunamadı. Lütfen giriş yapın.";
            return View();
        }

        var followerCount = await _spotifyService.GetFollowerCountAsync(accessToken);
        var userInfo = await _spotifyService.GetUserInfoAsync(accessToken);

        ViewBag.AccessToken = accessToken;
        ViewBag.FollowerCount = followerCount;
        ViewBag.DisplayName = userInfo.DisplayName;
        ViewBag.ProfileImageUrl = userInfo.ProfileImageUrl;
        return View("Dashboard");
    }

    public async Task<IActionResult> GetFollowerCount()
    {
        if (!_memoryCache.TryGetValue("AccessToken", out string accessToken))
        {
            return Json(0);
        }

        var followerCount = await _spotifyService.GetFollowerCountAsync(accessToken);
        return Json(followerCount);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}