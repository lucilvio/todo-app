using System;
using System.Collections.Generic;
using System.Linq;

namespace Vue.TodoApp.Model
{
    public partial class TaskList
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
            var foundTask = this.Tasks.FirstOrDefault(t => t.Id == taskId);

            if(foundTask is null)
                return;
                
            this.Tasks.Remove(foundTask);
        }

        internal Task FindTask(Guid id)
        {
            return this.Tasks.FirstOrDefault(t => t.Id == id);
        }
    }
}