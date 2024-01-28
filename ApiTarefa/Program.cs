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
app.MapGet("/", () => "Olá mundo");

app.MapGet("frases", async () => await new HttpClient().GetStreamAsync("")
);

//Lista de tarefas
app.MapGet("/tarefas", async (AppDbContext db) =>
{
    return await db.tarefas.ToListAsync();
}
);
//Faz o cadastro dos dados
app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
}
);

//Faz um busca atraves do id
app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) =>
    await db.tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

//Verifica se as taferas foram concluida 
app.MapGet("/tarefas/concluida", async (int id, AppDbContext db) => await db.tarefas.Where(t => t.IsConclcuida).ToListAsync());

//Edita o cadastro dos dados tarefa
app.MapPut("/tarefas/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.tarefas.FindAsync(id);

    if (tarefa is null) return Results.NotFound("");
    
    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConclcuida = inputTarefa.IsConclcuida;

    await db.SaveChangesAsync();
    return Results.NoContent();
}
);

//Delete o cadastro da tarefa
app.MapDelete("tarefa/{id}", async (int id, AppDbContext db) =>
{
    if (await db.tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.tarefas.Remove(tarefa);

        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }

    return Results.NotFound();
}
);

app.Run();


class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConclcuida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base (options)
    {

    }
    public DbSet<Tarefa> tarefas { get; set; }
}
