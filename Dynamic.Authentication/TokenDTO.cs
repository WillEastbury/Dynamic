using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Dynamic.Core.Authentication
{
    public class TokenDTO
    {
        public string Issuer { get; set; }  // "iss"
        public string Audience { get; set; }  // "aud"
        public string UserId { get; set; }  // "uid"
        public DateTime Expiration { get; set; }  // "exp"
        public DateTime NotBefore { get; set; }  // "nbf"
        public DateTime LastLogin { get; set; }  // "lastLogin"
        public int FailedLogins { get; set; }  // "failedLogins"
        public string PrincipalType { get; set; }  // "typ"
        public string FullName { get; set; }  // "name"
        public string Purpose { get; set; }  // "purpose"
        public string Roles { get; set; }  // "roles"
        public List<string> OBOUserIds { get; set; }
        public List<string> ListRoles() => Roles.Split(',').ToList();

        public TokenDTO(string token)
        {
            var jsonDocument = JsonDocument.Parse(token);
            var root = jsonDocument.RootElement;

            Issuer = root.GetProperty("iss").GetString();
            Audience = root.GetProperty("aud").GetString();
            UserId = root.GetProperty("uid").GetString();
            Expiration = root.GetProperty("exp").GetDateTime();
            NotBefore = root.GetProperty("nbf").GetDateTime();
            LastLogin = root.GetProperty("lastLogin").GetDateTime();
            FailedLogins = root.GetProperty("failedLogins").GetInt32();
            PrincipalType = root.GetProperty("typ").GetString();
            FullName = root.GetProperty("name").GetString();
            Purpose = root.GetProperty("purpose").GetString();
            Roles = root.GetProperty("roles").GetString();
        }

        public TokenDTO(Dictionary<string, object>? payload) { }

        public TokenDTO(string issuer, string audience, string userId, DateTime expiration, DateTime notBefore, DateTime lastLogin, int failedLogins, string principalType, string fullName, string purpose, string roles)
        {
            Issuer = issuer;
            Audience = audience;
            UserId = userId;
            Expiration = expiration;
            NotBefore = notBefore;
            LastLogin = lastLogin;
            FailedLogins = failedLogins;
            PrincipalType = principalType;
            FullName = fullName;
            Purpose = purpose;
            Roles = roles;
        }
    }
}
