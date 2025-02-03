using Dynamic.Core.Authentication;
using Dynamic.Core.Authentication.Levels;
using Dynamic.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

public static class WebApplicationExtensions
{
    public static void SetupDIModules(this IServiceCollection services)
    {
        services.AddSingleton<AuthConfigOptions>();
        services.AddSingleton<IDataStoreProvider<UserAccount>, InMemoryDataStoreProvider<UserAccount>>();
        // Allow any IStorable to be stored and indexed. 
        services.AddSingleton(typeof(IDataStoreProvider<>), typeof(InMemoryDataStoreProvider<>));
        services.AddSingleton<IAuthenticationProvider, TokenAuthenticationProvider>();
        services.AddHttpClient();
        services.AddLogging();
    }
    public static async Task UseAuthMiddleware(HttpContext context, RequestDelegate next)
    {
        // Get the 4 possible attributes for this application context we need to look at from the auth library
        TypeInfo? cti = context.GetEndpoint()?.Metadata?.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo;
        ServeAuthenticatedOnly? sao = cti?.GetCustomAttribute<ServeAuthenticatedOnly>();
        ServeAuthenticatedThisApplicationOnly? satao = cti?.GetCustomAttribute<ServeAuthenticatedThisApplicationOnly>();
        ServeAuthorizedThisApplicationRoleOnly? saar = cti?.GetCustomAttribute<ServeAuthorizedThisApplicationRoleOnly>();
        ServeAuthorizedThisApplicationSpecialClaimOnly? saasc = cti?.GetCustomAttribute<ServeAuthorizedThisApplicationSpecialClaimOnly>();

        // If any of the 4 are set then we need to check the auth token, if not then skip it and continue to the request
        if (sao.Equals(null) && satao.Equals(null) && saar.Equals(null) && saasc.Equals(null))
        {
            await next(context);
        }
        else
        {
            string? authh = context.Request.Headers.Authorization;
            if (authh == null || authh == "")
            {
                // No token specified
                context.Response.StatusCode = 401;
            }
            else
            {
                // Get the token data
                string jwt = authh.Split("bearer ")[1];
                IAuthenticationProvider tap = context.RequestServices.GetRequiredService<IAuthenticationProvider>();
                Dictionary<string, object> requiredClaims = new Dictionary<string, object>();

                // Default is equivalent ServeAuthenticatedOnly set, ignore it.
                requiredClaims["typ"] = "Authentication";
                requiredClaims["iss"] = "Root";
                requiredClaims["nbf"] = DateTime.Now;
                requiredClaims["exp"] = DateTime.Now.AddMinutes(1);


                if (satao != null)
                {
                    // ServeAuthenticatedThisApplicationOnly set, ignore it.
                    requiredClaims["aud"] = satao.TargetApplication ?? "Root";
                    requiredClaims["typ"] = "Authentication";
                }

                if (saar != null)
                {
                    // ServeAuthenticatedThisApplicationRoleOnly set, ignore it.
                    requiredClaims["aud"] = saar.TargetApplication ?? "Root";
                    requiredClaims["typ"] = "Authentication";
                    requiredClaims["roles"] = saar.TargetRoleAccess ?? "Root";
                }
                if (saasc != null)
                {
                    // ServeAuthenticatedThisApplicationSpecialClaimOnly set, ignore it.
                    requiredClaims["aud"] = saasc.TargetApplication ?? "Root";
                    requiredClaims["typ"] = "Authentication";
                    requiredClaims["roles"] = saasc.TargetRoleAccess ?? "Root";
                    requiredClaims[saasc.SpecialClaim.Key] = saasc.SpecialClaim.Value;
                }

                // Check the token
                if (tap.ValidateToken(jwt, true, requiredClaims))
                {
                    // Token is valid
                    await next(context);
                }
                else
                {
                    // Token is invalid
                    context.Response.StatusCode = 403;
                }
            }
        }
    }
    public static void SetupDataPlaneRoutes(this WebApplication app)
    {
        app.MapPost("/api/storage/{objectName}", async ([FromBody] IStorable data,[FromServices] IServiceProvider services, string objectName) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            await dsp.CreateAsync(data);
            return Results.Created($"/api/storage/{objectName}/{data.Id}", data);

        });

        app.MapGet("/api/storage/{objectName}/{id}", async (
            string objectName,
            string id,
            [FromServices] IServiceProvider services) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            var result = await dsp.ReadAsync(id);
            return result is not null ? Results.Ok(result) : Results.NotFound();

        });

        app.MapGet("/api/storage/{objectName}", async (
            string objectName,
            [FromServices] IServiceProvider services) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            var result = await dsp.ReadAllAsync();
            return Results.Ok(result.Values);

        });

        app.MapPut("/api/storage/{objectName}/{id}", async (
            [FromBody] IStorable updatedData,
            string objectName,
            string id,
            [FromServices] IServiceProvider services) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            if (!await dsp.ExistsAsync(id))
            {
                return Results.NotFound();
            }

            updatedData.Id = id;
            await dsp.UpdateAsync(updatedData);
            return Results.Ok(updatedData);

        });

        app.MapDelete("/api/storage/{objectName}/{id}", async (
            string objectName,
            string id,
            [FromServices] IServiceProvider services) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            if (!await dsp.ExistsAsync(id))
            {
                return Results.NotFound();
            }

            await dsp.DeleteAsync(id);
            return Results.NoContent();

        });

        app.MapGet("/api/storage/{objectName}/paged", async (
            string objectName,
            int skipPages,
            int pageSize,
            [FromServices] IServiceProvider services) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            var result = await dsp.ReadPagedAsync(skipPages, 1, pageSize);
            return Results.Ok(result.Values);

        });

        app.MapGet("/api/storage/{objectName}/filter", async (
            string objectName,
            [FromQuery] string filterField,
            [FromQuery] string filterValue,
            [FromServices] IServiceProvider services) =>
        {
            var providerType = typeof(IDataStoreProvider<>).MakeGenericType(Type.GetType(objectName)!);
            dynamic dsp = services.GetRequiredService(providerType);

            Func<IStorable, bool> filter = item =>
            {
                var property = item.GetType().GetProperty(filterField);
                if (property == null) return false;

                var value = property.GetValue(item)?.ToString();
                return value != null && value.Contains(filterValue, StringComparison.OrdinalIgnoreCase);
            };

            var filteredItems = await dsp.ReadAllFilteredAsync(filter);
            return Results.Ok(filteredItems.Values);

        });
    }
    public static void SetupAuthRoutes(this WebApplication app)
    {
        // Get and Return an auth token
        app.MapGet("/api/auth/token", async ([FromBody] TokenRequestDTO authdto, [FromServices] TokenAuthenticationProvider tap) =>
        {
            app.Logger.LogInformation("Token Issue Endpoint hit");
            return await tap.GenerateAuthTokenAsync(authdto.AccountId, authdto.AccountSecret, authdto.TargetApplication, "Authentication", "Root");
        })
        .WithName("GetAuthToken")
        .WithOpenApi();

        // Get a user by id
        
        app.MapGet("/api/users/{id}", [ServeAuthorizedThisApplicationRoleOnly(TargetApplication = "Root", TargetRoleAccess = "Admin")] async (string id, [FromServices] TokenAuthenticationProvider tap) =>
        {
            return tap.GetUserByIdAsync(id);

        })
        .WithName("GetUserById")
        .WithOpenApi();

        // Create a user
        app.MapPost("/api/users", [ServeAuthorizedThisApplicationRoleOnly(TargetApplication = "Root", TargetRoleAccess = "Admin")] async ([FromBody] UserAccount acct, [FromServices] TokenAuthenticationProvider tap) =>
        {
            return tap.CreateUser(acct);

        })
        .WithName("PostUser")
        .WithOpenApi();

        // Update a user
        app.MapPut("/api/users/{id}", [ServeAuthorizedThisApplicationRoleOnly(TargetApplication = "Root", TargetRoleAccess = "Admin")] async ([FromRoute] string id, [FromBody] UserAccount acct, [FromServices] TokenAuthenticationProvider tap) =>
        {
            if (acct.Id != id)
            {
                throw new InvalidDataException("Id mismatch");
            }
            else
            {
                await tap.UpdateUser(acct);
            }

        })
        .WithName("PutUser")
        .WithOpenApi();

        // Delete a user
        app.MapDelete("/api/users/{id}", [ServeAuthorizedThisApplicationRoleOnly(TargetApplication = "Root", TargetRoleAccess = "Admin")] async ([FromRoute] string id,[FromServices] TokenAuthenticationProvider tap) =>
        {
            await tap.DeleteUser(id);
            
        })
        .WithName("DeleteUser")
        .WithOpenApi();
    }
}