using System.Net;
using System.Net.Http.Json;
using DBData;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace IntTests;

public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper output;
    public UnitTest1(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        this.output = output;
    }
    [Fact]
    public async Task GetCharacters()
    {
        var client = _factory.CreateClient();
        var response =  await client.GetAsync("/sw-characters");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var characters = await response.Content.ReadFromJsonAsync<List<SwCharacter>>();
        foreach (var character in characters)
        {
            output.WriteLine(character.Name);
        }
    }

    [Fact]
    public async Task AddAddGetRemoveAll()
    {
        var client = _factory.CreateClient();
        var character = new SwCharacter
        {
            Name = "Test",
            Faction = "Test",
            Homeworld = "Test",
            Species = "Test"
        };
        var responseAdd = await client.PostAsJsonAsync("/sw-characters", character);
        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        var addedCharacter = await responseAdd.Content.ReadFromJsonAsync<SwCharacter>();
        Assert.Equal(character.Name, addedCharacter.Name);

        responseAdd = await client.PostAsJsonAsync("/sw-characters", character);
        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        addedCharacter = await responseAdd.Content.ReadFromJsonAsync<SwCharacter>();
        Assert.Equal(character.Name, addedCharacter.Name);

        var response =  await client.GetAsync("/sw-characters");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var characters = await response.Content.ReadFromJsonAsync<List<SwCharacter>>();

        foreach (var ch in characters)
        {
            var responseRemove = await client.DeleteAsync($"/sw-characters/{ch.Id}");
            Assert.Equal(HttpStatusCode.OK, responseRemove.StatusCode);
        }

    }

    [Fact]
    public async Task AddPutRemoveRemove()
    {
        var client = _factory.CreateClient();
        var character = new SwCharacter
        {
            Name = "Test",
            Faction = "Test",
            Homeworld = "Test",
            Species = "Test"
        };
        var responseAdd = await client.PostAsJsonAsync("/sw-characters", character);
        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        var addedCharacter = await responseAdd.Content.ReadFromJsonAsync<SwCharacter>();
        Assert.Equal(character.Name, addedCharacter.Name);

        var responsePut = await client.PutAsJsonAsync($"/sw-characters/{addedCharacter.Id}", new SwCharacter
        {
            Name = "Test2",
            Faction = "Test2",
            Homeworld = "Test2",
            Species = "Test2"
        });
        Assert.Equal(HttpStatusCode.OK, responsePut.StatusCode);
        var changedCharacter = await responsePut.Content.ReadFromJsonAsync<SwCharacter>();
        Assert.Equal("Test2", changedCharacter.Name);

        var responseRemove = await client.DeleteAsync($"/sw-characters/{addedCharacter.Id}");
        Assert.Equal(HttpStatusCode.OK, responseRemove.StatusCode);

        var responseRemove2 = await client.DeleteAsync($"/sw-characters/{addedCharacter.Id}");
        Assert.Equal(HttpStatusCode.NotFound, responseRemove2.StatusCode);
    }
}