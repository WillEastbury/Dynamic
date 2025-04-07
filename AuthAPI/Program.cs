using Dynamic.Core.Authentication;
using Dynamic.Core.Data;
using Dynamic.Core.Data.Interfaces;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.SetupDIModulesInMemory(); // Setup the DI modules for in-memory data store

        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.Use(WebApplicationExtensions.UseAuthMiddleware);
        app.SetupAuthRoutes();
        app.Run();
    }
}