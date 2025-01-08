using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using mySteamGraphAPI.Context;
using mySteamGraphAPI.Dtos;
using mySteamGraphAPI.Models;
using System.ComponentModel.DataAnnotations;

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
        public async Task<Results<Ok, ProblemHttpResult>> RegisterUser(RegisterDto userData)
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
    }
}
