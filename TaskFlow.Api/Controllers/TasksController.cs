using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static readonly List<TaskItem> Tasks =
        [
            new TaskItem
        {
            Id = 1,
            Title = "Learn Docker",
            Description = "Understand Docker fundamentals",
            IsCompleted = false
        }
        ];

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Tasks);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var task = Tasks.FirstOrDefault(x => x.Id == id);

            if (task is null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public IActionResult Create(TaskItem task)
        {
            task.Id = Tasks.Count == 0
                ? 1
                : Tasks.Max(x => x.Id) + 1;

            Tasks.Add(task);

            return CreatedAtAction(
                nameof(GetById),
                new { id = task.Id },
                task);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, TaskItem updatedTask)
        {
            var task = Tasks.FirstOrDefault(x => x.Id == id);

            if (task is null)
                return NotFound();

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.IsCompleted = updatedTask.IsCompleted;

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var task = Tasks.FirstOrDefault(x => x.Id == id);

            if (task is null)
                return NotFound();

            Tasks.Remove(task);

            return NoContent();
        }
    }


}
