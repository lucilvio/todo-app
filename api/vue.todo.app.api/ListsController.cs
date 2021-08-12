using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Vue.TodoApp.Model;

namespace Vue.TodoApp
{
    [Route("[controller]")]
    public class ListsController : ControllerBase
    {
        private readonly Auth _auth;
        private readonly TodoAppContext _context;
        private readonly IHubContext<NotifyHub> _notifyHub;

        public ListsController(TodoAppContext context, Auth auth, IHubContext<NotifyHub> notifyHub)
        {
            this._auth = auth;
            this._context = context;
            this._notifyHub = notifyHub;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var loggedUser = this._auth.GetLoggedUser();
            
            var foundUser = await this._context.Users
                .AsNoTracking()
                .Include(u => u.Lists)
                .FirstOrDefaultAsync(u => u.Id == loggedUser.Id);
                
            return Ok(foundUser.Lists.Select(l => new
            {
                id = l.Id,
                name = l.Name
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostListRequest request)
        {
            var loggedUser = this._auth.GetLoggedUser();
            
            var foundUser = await this._context.Users
                .Include(u => u.Lists)
                .FirstOrDefaultAsync(u => u.Id == loggedUser.Id);

            foundUser.AddList(request.Name);
            await _context.SaveChangesAsync();

            await this.SendListChangedEvent();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var foundList = await _context.Lists.Include(l => l.Tasks).FirstOrDefaultAsync(l => l.Id == id);

            if (foundList is null)
                return NotFound();

            _context.Lists.Remove(foundList);
            await _context.SaveChangesAsync();

            await this.SendListChangedEvent();

            return Ok();
        }

        private async System.Threading.Tasks.Task SendListChangedEvent() =>
            await this._notifyHub.Clients.All.SendAsync("listsChanged");

        public record PostListRequest(string Name);
    }
}