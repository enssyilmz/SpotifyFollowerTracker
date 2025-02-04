var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SpotifyService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new SpotifyService(
        config["Spotify:ClientId"],
        config["Spotify:ClientSecret"]
    );
});

builder.Services.AddSingleton<SmsService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new SmsService(
        config["Twilio:AccountSid"],
        config["Twilio:AuthToken"],
        config["Twilio:PhoneNumber"]
    );
});


builder.Services.AddHostedService<FollowerTrackerService>();
builder.Services.AddControllersWithViews();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
