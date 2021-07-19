using System;
using System.Collections.Generic;
using System.Linq;

namespace Vue.TodoApp.Model
{
    public class User
    {
        private User()
        {
            this.Id = Guid.NewGuid();
            this.Tasks = new List<Task>();
            this.Lists = new List<TaskList>();
        }

        public User(string name, string email, string password) : this()
        {
            this.Name = name;
            this.Email = email;
            this.Password = password;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        
        public IList<Task> Tasks { get; private set; }
        public IList<TaskList> Lists { get; private set; }

        internal void AddTaskToList(string taskName, Guid list)
        {
            var foundList = this.Lists.FirstOrDefault(l => l.Id == list);

            if(foundList is null)
                return;
                
            foundList.AddTask(taskName);
        }

        internal void AddTask(string name)
        {
            this.Tasks.Add(new Task(name, this.Id));
        }

        internal void AddList(string name)
        {
            this.Lists.Add(new TaskList(name, this.Id));
        }
    }
}