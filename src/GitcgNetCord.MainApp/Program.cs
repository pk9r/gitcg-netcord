using Azure.Identity;
using GitcgNetCord.MainApp.Components;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.SemanticKernel;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var azureVaultUri = builder.Configuration["AzureVaultUri"];
if (!string.IsNullOrEmpty(azureVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        vaultUri: new Uri(azureVaultUri),
        credential: new DefaultAzureCredential()
    );
}

builder.AddRedisDistributedCache("redis");
builder.Services.AddHybridCache();

builder.AddNpgsqlDbContext<AppDbContext>("gitcgnetcorddb");

builder.Services.AddOpenAIChatCompletion(
    modelId: builder.Configuration["OpenAiOptions:ModelId"]!,
    apiKey: builder.Configuration["OpenAiOptions:ApiKey"]!,
    endpoint: new Uri(builder.Configuration["OpenAiOptions:EndpointUrl"]!)
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

app.Run();
