using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vue.TodoApp
{
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly TodoAppContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ILogger<TokenController> logger;
        private readonly AppSettings _appSettings;

        public TokenController(TodoAppContext context, JwtTokenGenerator tokenGenerator, IOptions<AppSettings> appSettings, ILogger<TokenController> logger)
        {
            this._context = context ?? throw new System.ArgumentNullException(nameof(context));
            this._tokenGenerator = tokenGenerator ?? throw new System.ArgumentNullException(nameof(tokenGenerator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._appSettings = appSettings != null ? appSettings.Value : throw new System.ArgumentNullException(nameof(appSettings));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] TokenPostRequest request)
        {
            var issuer = base.Request.Headers["iss"];
            var audience = base.Request.Headers["aud"];

            var foundUser = await this._context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.User && u.Password == request.Password);

            if (foundUser is null)
                return BadRequest(new { ErrorMessage = "Invalid user os password" });

            try
            {
                var generatedToken = _tokenGenerator.Generate(foundUser, issuer, audience);

                return Ok(new
                {
                    name = foundUser.Name,
                    email = foundUser.Email,
                    expiresIn = generatedToken.ExpiresIn,
                    token = generatedToken.Token
                });
            }
            catch (TokenGenerationException ex)
            {
                return BadRequest(new { ErrorMessage = ex.Message });
            }
        }

        [HttpPost]
        [Route("facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> Facebook([FromServices] IHttpClientFactory httpClientsFactory, [FromBody] FacebookTokenPostRequest request)
        {
            var issuer = base.Request.Headers["iss"];
            var audience = base.Request.Headers["aud"];

            if(request is null || string.IsNullOrEmpty(request.Code))
                return BadRequest(new { ErrorMessage = "Invalid request or Facebook code" });

            try
            {
                this.logger.LogError("FACEBOOK CODE: " + request.Code);
                var httpClient = httpClientsFactory.CreateClient("FacebookClient");
                
                this.logger.LogError($"Validate Code: https://graph.facebook.com/v11.0/oauth/access_token?client_id={this._appSettings.Facebook.ClientId}&redirect_uri={this._appSettings.Facebook.RedirectUrl}&client_secret={this._appSettings.Facebook.ClientSecret}&code={request.Code}");

                var authCodeChangeResponse = await httpClient.GetFromJsonAsync<AuthCodeChangeResponse>($"https://graph.facebook.com/v11.0/oauth/access_token?client_id={this._appSettings.Facebook.ClientId}&redirect_uri={this._appSettings.Facebook.RedirectUrl}&client_secret={this._appSettings.Facebook.ClientSecret}&code={request.Code}");

                this.logger.LogError($"Get Acces Token: https://graph.facebook.com/debug_token?input_token={authCodeChangeResponse.access_token}&access_token={this._appSettings.Facebook.ClientToken}");

                var inspectAccessToken = await httpClient.GetFromJsonAsync<InspectAccessTokenResponse>($"https://graph.facebook.com/debug_token?input_token={authCodeChangeResponse.access_token}&access_token={this._appSettings.Facebook.ClientToken}");

                this.logger.LogError($"Inspect Access Token: https://graph.facebook.com/debug_token?input_token={authCodeChangeResponse.access_token}&access_token={this._appSettings.Facebook.ClientToken}");

                if (!inspectAccessToken.is_valid)
                    return BadRequest(new { ErrorMessage = "Can't validate Facebook Authorization Code" });

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authCodeChangeResponse.access_token);
                var userInfo = await httpClient.GetFromJsonAsync<UserInfoResponse>($"https://graph.facebook.com/v11.0/me?fields=name, email");

                return Ok(new { userInfo });

                var foundUser = await this._context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == userInfo.email);

                if (foundUser is null)
                    return BadRequest(new { ErrorMessage = "Invalid user os password" });

                var generatedToken = _tokenGenerator.Generate(foundUser, issuer, audience);

                return Ok(new
                {
                    name = foundUser.Name,
                    email = foundUser.Email,
                    expiresIn = generatedToken.ExpiresIn,
                    token = generatedToken.Token
                });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(new { ErrorMessage = $"Can't validate Facebook Authorization Code. { ex.Message }" });
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { ErrorMessage = $"Method not suportted. { ex.Message }" });
            }
            catch (JsonException ex)
            {
                return BadRequest(new { ErrorMessage = $"Can't parser Facebook json response. { ex.Message }" });
            }
            catch (TokenGenerationException ex)
            {
                return BadRequest(new { ErrorMessage = ex.Message });
            }
        }

        public record TokenPostRequest(string User, string Password);
        public record FacebookTokenPostRequest(string Code);
        public record AuthCodeChangeResponse(string access_token);
        public record InspectAccessTokenResponse(bool is_valid, string user_id);
        public record UserInfoResponse(string name, string email);
    }
}