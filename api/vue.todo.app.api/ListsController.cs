using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Vue.TodoApp.Model;

namespace Vue.TodoApp
{
    [Route("lists")]
    public class ListsController : ControllerBase
    {
        private readonly TodoAppContext _context;
        private readonly IHubContext<ChangesListenerHub> _changesListenerHub;

        public ListsController(TodoAppContext context, IHubContext<ChangesListenerHub> changesListenerHub)
        {
            this._context = context;
            this._changesListenerHub = changesListenerHub;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lists = await this._context.Lists
                .AsNoTracking()
                .ToListAsync();

            return Ok(lists.Select(l => new
            {
                id = l.Id,
                name = l.Name
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostListRequest request)
        {
            await this._context.AddAsync(new TaskList(request.Name));
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
            await this._changesListenerHub.Clients.All.SendAsync("listsChanged");

        public record PostListRequest(string Name);
    }
}