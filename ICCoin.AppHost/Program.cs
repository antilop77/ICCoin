var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ICCoin>("iccoin");

builder.Build().Run();
