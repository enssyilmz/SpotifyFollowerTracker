using Microsoft.Extensions.Caching.Memory;

public class FollowerTrackerService : BackgroundService
{
    private readonly SpotifyService _spotifyService;
    private readonly SmsService _smsService; 
    private readonly IMemoryCache _cache;
    public FollowerTrackerService(SpotifyService spotifyService, SmsService smsService, IMemoryCache cache)
    {
        _spotifyService = spotifyService;
        _smsService = smsService;
        _cache = cache;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool isLoggedOut = _cache.TryGetValue("IsLoggedOut", out bool loggedOut) && loggedOut;
            if (isLoggedOut)
            {
                Console.WriteLine("Kullanıcı logout oldu; takipçi kontrolü durduruldu.");
                _cache.Set("FollowerCount", 0, TimeSpan.FromHours(1));
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }
            try
            {
                var accessToken = _cache.Get<string>("AccessToken");
                var refreshToken = _cache.Get<string>("RefreshToken");
                var expiresAt = _cache.Get<DateTime?>("ExpiresAt");

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || expiresAt == null)
                {
                    Console.WriteLine("⚠ Henüz yetkilendirme yapılmadı. Takipçi kontrolü bekletiliyor.");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }
                if (expiresAt <= DateTime.UtcNow)
                {
                    var newToken = await _spotifyService.RefreshAccessTokenAsync(refreshToken);
                    _cache.Set("AccessToken", newToken.AccessToken, TimeSpan.FromHours(1));
                    _cache.Set("RefreshToken", newToken.RefreshToken, TimeSpan.FromDays(30));
                    _cache.Set("ExpiresAt", DateTime.UtcNow.AddSeconds(newToken.ExpiresIn), TimeSpan.FromHours(1));
                    accessToken = newToken.AccessToken;
                }

                var currentFollowerCount = await _spotifyService.GetFollowerCountAsync(accessToken);
                Console.WriteLine($"🔄 Güncel takipçi sayısı: {currentFollowerCount}");
                int cachedFollowerCount;
                if (!_cache.TryGetValue("FollowerCount", out cachedFollowerCount))
                {
                    cachedFollowerCount = currentFollowerCount;
                    _cache.Set("FollowerCount", currentFollowerCount, TimeSpan.FromHours(1));
                }
                Console.WriteLine($"🔄 Önceki (cache'deki) takipçi sayısı: {cachedFollowerCount}");

                if (currentFollowerCount != cachedFollowerCount)
                {
                    Console.WriteLine($"🔔 Takipçi değişikliği tespit edildi! Eski: {cachedFollowerCount}, Yeni: {currentFollowerCount}");
                    var message = currentFollowerCount > cachedFollowerCount
                        ? "Takipçi sayınız arttı!"
                        : "Takipçi sayınız azaldı!";
                    string phoneNumber = "+905389137670"; 

                    Console.WriteLine("📩 SMS gönderiliyor...");
                    try
                    {
                        await _smsService.SendSmsAsync(phoneNumber, message);
                        Console.WriteLine("✅ SMS gönderildi!");
                    }
                    catch (Exception smsEx)
                    {
                        Console.WriteLine($"❌ SMS gönderme hatası: {smsEx.Message}");
                    }
                    _cache.Set("FollowerCount", currentFollowerCount, TimeSpan.FromHours(1));
                }
                else
                {
                    Console.WriteLine("🔄 Takipçi sayısında değişiklik yok.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
