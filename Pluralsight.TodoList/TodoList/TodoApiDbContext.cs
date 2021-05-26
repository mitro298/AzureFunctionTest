using Microsoft.EntityFrameworkCore;

namespace TodoList
{
    public class TodoApiDbContext : DbContext 
    {
        public TodoApiDbContext(DbContextOptions<TodoApiDbContext> options)
            : base(options)
        { }

        public DbSet<Todo> Todo { get; set; }
    }
}
