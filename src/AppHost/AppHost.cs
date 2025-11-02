using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var main = builder
    .AddProject<GitcgNetCord_MainApp>("main");

var useRemoteRedis = builder.Configuration
    .GetValue<bool>("AppBuilderOptions:UseRemoteRedis");
if (useRemoteRedis)
{
    var isRedisConnectionStringEmpty = string.IsNullOrWhiteSpace(
        builder.Configuration.GetConnectionString("redis")
    );
    if (!isRedisConnectionStringEmpty)
    {
        var redisConnectionString = builder
            .AddConnectionString("redis");

        main.WithReference(redisConnectionString);
    }
    else
    {
        var redis = builder
            .AddRedis("redis")
            .WithDataVolume();
        
        main.WithReference(redis).WaitFor(redis);
    }
}

var useRemotePostgres = builder.Configuration
    .GetValue<bool>("AppBuilderOptions:UseRemotePostgres");

const string postgresResourceName = "gitcgnetcorddb";
if (useRemotePostgres)
{
    var isPostgresConnectionStringEmpty = string.IsNullOrWhiteSpace(
        builder.Configuration.GetConnectionString(postgresResourceName)
    );
    if (!isPostgresConnectionStringEmpty)
    {
        var gitcgnetcorddbConnectionString = builder
            .AddConnectionString(postgresResourceName);

        main.WithReference(gitcgnetcorddbConnectionString);
    }
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

    var gitcgnetcorddb = postgres
        .AddDatabase("gitcgnetcord");

    main.WithReference(gitcgnetcorddb).WaitFor(gitcgnetcorddb);
}

builder.Build().Run();