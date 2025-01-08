using Microsoft.AspNetCore.Identity;

namespace mySteamGraphAPI.Models
{
    public class CompletedGame
    {
      
        public long Id { get; set; }
        public string GameName { get; set; }
        public int SteamId { get; set; }
        public string? Image { get; set; }
        public ApplicationUser User { get; set; }

        public string IdentityUserId { get; set; }

  
    }
}
