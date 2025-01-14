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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IOptionsMonitor<BearerTokenOptions> _bearerOptions;
        private readonly TimeProvider _timeProvider;
        private readonly HttpClient client = new HttpClient();
        private readonly string steamApiUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=2621FCCA6A21E594FF85870DA3470D65";
        public AuthController(SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore,
             IOptionsMonitor<BearerTokenOptions> bearerOption, TimeProvider timeProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _bearerOptions = bearerOption;
            _timeProvider = timeProvider;
            _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        }

        [HttpPost("signUp")]
        public async Task<Results<Ok, ProblemHttpResult, NotFound<string>>> RegisterUser(RegisterDto userData)
        {

            HttpResponseMessage response= await client.GetAsync(steamApiUrl+ "&steamids="+userData.steamID);

            if (response.IsSuccessStatusCode) {
            var data= JToken.Parse(await response.Content.ReadAsStringAsync());

            if (data["response"].SelectToken("players").Count()>0)
            {
                var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
                var email = userData.Email;
                var user = new ApplicationUser();
                user.steamUserId = userData.steamID;
                await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, userData.Password);

                if (!result.Succeeded)
                {
                    return TypedResults.Problem(result.ToString());
                }

                return TypedResults.Ok();


            }
            else
            {

                return TypedResults.NotFound("Steam Id is not valid");
            }


            }
            else
            {

                return TypedResults.Problem("There was a problem");

            }


        }

        [HttpPost("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(LoginDto loginInput)
        {

            var result = await _signInManager.PasswordSignInAsync(loginInput.Email, loginInput.Password, false, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                 
                 
                return TypedResults.Empty;
            }
            else
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

        }

        [HttpPost("refresh")]
        public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> RefreshToken([FromBody] RefreshRequest refreshRequest)

        {
             
            var refreshTokenProtector = _bearerOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
            if (refreshTicket?.Properties?.ExpiresUtc is not { }
                expiresUtc ||
                _timeProvider.GetUtcNow() >= expiresUtc ||
                await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not ApplicationUser user)

            {
                return TypedResults.Challenge();
            }

            var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
            return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
}
        [Authorize(AuthenticationSchemes = "Identity.Bearer")]
        [HttpGet("userSteamId")]
        public async Task<Results<Ok<long>, ProblemHttpResult>> getSteamId()
        {

            var result=  await _userManager.GetUserAsync(User);

            if(result != null)
            {
                return TypedResults.Ok(result.steamUserId);

            }
            else
            {
               return TypedResults.Problem("no encontrado");

            }

        }


    }
}
