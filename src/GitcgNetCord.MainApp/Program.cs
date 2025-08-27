using GitcgNetCord.MainApp.Components;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisDistributedCache("redis");
builder.Services.AddHybridCache();

builder.AddNpgsqlDbContext<AppDbContext>("gitcgnetcorddb");

builder.Services.AddHttpClient<IChatCompletionService>()
    .ConfigureHttpClient(o => o.Timeout = Timeout.InfiniteTimeSpan);

builder.Services.AddOpenAIChatCompletion(
    modelId: builder.Configuration["DuelAssistantOptions:ModelId"]!,
    apiKey: builder.Configuration["DuelAssistantOptions:ApiKey"]!,
    endpoint: new Uri(builder.Configuration["DuelAssistantOptions:EndpointUrl"]!)
);

builder.Services.AddNetCordServices();
builder.Services.AddHoyolabServices();
builder.Services.AddAppServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.AddSlashCommand(
    name: "ping",
    description: "Ping!",
    handler: () => "Pong!"
);

app.AddDiscordBotModules();

app.MapGet(
    pattern: "/api/",
    handler: () => "Hello World!"
);

app.UseGatewayHandlers();

app.Run();
