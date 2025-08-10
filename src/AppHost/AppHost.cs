using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<GitcgNetCord_MainApp>("main");

builder.Build().Run();
