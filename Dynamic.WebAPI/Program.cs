var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.SetupDIModules();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.Use(WebApplicationExtensions.UseAuthMiddleware);
app.SetupAuthRoutes();

app.Run();