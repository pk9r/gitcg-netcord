using Azure.Identity;
using GitcgNetCord.MainApp.Components;
using GitcgNetCord.MainApp.Configuration;
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

var openAiOptions = builder.Configuration
    .GetSection(OpenAiOptions.ConfigurationSectionName)
    .Get<OpenAiOptions>();

if (openAiOptions is { Enabled: true }
    && !string.IsNullOrWhiteSpace(openAiOptions.ModelId)
    && !string.IsNullOrWhiteSpace(openAiOptions.ApiKey))
{
    if (Uri.TryCreate(openAiOptions.EndpointUrl, UriKind.Absolute, out var endpointUri))
    {
        builder.Services.AddOpenAIChatCompletion(
            modelId: openAiOptions.ModelId,
            apiKey: openAiOptions.ApiKey,
            endpoint: endpointUri
        );
    }
    else
    {
        builder.Services.AddOpenAIChatCompletion(
            modelId: openAiOptions.ModelId,
            apiKey: openAiOptions.ApiKey
        );
    }
}

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
