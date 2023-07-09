using Auction.API;
using Auction.API.Hubs;
using Auction.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddSingleton<IDictionary<string, ConnectionParameter>>(options => new Dictionary<string, ConnectionParameter>());

builder.Services.AddHostedService<ApiRabbitMqListener>();
builder.Services.AddSingleton<IAuctionEventHandler, AuctionEventHandler>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Auction.API_";
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(builder =>
{
    builder.WithOrigins("https://localhost:7264")
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
});

app.UseAuthorization();

app.MapControllers();

app.MapHub<AuctionHub>("auction/hub");

app.Run();
