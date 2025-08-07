using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Gitcg_NetCord_MainApp>("main");

builder.Build().Run();
