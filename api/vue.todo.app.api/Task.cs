using System;

namespace Vue.TodoApp.Model
{
    public class Task
    {
        public Task(string name)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Done = false;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool Done { get; private set; }
        public bool Important { get; private set; }
        public Guid? ListId { get; private set; }

        public void MarkAsDone() => this.Done = true;
        public void MarkAsTodo() => this.Done = false;
        public void MarkAsImportant() => this.Important = true;
        public void MarkAsNotImportant() => this.Important = false;
    }
}