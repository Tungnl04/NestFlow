using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Application.Services;
using NestFlow.Models;
using NestFlow.Hubs;
using Net.payOS;

var builder = WebApplication.CreateBuilder(args);
var currentConn = builder.Configuration.GetConnectionString("Default");
Console.WriteLine($"DEBUG: Chuỗi kết nối thực tế đang chạy là: {currentConn}");

// Add services Razor Pages &  to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Add SignalR

// Add db context service
builder.Services.AddDbContext<NestFlowSystemContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // Session timeout 24 giờ
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IWalletService, WalletService>();

// Configure PayOS
PayOS payOS = new PayOS(
    builder.Configuration["PayOS:ClientId"] ?? throw new Exception("PayOS ClientId not found"),
    builder.Configuration["PayOS:ApiKey"] ?? throw new Exception("PayOS ApiKey not found"),
    builder.Configuration["PayOS:ChecksumKey"] ?? throw new Exception("PayOS ChecksumKey not found")
);
builder.Services.AddSingleton(payOS);
builder.Services.AddHttpContextAccessor();

// Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Note: For SignalR with Credentials, AllowAnyOrigin cannot be used.
        // If enabling credentials, you must specify origins: .WithOrigins("http://localhost:3000")
        // For this phase with "AllowAnyOrigin", stick to standard CORS, but SignalR JS client might need specific handling if cookies are involved.
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/Home/Index");
        return;
    }
    await next();
});


app.Run();
