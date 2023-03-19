using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyDbContext>(options => options
        .UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection") ?? throw new
            InvalidOperationException("Connection string 'DefaultConnection' not found.")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Olá mundo!");

app.MapGet("/ToDo", async (MyDbContext context) => await context.ToDos.ToListAsync());

app.MapGet("/ToDo/{id}", async (int id, MyDbContext context) =>
{
    var entity = await context.ToDos.FindAsync(id);
    if (entity == null)
        return Results.NotFound();
    return Results.Ok(entity);
});

app.MapPost("/ToDo", async (ToDo todo, MyDbContext context) =>
{
    context.ToDos.Add(todo);
    await context.SaveChangesAsync();
    return Results.Created($"/ToDo/{todo.Id}", todo);
});

app.MapPut("/ToDo/{id}", async (int id, ToDo todo, MyDbContext context) =>
{
    var entity = await context.ToDos.FindAsync(id);
    if (entity == null)
        return Results.NotFound();
    entity.Name = todo.Name;
    entity.IsComplete = todo.IsComplete;
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/ToDo/{id}", async (int id, MyDbContext context) =>
{
    var entity = await context.ToDos.FindAsync(id);
    if (entity == null)
        return Results.NotFound();
    context.ToDos.Remove(entity);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


app.Run();

class ToDo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<ToDo> ToDos => Set<ToDo>();
}