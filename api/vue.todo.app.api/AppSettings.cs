using Microsoft.Extensions.Primitives;

namespace Vue.TodoApp
{
    public class AppSettings
    {
        public string AllowedOrigin { get; set; }
        public JwtSettings Jwt { get; set; }
        public FacebookSettings Facebook { get; set; }

        public class JwtSettings
        {
            public string Secret { get; set; }
            public double Expiration { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
        }

        public class FacebookSettings
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string ClientToken { get; set; }
            public string RedirectUrl { get; set; }
        }
    }
}