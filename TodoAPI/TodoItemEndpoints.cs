using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using TodoAPI.Data;
using TodoAPI.Model;
namespace TodoAPI;

public static class TodoItemEndpoints
{
    public static void MapTodoItemEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/TodoItem").WithTags(nameof(TodoItem));

        group.MapGet("/", async (TodoAPIContext db) =>
        {
            return await db.TodoItem.ToListAsync();
        })
        .WithName("GetAllTodoItems")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<TodoItem>, NotFound>> (int id, TodoAPIContext db) =>
        {
            return await db.TodoItem.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is TodoItem model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetTodoItemById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, TodoItem todoItem, TodoAPIContext db) =>
        {
            var affected = await db.TodoItem
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                  .SetProperty(m => m.Id, todoItem.Id)
                  .SetProperty(m => m.Title, todoItem.Title)
                  .SetProperty(m => m.Description, todoItem.Description)
                  .SetProperty(m => m.DueDate, todoItem.DueDate)
                  .SetProperty(m => m.Status, todoItem.Status)
                  .SetProperty(m => m.UserId, todoItem.UserId)
                );

            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateTodoItem")
        .WithOpenApi();

        group.MapPost("/", async (TodoItem todoItem, TodoAPIContext db) =>
        {
            db.TodoItem.Add(todoItem);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/TodoItem/{todoItem.Id}",todoItem);
        })
        .WithName("CreateTodoItem")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, TodoAPIContext db) =>
        {
            var affected = await db.TodoItem
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();

            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteTodoItem")
        .WithOpenApi();

        group.MapGet("/user/{userId}", async Task<Results<Ok<List<TodoItem>>, NotFound>> (int userId, TodoAPIContext db) =>
        {
            var todoItems = await db.TodoItem.AsNoTracking()
                .Where(model => model.UserId == userId)
                .ToListAsync();

            return todoItems.Count > 0 ? TypedResults.Ok(todoItems) : TypedResults.NotFound();
        })
        .WithName("GetTodoItemsByUserId")
        .WithOpenApi();

        group.MapPut("/{id}/toggle-status", async Task<Results<Ok<TodoItem>, NotFound>> (int id, TodoAPIContext db) =>
        {
            var todoItem = await db.TodoItem.FindAsync(id);

            if (todoItem == null)
            {
                return TypedResults.NotFound();
            }

            todoItem.Status = !todoItem.Status;

            await db.SaveChangesAsync();

            return TypedResults.Ok(todoItem);
        })
        .WithName("ToggleTodoItemStatus")
        .WithOpenApi();

    }

}
