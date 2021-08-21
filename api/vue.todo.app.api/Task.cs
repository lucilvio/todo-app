using System;

namespace Vue.TodoApp.Model
{
    public class Task
    {
        public Task()
        {
            this.Id = Guid.NewGuid();
        }

        public Task(string name, Guid userId, Guid? list = null) : this()
        {
            if (string.IsNullOrEmpty(name))
                throw new BusinessException("Can't create task without name");

            this.Name = name;
            this.UserId = userId;
            this.ListId = list;
        }

        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        public bool Done { get; private set; } = false;
        public bool Important { get; private set; } = false;
        public bool Deleted { get; private set; } = false;
        public Guid? ListId { get; private set; } = null;

        public void MarkAsDone() => this.Done = true;
        public void MarkAsTodo() => this.Done = false;
        public void MarkAsImportant() => this.Important = true;
        public void MarkAsNotImportant() => this.Important = false;
        
        public bool CanBeDeleted => !this.Deleted && !this.Done;
        public bool CanBeMarkedAsImportant => !this.Deleted && !this.Done;
        public bool CanBeMarkedAsDone => !this.Deleted;

        public void Delete()
        {
            this.Done = false;
            this.Important = false;
            this.Deleted = true;
        }
    }
}