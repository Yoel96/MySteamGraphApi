using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using mySteamGraphAPI.Context;
using mySteamGraphAPI.Dtos;
using mySteamGraphAPI.Models;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mySteamGraphAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Bearer")]
    [Route("api/[controller]")]

    [ApiController]

    public class SteamController : ControllerBase
    {
        private readonly string steamApiKey = "";
        private readonly HttpClient client = new HttpClient();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string steamUserUrl = "https://api.steampowered.com/ISteamUser/";
        private readonly string steamUserGameUrl = "http://api.steampowered.com/IPlayerService/";

        public SteamController(UserManager<ApplicationUser> userManager)
        {

            _userManager = userManager;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> getProfileSumarie()
        {
            var userData = await _userManager.GetUserAsync(User);
            System.Diagnostics.Debug.WriteLine(userData.steamUserId);
            
            HttpResponseMessage response = await client.GetAsync(steamUserUrl +"GetPlayerSummaries/v0002/?key=" + steamApiKey + "&steamids=" + userData.steamUserId);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);

            }
            else
            {
                return BadRequest("There was a problem fetching the data");
            }

        }

        [HttpGet("games")]
        public async Task<IActionResult> getUserGames()
        {
            var userData = await _userManager.GetUserAsync(User);
            System.Diagnostics.Debug.WriteLine(userData.steamUserId);

            HttpResponseMessage response = await client.GetAsync(steamUserGameUrl + "GetOwnedGames/v0001/?key=" + steamApiKey + "&steamid=" + userData.steamUserId);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);

            }
            else
            {
                return BadRequest("There was a problem fetching the data");
            }

        }

    }
}
