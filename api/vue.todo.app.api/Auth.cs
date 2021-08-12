using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Vue.TodoApp
{
    public class Auth
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Auth(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        internal LoggedUser GetLoggedUser()
        {
            var contextUser = this._httpContextAccessor.HttpContext.User;

            if(contextUser is null)
                throw new BusinessException("No user logged in");

            var id = contextUser.Claims.FirstOrDefault(c => c.Type == "id");

            if(id is null || string.IsNullOrEmpty(id.Value))
                throw new BusinessException("Can't identify logged user");

            return new LoggedUser(id.Value);
        }

        public record LoggedUser
        {
            public LoggedUser(string id)
            {
                this.Id = Guid.Parse(id);
            }

            public Guid Id { get; }
        }
    }
}