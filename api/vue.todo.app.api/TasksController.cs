using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Vue.TodoApp
{
    [Route("tasks")]
    public class TasksController : ControllerBase
    {
        private readonly TodoAppContext _context;
        private readonly IHubContext<ChangesListenerHub> _changesListenerHub;

        public TasksController(TodoAppContext context, IHubContext<ChangesListenerHub> changesListenerHub)
        {
            this._context = context;
            this._changesListenerHub = changesListenerHub;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var foundTasks = await this._context.Tasks
                .AsNoTracking()
                .ToListAsync();

            return Ok(foundTasks.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                done = t.Done,
                important = t.Important,
                list = t.ListId
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid id, [FromBody] PostTaskRequest request)
        {
            if (request.List.HasValue)
            {
                var foundList = this._context.Lists.FirstOrDefault(l => l.Id == request.List.Value);

                if (foundList is null)
                    return NotFound("List not found");

                foundList.AddTask(request.Name);
            }
            else
            {
                await this._context.Tasks.AddAsync(new Model.Task(request.Name));
            }

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

            this._context.Remove(foundTask);
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
            await this._changesListenerHub.Clients.All.SendAsync("tasksChanged");

        public record PostTaskRequest(string Name, Guid? List);
    }
}