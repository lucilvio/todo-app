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
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(p => p.Name).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<User>().Property(p => p.Email).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<User>().Property(p => p.Password).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<User>().Property(p => p.Id).ValueGeneratedNever();            
            modelBuilder.Entity<User>().HasData(new User("Admin", "admin@mail.com", "123456"));

            modelBuilder.Entity<TaskList>().ToTable("Lists");
            modelBuilder.Entity<TaskList>().Property(p => p.Name).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<TaskList>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<TaskList>().HasOne<User>().WithMany(u => u.Lists).HasForeignKey(tl => tl.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);           

            modelBuilder.Entity<Task>().ToTable("Tasks");
            modelBuilder.Entity<Task>().Property(p => p.Name).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Task>().Property(p => p.Done).IsRequired();
            modelBuilder.Entity<Task>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Task>().HasOne<TaskList>().WithMany(t => t.Tasks).HasForeignKey(p => p.ListId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Task>().HasOne<User>().WithMany(u => u.Tasks).HasForeignKey(u => u.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskList> Lists { get; set; }
    }
}