using Vue.TodoApp.Model;
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

            modelBuilder.Entity<Task>().ToTable("Tasks");
            modelBuilder.Entity<Task>().Property(p => p.Name).IsRequired();
            modelBuilder.Entity<Task>().Property(p => p.Done).IsRequired();
            modelBuilder.Entity<Task>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Task>().HasOne<TaskList>().WithMany(t => t.Tasks).HasForeignKey("ListId").OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskList> Lists { get; set; }
    }
}