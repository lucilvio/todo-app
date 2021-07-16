using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Vue.TodoApp
{
    [Route("lists")]
    public class TasksController : ControllerBase
    {
        private readonly TodoAppContext _context;
        private readonly IHubContext<ChangesListenerHub> _changesListenerHub;

        public TasksController(TodoAppContext context, IHubContext<ChangesListenerHub> changesListenerHub)
        {
            this._context = context;
            this._changesListenerHub = changesListenerHub;
        }

        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> Get(Guid id)
        {
            var foundList = await this._context.Lists
                .AsNoTracking()
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if(foundList is null)
                return NotFound("List not found");

            return Ok(foundList.Tasks.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                done = t.Done
            }));
        }

        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> Post(Guid id, [FromBody] PostTaskRequest request)
        {
            var foundList = await this._context.Lists    
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if(foundList is null)
                return NotFound("List not found");

            foundList.AddTask(request.Name);            
            await _context.SaveChangesAsync();

            await _changesListenerHub.Clients.All.SendAsync("tasksChanged");

            return Ok();
        }

        [HttpDelete("{id}/tasks/{taskId}")]
        public async Task<IActionResult> Delete(Guid id, Guid taskId)
        {
            var foundList = await this._context.Lists            
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if(foundList is null)
                return NotFound("List not found");

            foundList.RemoveTask(taskId);
            await _context.SaveChangesAsync();

            await _changesListenerHub.Clients.All.SendAsync("tasksChanged");

            return Ok();
        }

        [HttpPut("{id}/tasks/{taskId}/done")]
        public async Task<IActionResult> Done(Guid id, Guid taskId)
        {
            var foundList = await this._context.Lists            
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if(foundList is null)
                return NotFound("List not found");

            foundList.Task(taskId).MarkAsDone();
            await _context.SaveChangesAsync();

            await _changesListenerHub.Clients.All.SendAsync("tasksChanged");

            return Ok();
        }

        [HttpPut("{id}/tasks/{taskId}/undo")]
        public async Task<IActionResult> Undo(Guid id, Guid taskId)
        {
            var foundList = await this._context.Lists            
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if(foundList is null)
                return NotFound("List not found");

            foundList.Task(taskId).MarkAsTodo();
            await _context.SaveChangesAsync();

            await _changesListenerHub.Clients.All.SendAsync("tasksChanged");

            return Ok();
        }

        public record PostTaskRequest(string Name);
    }
}