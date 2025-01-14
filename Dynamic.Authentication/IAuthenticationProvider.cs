using System.Security.Claims;

namespace Dynamic.Core.Authentication;

/// <summary>
/// Interface for an authentication provider that provides the root of all authentication (local, OAuth, etc.)
/// the IAuthenticationProvider is an interface that will be implemented by the AuthenticationProvider class
/// it is called from the webapi to implement auth functions and from the middleware layer to check auth is valid
/// it does not implement any authZ functions, only authN

/// </summary>

public interface IAuthenticationProvider
{ 
    public Task<UserAccount?> GetUserByIdAsync(string id);
    public Task<UserAccount> CreateUser(UserAccount user);
    public Task<UserAccount> UpdateUser(UserAccount user);
    public Task DeleteUser(string id);

    public Task<UserAccount?> GetUserByValidTokenAsync(string token);
    public Task<string?> GenerateAuthTokenAsync(string id, string password, string TargetApplication, string Purpose = "AuthN", string SourceApplication = "nothing");
    public Task<UserAccount> RevokeAuthTokenAsync(UserAccount acct, string token);
    public bool ValidateToken(string jwt, bool signed, Dictionary<string, object>? claimsToMatch = default, string SourceApplication = "nothing");
    // Actions on the user
    public Task<bool> ChangePasswordAsync(string id, string oldPassword, string newPassword);
    public Task<bool> ResetPasswordAsync(string id, string newPassword);
    public Task<bool> TripMFAasync(string id, string challengeString);
    public Task<bool> ApplyLoginConditions(string id, List<LoginCondition> conditions);

}
/// <summary>
/// This class is designed to contain login conditions that the user must pass to be able to connect to the service.
/// It implements a rudimentatry form of conditional access feature.
/// </summary>
public class LoginCondition
{
    public List<string> PermittedLocations { get; set; } = new List<string>();
    public List<string> AllowListIPs { get; set; } = new List<string>();
    public List<string> DenyListIPs { get; set; } = new List<string>();
    public List<string> PermittedMachineIDs { get; set; } = new List<string>();

}