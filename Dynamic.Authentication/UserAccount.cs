using Dynamic.Core;
using Dynamic.Core.Data;
namespace Dynamic.Core.Authentication
{
    public class UserAccount : IStorable
    {
        public string Id { get => UserName; set => UserName = value; }
        public required string UserName { get; set; }
        public bool Locked { get; set; } = false;
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddMonths(12);
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public required string FullName { get; set; }
        public PrincipalType principalType { get; set; } = PrincipalType.User;

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime LastSuccessfulLogin { get; set; } = DateTime.MinValue;
        public byte[] Salt { get; set; } = Guid.NewGuid().ToByteArray();
        public List<byte[]> PasswordHashes { get; set; } = new();
        public byte[] currentHash() => PasswordHashes[PasswordHashes.Count - 1];
        public string? MFAEmail { get; set; }
        public string? MFAPhoneNumber { get; set; } 
        public List<string> revokedTokens { get; set; } = new();
        public string? lastRefreshToken { get; set; } = null;
        public List<string> roles { get; set; } = new List<string>() {"User"};
        public DateTime LoginTimeRangeStart { get; set; }
        public int LoginTimeRangeHours { get; set; } = 8;
        public List<string> KnownLoginLocations { get; set; } 
        public List<string> KnownLoginIPs { get; set; }
        
        public bool SetPasswordWithChecks(string newPassword) {

            // Firstly check the complexity rules of the password
            // 1. Must be over 8 chars
            if(newPassword.Length < 8) return false;
            // 2. Must contain at least one of each of the following: digit, upper case, lower case, punctuation
            if(!newPassword.Any(char.IsDigit) && !newPassword.Any(char.IsUpper) && !newPassword.Any(char.IsPunctuation) && !newPassword.Any(char.IsLower)) return false; 

            // 3. Known words must not be 2 distances of change away from an item in the banned password list
            if (BannedPasswordList.Passwords.Any(e => CommonUtilities.IsStepsOfChangeAway(e, newPassword,2))) return false;
          
            // 4. Now hash it with the user salt
            byte[] newHash = CommonUtilities.GetSHA256Hash(newPassword, Salt);
            
            // 5. Must not have been used before (i.e. already in the PasswordHashes list
            if(PasswordHashes.Contains(newHash)) return false;

            return true;

        }
        public bool VerifyAuth(string password) {

            bool OKYn = true;
            // Firstly do validity checks
            if (Locked) OKYn = false;
            if(DateTime.Now > ValidUntil) OKYn = false;
            if(DateTime.Now.AddMinutes(-5) < ValidFrom) OKYn = false;
            if(FailedLoginAttempts >= 3) OKYn = false;
            if (PasswordHashes.Last() != CommonUtilities.GetSHA256Hash(password, Salt)) OKYn = false;

            // If we are OK, reset the failed login attempts
            if (OKYn) { 
                FailedLoginAttempts = 0; 
                LastSuccessfulLogin = DateTime.Now; 
            }
            else FailedLoginAttempts++;

            return OKYn;
        }
    }
}