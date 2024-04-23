using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = "mongodb://localhost/DoAn"; // Replace with your actual connection string
    return new MongoClient(connectionString);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/user/login"; // Đường dẫn tới trang đăng nhập
});

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Sử dụng CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=user}/{action=Login}/{id?}");

app.Run();