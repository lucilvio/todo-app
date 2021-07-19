using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vue.TodoApp.Model;

namespace Vue.TodoApp
{
    public class JwtTokenGenerator
    {
        private readonly AppSettings _settings;

        public JwtTokenGenerator(IOptions<AppSettings> settings)
        {
            this._settings = settings != null ? settings.Value : throw new System.ArgumentNullException(nameof(settings));
        }

        public GeneratedToken Generate(User user, string issuer, string audience)
        {
            var now = DateTime.UtcNow;
            var expiresIn = now.AddHours(_settings.Jwt.Expiration);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.Jwt.Secret);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name),
                    new Claim("email", user.Email),
                }),
                IssuedAt = now,
                Issuer = issuer,
                Audience = audience,                
                Expires = expiresIn,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);
            var stringToken = tokenHandler.WriteToken(token);

            try
            {
                tokenHandler.ValidateToken(stringToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _settings.Jwt.Issuer,
                    ValidAudience = _settings.Jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.Jwt.Secret))
                }, out var validatedToken);

                return new GeneratedToken(stringToken, new DateTimeOffset(expiresIn).ToUnixTimeSeconds());
            }
            catch (Exception ex)
            {
                throw new TokenGenerationException($"Error while trying to generate token", ex);
            }
        }

        public record GeneratedToken(string Token, long ExpiresIn);
    }

    public class TokenGenerationException : Exception
    {
        public TokenGenerationException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}