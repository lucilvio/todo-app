using System;
using System.Collections.Generic;

namespace Vue.TodoApp.Model
{
    public partial class TaskList
    {
        private TaskList() 
        {
            this.Id = Guid.NewGuid();
            this.Tasks = new List<Task>();
        }
        
        public TaskList(string name, Guid userId) : this()
        {
            this.Name = name;
            UserId = userId;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; }
        public IList<Task> Tasks { get; set; }

        public void AddTask(string name)
        {
            this.Tasks.Add(new Task(name, this.UserId, this.Id));
        }
    }
}