using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Vue.TodoApp
{
    [Route("lists")]
    public class ListsController : ControllerBase
    {
        private readonly TodoAppContext _context;

        public ListsController(TodoAppContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lists = await this._context.Lists.ToListAsync();

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

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var foundList = await _context.Lists.FirstOrDefaultAsync(l => l.Id == id);

            if (foundList is null)
                return NotFound();

            _context.Lists.Remove(foundList);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public record PostListRequest(string Name);
    }
}