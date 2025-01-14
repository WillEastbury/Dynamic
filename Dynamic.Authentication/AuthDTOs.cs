namespace Dynamic.Core.Authentication
{
    /// <summary>
    /// Used to transit the authentication request from the service to the client
    /// </summary>
    
    public class TokenRequestDTO
    {
        public string? AccountId { get; set; }
        public string? AccountSecret { get; set; }
        public string? TargetApplication { get; set; }
        public string? TargetAccessScope { get; set; }
        public string? OnBehalfOf { get; set; }
    }

    public class TokenResponseDTO
    {
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Roles { get; set; }
    }
}
