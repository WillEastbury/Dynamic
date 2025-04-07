var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.SetupDIModulesInMemory(); // Setup the DI modules for in-memory data store

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.Use(WebApplicationExtensions.UseAuthMiddleware);
app.SetupDataPlaneRoutes();
app.Run();