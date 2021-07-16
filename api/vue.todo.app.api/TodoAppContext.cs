using Microsoft.EntityFrameworkCore;

namespace Vue.TodoApp
{
    public class TodoAppContext : DbContext
    {
        public TodoAppContext(DbContextOptions options) : base(options)
        {
            base.Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<TaskList>().ToTable("Lists");
            modelBuilder.Entity<TaskList>().Property(p => p.Name).IsRequired();
            modelBuilder.Entity<TaskList>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<TaskList>().HasData(new TaskList("Default"));
            
            modelBuilder.Entity<TaskList.Task>().ToTable("Tasks");
            modelBuilder.Entity<TaskList.Task>().Property(p => p.Name).IsRequired();
            modelBuilder.Entity<TaskList.Task>().Property(p => p.Done).IsRequired();           
            modelBuilder.Entity<TaskList.Task>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<TaskList.Task>().HasOne<TaskList>().WithMany(t => t.Tasks).IsRequired();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TaskList> Lists { get; set; }
    }
}