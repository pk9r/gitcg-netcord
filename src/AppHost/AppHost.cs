using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var main = builder.AddProject<GitcgNetCord_MainApp>("main");

main.WithReference(redis).WaitFor(redis);

var useRemotePostgres = builder.Configuration
    .GetValue<bool>("AppBuilderOptions:UseRemotePostgres");

var postgresResourceName = "gitcgnetcorddb";
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