using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Model;

namespace TodoAPI.Data
{
    public class TodoAPIContext : DbContext
    {
        public TodoAPIContext (DbContextOptions<TodoAPIContext> options)
            : base(options)
        {
        }

        public DbSet<TodoAPI.Model.TodoItem> TodoItem { get; set; } = default!;

        public DbSet<TodoAPI.Model.UserModel> UserModel { get; set; } = default!;
    }
}
