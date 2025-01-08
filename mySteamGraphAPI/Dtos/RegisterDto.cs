namespace mySteamGraphAPI.Dtos
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public long steamID { get; set; }
    }
}
