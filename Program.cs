using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TodoDbContext>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/todoitems", async (TodoDbContext context) => await context.Todos.ToListAsync());
app.MapGet("/todoitems/complete",
    async (TodoDbContext context) => await context.Todos.Where(t => t.IsComplete).ToListAsync());
app.MapGet("/todoitems/{id}",
    async (TodoDbContext context, int id) =>
        await context.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound());
app.MapPost("/todoitems", async (TodoDbContext context, Todo todo) =>
{
    context.Add(todo);
    await context.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (TodoDbContext context, int id, Todo inputTodo) =>
{
    var todo = await context.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (TodoDbContext context, int id) =>
{
    if (await context.Todos.FindAsync(id) is Todo  todo)
    {
        context.Todos.Remove(todo);
        await context.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos => Set<Todo>();
}

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}