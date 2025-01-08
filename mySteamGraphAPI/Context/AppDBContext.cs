using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mySteamGraphAPI.Models;
using System;
using System.Reflection.Metadata;

namespace mySteamGraphAPI.Context
{
    public class AppDBContext: IdentityDbContext<ApplicationUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }
   
        public DbSet<CompletedGame> completedGames { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.completedGames)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.IdentityUserId)
                .HasPrincipalKey(e => e.Id);
        }


    }
}
