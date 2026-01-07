using BE.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("todo_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title)
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(item => item.IsCompleted)
                .HasDefaultValue(false);
            entity.Property(item => item.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.HasIndex(item => item.IsCompleted);
        });
    }
}
