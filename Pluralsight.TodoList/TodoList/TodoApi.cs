using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TodoList
{
    public class TodoApi
    {
        private const string Route = "todo";
        private readonly TodoApiDbContext todoContext;

        public TodoApi(TodoApiDbContext todoContext)
        {
            this.todoContext = todoContext;
        }

        [FunctionName("GetTodos")]
        public async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var todos = await todoContext.Todo.ToListAsync();
            return new OkObjectResult(todos);
        }

        [FunctionName("GetTodoById")]
        public async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            var todo = await todoContext.Todo.FindAsync(Guid.Parse(id));
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo);
        }

        [FunctionName("CreateTodo")]
        public async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Route)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody, new ExpandoObjectConverter());

            var todo = new Todo() { TaskDescription = input.TaskDescription };

            await todoContext.Todo.AddAsync(todo);
            await todoContext.SaveChangesAsync();

            return new OkObjectResult(todo);
        }


        [FunctionName("UpdateTodo")]
        public async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route + "/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            var todo = await todoContext.Todo.FindAsync(Guid.Parse(id));
            if (todo == null)
            {
                log.LogWarning($"Item {id} not found");
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
            todo.IsCompleted = updated.IsCompleted;

            if (!string.IsNullOrWhiteSpace(updated.TaskDescription))
            {
                todo.TaskDescription = updated.TaskDescription;
            }

            await todoContext.SaveChangesAsync();

            return new OkObjectResult(todo);
        }

        [FunctionName("DeleteTodo")]
        public async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            var todo = await todoContext.Todo.FindAsync(Guid.Parse(id));
            if (todo == null)
            {
                return new NotFoundResult();
            }

            todoContext.Todo.Remove(todo);
            await todoContext.SaveChangesAsync();
            return new OkResult();
        }
    }
}
