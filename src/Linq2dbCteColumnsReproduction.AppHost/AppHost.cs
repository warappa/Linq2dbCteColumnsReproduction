var builder = DistributedApplication.CreateBuilder(args);

// SQL Server container (Aspire manages docker automatically)
var password = builder.AddParameter("sql-password", "SQL-password12!!", secret: true);

var sql = builder.AddSqlServer("sql", password)
    .WithContainerName("sql")
    .WithHostPort(1433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    ;

var apiService = builder.AddProject<Projects.Linq2dbCteColumnsReproduction_ApiService>("apiservice")
    //.WithHttpHealthCheck("/health")
    // .WithExternalHttpEndpoints()
    .WithReference(sql)
    .WaitFor(sql)
    ;

// builder.AddProject<Projects.Linq2dbCteColumnsReproduction_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithHttpHealthCheck("/health")
//     .WithReference(apiService)
//    .WaitFor(apiService);

builder.Build().Run();
