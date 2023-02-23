using Microsoft.AspNetCore.HttpOverrides;
using PlayAudioUsingMediaStreams.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// For load balancers, reverse proxies, and tunnels like ngrok and VS dev tunnels
// Follow guidance to secure here: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddSpeechClient();
builder.Services.AddTransient<SoundService>();
builder.Services.AddTransient<SpeechRecognitionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseForwardedHeaders();

app.MapControllers();

app.UseWebSockets();

app.Run();