using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", () => "ola mundo");

// buscando  tarefas pelo ID
app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound("Tarefa não encontrada"));

//buncando todas as tarefas filtrando por valor da proriedade isConcluida seja igual a true
app.MapGet("/tarefas/concluidas", async (AppDbContext db) =>
    await db.Tarefas.Where(x => x.isConcluida == true).ToListAsync());

// buscando todas as tarefas.
app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

// criando uma nota Tarefa
app.MapPost("/tarefas", async (AppDbContext db, Tarefa tarefa) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (AppDbContext db, Tarefa tarefa, int id) =>
{
    var task = await db.Tarefas.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Nome = tarefa.Nome;
    task.isConcluida = tarefa.isConcluida;

    await db.SaveChangesAsync();

    return Results.NoContent();


});


app.MapDelete("/tarefas/{id}", async (AppDbContext db, int id) =>

{


    if (await db.Tarefas.FirstOrDefaultAsync(x => x.Id == id) is Tarefa tarefa)
    {

        db.Tarefas.Remove(tarefa);
        db.SaveChanges();

        return Results.Ok();
    }
    return Results.NotFound();
}
);

app.Run();



class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool isConcluida { get; set; }

}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}