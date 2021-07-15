using System;
using System.Collections.Generic;
using System.Linq;

namespace Vue.TodoApp
{
    public class TaskList
    {
        public TaskList(string name)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Tasks = new List<Task>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public IList<Task> Tasks { get; set; }

        public void AddTask(string name)
        {
            this.Tasks.Add(new Task(name));
        }

        public void RemoveTask(Guid taskId)
        {
            this.Tasks = this.Tasks.Where(t => t.Id != taskId).ToList();
        }

        internal Task Task(Guid id)
        {
            return this.Tasks.FirstOrDefault(t => t.Id == id);
        }
    }

    public class Task
    {
        public Task(string name)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Done = false;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Done { get; set; }

        internal void MarkAsDone()
        {
            this.Done = true;
        }

        internal void MarkAsTodo()
        {
            this.Done = false;
        }
    }
}