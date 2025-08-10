using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var main = builder.AddProject<GitcgNetCord_MainApp>("main");

main.WithReference(redis).WaitFor(redis);

var useRemotePostgres = builder.Configuration
    .GetValue<bool>("AppBuilderOptions:UseRemotePostgres");

if (useRemotePostgres)
{
    var postgresConnectionString = builder
        .AddConnectionString("gitcgnetcorddb");

    main.WithReference(postgresConnectionString);
}
else
{
    var postgresPassword = builder.AddParameter(
        name: "postgresql-password",
        value: "PGPASSWORD"
    );

    var postgres = builder.AddPostgres(
        name: "postgresql",
        password: postgresPassword
    );
    
    postgres
        .WithDataVolume()
        .WithPgAdmin();
    
    main.WithReference(postgres).WaitFor(postgres);
}

builder.Build().Run();