using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Vue.TodoApp
{
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly TodoAppContext _context;

        public UsersController(TodoAppContext context)
        {
            this._context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] UserPostRequest request)
        {
            var userWithSameEmail = await this._context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (userWithSameEmail is not null)
                return BadRequest(new { message = "Email is not available. Plase, try a different one" });

            await this._context.AddAsync(new Model.User(request.Name, request.Email, request.Password));
            await this._context.SaveChangesAsync();
            
            return Ok();
        }

        public record UserPostRequest(string Name, string Email, string Password);
    }
}