namespace SIRPSI.DTOs.User
{
    public class ConsultarUsuarios
    {
        public string? TypeDocument { get; set; }
        public string? Document { get; set; }
        public string? IdCountry { get; set; }
        public string? IdCompany { get; set; }
        public string? IdRol { get; set; }
        public string? Names { get; set; }
        public string? Surnames { get; set; }
        public string? Status { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? UserRegistration { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? UserModify { get; set; }
        public string? Discriminator { get; set; }
    }
}
