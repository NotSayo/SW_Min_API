using System.Reflection;
using API_Server.Queries;
using Microsoft.EntityFrameworkCore;
using DBData;
using Microsoft.AspNetCore.Mvc;

var assembly = Assembly.GetExecutingAssembly();

var builder = WebApplication.CreateBuilder(args);
var conf = builder.Configuration;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SwRepository>();
builder.Services.AddDbContext<SwContext>(options =>
{
    options.UseSqlite(conf.GetConnectionString("DefaultConnection"), sqliteOptions =>
    {
        sqliteOptions.MigrationsAssembly(assembly.FullName);
    });
});

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapGet("/sw-characters", (SwRepository db, string name = "", string faction = "", string homeland = "", string species = "") =>
    db.GetCharacters(name, faction, homeland, species));

app.MapGet("/sw-characters/{id}", (SwRepository db, int id) =>
{
    var character = db.GetCharacter(id);
    return character is not null ? Results.Ok(character) : Results.NotFound();
});

app.MapPost("/sw-characters", (SwRepository db, [FromBody] CharacterInput characterId) =>
{
    if(string.IsNullOrWhiteSpace(characterId.Name) || string.IsNullOrWhiteSpace(characterId.Faction) || string.IsNullOrWhiteSpace(characterId.Homeworld) || string.IsNullOrWhiteSpace(characterId.Species))
        return Results.BadRequest("All fields must be filled");
    var character = new SwCharacter(characterId.Name, characterId.Faction, characterId.Homeworld, characterId.Species);
    db.AddCharacter(character);
    return Results.Created($"/sw-characters/{character.Id}", character);
});

app.MapPut("/sw-characters/{id}", (SwRepository db, int id, [FromBody] CharacterInput characterId) =>
{
    if(string.IsNullOrWhiteSpace(characterId.Name) || string.IsNullOrWhiteSpace(characterId.Faction) || string.IsNullOrWhiteSpace(characterId.Homeworld) || string.IsNullOrWhiteSpace(characterId.Species))
        return Results.BadRequest("All fields must be filled");
    var character = new SwCharacter(characterId.Name, characterId.Faction, characterId.Homeworld, characterId.Species);
    var result = db.ChangeCharacter(id, character);
    return result ? Results.Ok(db.GetCharacter(id)) : Results.NotFound();
});

app.MapDelete("/sw-characters/{id}", (SwRepository db, int id) =>
{
    var result = db.DeleteCharacter(id);
    return result ? Results.Ok() : Results.NotFound();
});

app.MapGraphQL(path: "/graphql");


app.Run();

public partial class Program {}

class CharacterInput
{
    public string Name { get; set; } = "";
    public string Faction { get; set; } = "";
    public string Homeworld { get; set; } = "";
    public string Species { get; set; } = "";
}