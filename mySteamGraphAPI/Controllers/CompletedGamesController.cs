using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mySteamGraphAPI.Context;
using mySteamGraphAPI.Dtos;
using mySteamGraphAPI.Models;

namespace mySteamGraphAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class CompletedGamesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public CompletedGamesController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/CompletedGames
        [HttpGet]
        public async Task<IActionResult> GetUsercompletedGames()
        {
         

            return Ok(await _context.completedGames.Where(c => c.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).Select(c => new
            {
                c.Id,
                c.GameName,
                c.Image,
                c.SteamId
            }).ToListAsync());
     
        }

        // GET: api/CompletedGames/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompletedGame>> GetCompletedGame(long id)
        {

            var completedGame = await _context.completedGames.FindAsync(id);

            if (completedGame == null)
            {
                return NotFound();
            }

            return completedGame;
        }

 
        [HttpPost]
        public async Task<IActionResult> PostCompletedGame(CompletedGameDto completedGameData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            CompletedGame completedGame = new CompletedGame( );
            completedGame.Image = completedGameData.Image;
            completedGame.GameName = completedGameData.Name;
            completedGame.SteamId = completedGameData.steamGameId;
            completedGame.IdentityUserId = userId;
            _context.completedGames.Add(completedGame);
            await _context.SaveChangesAsync();
            return Ok("Created correctly");
        }

   

        private bool CompletedGameExists(long id)
        {
            return _context.completedGames.Any(e => e.Id == id);
        }
    }
}
