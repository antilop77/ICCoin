var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ICCoinAPI>("iccoin");

builder.Build().Run();
