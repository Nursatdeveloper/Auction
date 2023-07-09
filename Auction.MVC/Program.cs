using Auction.MVC;
using Auction.MVC.Jobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System;
using Auction.MVC.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", options => options
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Forbidden/";
        options.LoginPath = "/User/Login";
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("ModeratorOnly", policy => policy.RequireClaim("Moderator"));
    options.AddPolicy("AuthenticatedUsersOnly", policy => policy.RequireClaim("Iin"));
});

var connectionString = builder.Configuration.GetConnectionString("DbConnection");

builder.Services.AddDbContext<AuctionDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<IHostedService, JobConsumerService>(s => 
    new JobConsumerService(new AuctionDbContext(null, connectionString), new IJob[] {
        new StartAuctionJob(new RabbitMqService()),
        new StartAcceptingParticipantsJob()
    })
);
builder.Services.AddHostedService<WebRabbitMqListener>();

builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
//builder.Services.AddScoped<IAuctionEventHandler, AuctionEventHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
