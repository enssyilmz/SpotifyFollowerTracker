using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FollowerTrackerService : BackgroundService
{
    private readonly SpotifyService _spotifyService;
    private readonly SmsService _smsService;
    private readonly IMemoryCache _cache;
    private readonly string _followerCountFile = "follower_count.txt";

    public FollowerTrackerService(SpotifyService spotifyService, SmsService smsService, IMemoryCache cache)
    {
        _spotifyService = spotifyService;
        _smsService = smsService;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int previousFollowerCount = LoadFollowerCountFromFile();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var accessToken = _cache.Get<string>("AccessToken");
                var refreshToken = _cache.Get<string>("RefreshToken");

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    Console.WriteLine("⚠ Henüz yetkilendirme yapılmadı. Takipçi kontrolü bekletiliyor.");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                var currentFollowerCount = await _spotifyService.GetFollowerCountAsync(accessToken);
                Console.WriteLine($"🔄 Güncel takipçi sayısı: {currentFollowerCount}");

                if (currentFollowerCount == 0 && previousFollowerCount > 0)
                {
                    Console.WriteLine("⚠ API hatası olabilir. Önceki takipçi sayısı korunuyor.");
                    currentFollowerCount = previousFollowerCount;
                }

                Console.WriteLine($"📊 Önceki takipçi sayısı: {previousFollowerCount}");

                if (!_cache.TryGetValue("FollowerCount", out int cachedFollowerCount))
                {
                    cachedFollowerCount = previousFollowerCount;
                }

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
                }
                else
                {
                    Console.WriteLine("✅ Takipçi sayısında değişiklik yok.");
                }

                previousFollowerCount = currentFollowerCount;
                SaveFollowerCountToFile(currentFollowerCount);
                _cache.Set("FollowerCount", currentFollowerCount, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }


    private int LoadFollowerCountFromFile()
    {
        if (File.Exists(_followerCountFile))
        {
            if (int.TryParse(File.ReadAllText(_followerCountFile), out int count))
            {
                return count;
            }
        }
        return 0;
    }

    private void SaveFollowerCountToFile(int count)
    {
        File.WriteAllText(_followerCountFile, count.ToString());
    }
}
