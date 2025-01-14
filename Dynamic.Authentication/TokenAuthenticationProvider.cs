using Dynamic.Core.Data;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace Dynamic.Core.Authentication
{
    public class TokenAuthenticationProvider : IAuthenticationProvider
    {
        private HMACSHA256 Sha { get; set; }
        private IDataStoreProvider<UserAccount> _UserAccountStoreProvider;

        public TokenAuthenticationProvider(IDataStoreProvider<UserAccount> provider, AuthConfigOptions secretKey) 
        {
            _UserAccountStoreProvider = provider;
            Sha = new HMACSHA256(secretKey.seckey);
        }
        
        private string GenerateToken(Dictionary<string, object> claims)
        {
            // Step 1: Create Header
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };

            // Step 2: Create Payload (Claims)
            var payload = claims;

            // Step 3: Base64Url encode Header and Payload
            var base64Header = CommonUtilities.Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header)));
            var base64Payload = CommonUtilities.Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));

            // Step 4: Create the string to be signed: "header.payload"
            var stringToSign = $"{base64Header}.{base64Payload}";

            // Step 5: Sign the string with HMAC SHA-256 using the secret key
            var signature = CommonUtilities.Base64UrlEncode(Sha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            // Step 6: Create the final JWT token by combining the encoded Header, Payload, and Signature
            var jwt = $"{stringToSign}.{signature}";

            return jwt;
        }
        
        public bool ValidateToken(string jwt, bool signed, Dictionary<string, object>? claimsToMatch = default, string SourceApplication = "nothing")
        {

            // Step 1: Split the JWT into Header, Payload, and Signature
            var parts = jwt.Split('.');
            var base64Header = parts[0];
            var base64Payload = parts[1];
            var base64Signature = parts[2];

            // Step 2: Decode the Header and Payload
            var header = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(CommonUtilities.Base64UrlDecode(base64Header)));
            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(CommonUtilities.Base64UrlDecode(base64Payload)));

            // Step 3: Create the string to be signed: "header.payload"
            var stringToSign = $"{base64Header}.{base64Payload}";

            // Step 4: Sign the string with HMAC SHA-256 using the secret key
            var signature = CommonUtilities.Base64UrlEncode(Sha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            // Step 5: Compare the signature in the JWT with the signature we just computed
            if (signed && base64Signature != signature) return false;

            if (claimsToMatch != null)
            {
                // Check if all keys in claimsToMatch exist in payload
                if (claimsToMatch.All(k => payload.ContainsKey(k.Key)))
                {
                    // All keys are present, now optionally check if values match
                    bool valuesMatch = claimsToMatch.All(k => payload[k.Key].Equals(k.Value));

                    if (valuesMatch)
                    {
                        // All claims match
                        return true;
                    }
                    else
                    {
                        // Some claims don't match
                        return false;
                    }
                }
                else
                {
                    // Some keys are missing from the payload
                    return false;
                }
            }
            return true;
        }
        
        public TokenDTO ExtractJWT(Dictionary<string, object> claims)
        {
            return new TokenDTO(claims);
        }
        public async Task<bool> ResetPasswordAsync(string id, string newPassword)
        {
            if (newPassword == null) return false;

            UserAccount? me = await _UserAccountStoreProvider.ReadAsync(id);

            if (me == null) return false;

            if (me.SetPasswordWithChecks(newPassword))
            {
                me.FailedLoginAttempts = 0;
                me.LastSuccessfulLogin = DateTime.Now;
                return true;
            }

            return false;

        }
        public async Task<bool> ChangePasswordAsync(string id, string oldPassword, string newPassword)
        {
            if (oldPassword == null) return false;

            return await ResetPasswordAsync(id, newPassword);
            
        }

        public async Task<int> CheckFailedLogins(string id)
        {
            if (id == null) return 99999;
            UserAccount? me = await _UserAccountStoreProvider.ReadAsync(id);
            if (me == null) return 99999;
            return me.FailedLoginAttempts;
        }

        public async Task<UserAccount> CreateUser(UserAccount user)
        {
            await _UserAccountStoreProvider.CreateAsync(user);
            return user;
        }

        public async Task DeleteUser(string id)
        {
            await _UserAccountStoreProvider.DeleteAsync(id);
        }

        public async Task<string?> GenerateAuthTokenAsync(string id, string password, string TargetApplication = "nothing", string Purpose = "AuthN", string SourceApplication = "nothing")
        {
            UserAccount? me = await CheckNullAndRetrieveUserById(id);
            if (me == null) return null;

            // Hash the password and compare it to the current user
            if (!me.VerifyAuth(password))
            {
                return null;
            }

            // Store the account
            await _UserAccountStoreProvider.UpdateAsync(me);

            // all good, generate a token
            return GenerateToken(new()
            {
                { "iss", SourceApplication },
                { "aud", TargetApplication },
                { "uid", me.Id },
                { "exp", DateTime.Now.AddHours(1) },
                { "nbf", DateTime.Now.AddMinutes(-5) },
                { "lastLogin", me.LastSuccessfulLogin },
                { "failedLogins", me.FailedLoginAttempts },
                { "typ", me.principalType },
                { "name", me.FullName },
                { "purpose", Purpose },
                { "roles", string.Join(",", me.roles)}
            });
        }

        private async Task<UserAccount?> CheckNullAndRetrieveUserById(string id)
        {
            if (id == null || id == "") return null;
            UserAccount? me = await _UserAccountStoreProvider.ReadAsync(id);
            return me;
        }

        public Task<UserAccount?> GetUserByIdAsync(string id)
        {
            return CheckNullAndRetrieveUserById(id);
        }

        public async Task<UserAccount?> GetUserByValidTokenAsync(string token)
        {
            // Decode the token first and fetch the user account
            if(!ValidateToken(token, true)) return null;
            TokenDTO tdto = new TokenDTO(token);
            return await CheckNullAndRetrieveUserById(tdto.UserId);
        }

        public Task<UserAccount> RevokeAuthTokenAsync(UserAccount acct, string token)
        {
            acct.revokedTokens.Add(token);
            return Task.FromResult(acct);

        }

        public Task<bool> TripMFAasync(string id, string challengeString)
        {
            // Todo: NotImplementedException MFA Algo
            throw new NotImplementedException();
        }

        public async Task<UserAccount> UpdateUser(UserAccount user)
        {
            await _UserAccountStoreProvider.UpdateAsync(user);
            return user;
        }


    }
}
