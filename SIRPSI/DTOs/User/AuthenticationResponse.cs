namespace SIRPSI.DTOs.User
{
    //Response login user
    public class AuthenticationResponse
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
