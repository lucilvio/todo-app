using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Vue.TodoApp
{
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly Auth _auth;
        private readonly TodoAppContext _context;
        private readonly IHubContext<NotifyHub> _notifyHub;

        public TasksController(TodoAppContext context, Auth auth, IHubContext<NotifyHub> notifyHub)
        {
            this._context = context;
            this._auth = auth;
            this._notifyHub = notifyHub;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var loggedUser = this._auth.GetLoggedUser();
            
            var foundUser = await this._context.Users
                .AsNoTracking()
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == loggedUser.Id);

            return Ok(foundUser.Tasks.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                done = t.Done,
                important = t.Important,
                deleted = t.Deleted,
                list = t.ListId,
                t.CanBeDeleted,
                t.CanBeMarkedAsDone,
                t.CanBeMarkedAsImportant
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostTaskRequest request)
        {
            if(request is null)
                throw new BusinessException("Error while trying to add task. Make sure you send all required data");

            var loggedUser = this._auth.GetLoggedUser();
            
            var foundUser = await this._context.Users
                .Include(u => u.Lists)
                .FirstOrDefaultAsync(u => u.Id == loggedUser.Id);

            if (request.List is not null && request.List.HasValue)
                foundUser.AddTaskToList(request.Name, request.List.Value);
            else
                foundUser.AddTask(request.Name);

            await _context.SaveChangesAsync();

            await this.SendTaskChangedEvent();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var foundTask = await this._context.Tasks
                .FirstOrDefaultAsync(l => l.Id == id);

            if (foundTask is null)
                return NotFound("Task not found");

            foundTask.Delete();

            await _context.SaveChangesAsync();

            await this.SendTaskChangedEvent();

            return Ok();
        }

        [HttpPut("{id}/done")]
        public async Task<IActionResult> Done(Guid id, Guid taskId)
        {
            var foundTask = await this._context.Tasks
                .FirstOrDefaultAsync(l => l.Id == id);

            if (foundTask is null)
                return NotFound("Task not found");

            foundTask.MarkAsDone();
            await _context.SaveChangesAsync();

            await this.SendTaskChangedEvent();

            return Ok();
        }

        [HttpPut("{id}/undo")]
        public async Task<IActionResult> Undo(Guid id, Guid taskId)
        {
            var foundTask = await this._context.Tasks
                .FirstOrDefaultAsync(l => l.Id == id);

            if (foundTask is null)
                return NotFound("Task not found");

            foundTask.MarkAsTodo();
            await _context.SaveChangesAsync();

            await this.SendTaskChangedEvent();

            return Ok();
        }

        [HttpPut("{id}/important")]
        public async Task<IActionResult> Important(Guid id)
        {
            var foundTask = await this._context.Tasks
                .FirstOrDefaultAsync(l => l.Id == id);

            if (foundTask is null)
                return NotFound("Task not found");

            foundTask.MarkAsImportant();
            await _context.SaveChangesAsync();

            await this.SendTaskChangedEvent();

            return Ok();
        }

        [HttpPut("{id}/not-important")]
        public async Task<IActionResult> NotImportant(Guid id)
        {
            var foundTask = await this._context.Tasks
                .FirstOrDefaultAsync(l => l.Id == id);

            if (foundTask is null)
                return NotFound("Task not found");

            foundTask.MarkAsNotImportant();
            await _context.SaveChangesAsync();

            await this.SendTaskChangedEvent();

            return Ok();
        }

        private async Task SendTaskChangedEvent() =>
            await this._notifyHub.Clients.All.SendAsync("tasksChanged");

        public record PostTaskRequest(string Name, Guid? List);
    }
}