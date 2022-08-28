using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("PostmanAPI"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "This API works with UseInMemoryDatabase, once the API is restarted all data is deleted as well");

#region TodoList

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    Console.WriteLine("Here the Id value"); 
    Console.WriteLine(todo.Id);

    if (todo.Id > 0) {
        return Results.Problem("Please remove the Id on Request Body");
    }
    if (String.IsNullOrEmpty(todo.Name)) {
        return Results.Problem("Name cannot be a null or empty text");
    }

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

#endregion

#region Donations

app.MapGet("/donations", async (TodoDb db) => await db.Donation.ToListAsync());

//app.MapGet("/donations/silent", async (TodoDb db) =>
//    await db.Donation.Where(t => t.IsSilentDonor).ToListAsync());

//app.MapGet("/donations/{id}", async (int id, TodoDb db) =>
//    await db.Todos.FindAsync(id)
//        is Todo todo
//            ? Results.Ok(todo)
//            : Results.NotFound());

app.MapPost("/donations", async (Donation donation, TodoDb db) =>
{
    db.Donation.Add(donation);
    await db.SaveChangesAsync();

    return Results.Created($"/donations/{donation.Id}", donation);
});

app.MapPut("/donations/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/donations/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

#endregion

app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class Donation
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public bool IsSilentDonor { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Donation> Donation => Set<Donation>();
}