using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace mySteamGraphAPI.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public long steamUserId { get; set; }
        public ICollection<CompletedGame> completedGames { get; }
    }
}
