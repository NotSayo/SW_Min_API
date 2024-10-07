using System.Net;
using System.Net.Http.Json;
using DBData;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace IntTests;

public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> _factory;
    readonly ITestOutputHelper _output; // For testing in console
    public UnitTest1(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        this._output = output;
    }

    /*
     * The tests are seperated into 2 categories
     *
     * T0 - Are tests which cover the basic functions of the API and should be run first
     * T1 - Are tests which cover a full approach of the API with many requests that are supposed to
     *      fail (e.g. Delete a character twice). These tests should be run after T0
     *
     * If anything goes wrong, check all the T0 tests before troubleshooting
     */

    // T0 - Tests

    [Fact]
    public async Task T0_AddCharacter()
    {
        var client = _factory.CreateClient();
        var character = new SwCharacter()
        {
            Name = "Anakin Skywalker",
            Faction = "Jedi",
            Homeworld = "Tatooine",
            Species = "Human"
        };

        var response = await client.PostAsJsonAsync("/sw-characters", character);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetCharacterById()
    {
        var client = _factory.CreateClient();
        SwCharacter? character = await GetOneCharacter(client);
        if (character is null)
        {
            Assert.Fail("No Characters found, add a character first");
            return;
        }

        var response = await client.GetAsync($"sw-characters/{character.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task T0_GetAllCharacters()
    {
        var client = _factory.CreateClient();
        var response =  await client.GetAsync("/sw-characters");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task T0_AddAndGetACharacter()
    {
        var client = _factory.CreateClient();
        var responseCharacter = await client.GetAsync($"sw-characters/");
        SwCharacter? character;
        if(responseCharacter.StatusCode == HttpStatusCode.OK)
        {
            var characters = await responseCharacter.Content.ReadFromJsonAsync<List<SwCharacter>>();
            character = characters?.FirstOrDefault();
            if (character == null)
            {
                Assert.Fail("No Characters found, add a character first");
                return;
            }
        }
        else
        {
            Assert.Fail("DB error");
            return;
        }

        var response =  await client.GetAsync($"/sw-characters/{character.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);


    }

    [Fact]
    public async Task T0_UpdateCharacter()
    {
        var client = _factory.CreateClient();
        SwCharacter? character = await GetOneCharacter(client);
        if (character is null)
        {
            Assert.Fail("No Characters found, add a character first");
            return;
        }

        var response = await client.PutAsJsonAsync($"/sw-characters/{character.Id}", new SwCharacter
        {
            Name = $"{character.Name} v2",
            Faction = "Sith",
            Homeworld = "Tatooine",
            Species = "Human"
        });

        var updatedCharacter = await response.Content.ReadFromJsonAsync<SwCharacter>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Sith", updatedCharacter is null ? "" : updatedCharacter.Faction);
        Assert.Equal($"{character.Name} v2", updatedCharacter is null ? "" : updatedCharacter.Name);
    }

    [Fact]
    public async Task T0_DeleteCharacter()
    {
        var client = _factory.CreateClient();
        SwCharacter? character = await GetOneCharacter(client);
        if (character is null)
        {
            Assert.Fail("No Characters found, add a character first");
            return;
        }

        var response = await client.DeleteAsync($"/sw-characters/{character.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    // T1 - Tests

    [Fact]
    public async Task T1_AddAddGetRemoveAll()
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
        var addedCharacter = await responseAdd.Content.ReadFromJsonAsync<SwCharacter>();
        var responseAdd2 = await client.PostAsJsonAsync("/sw-characters", character);
        var addedCharacter2 = await responseAdd.Content.ReadFromJsonAsync<SwCharacter>();

        var response =  await client.GetAsync("/sw-characters");
        var characters = await response.Content.ReadFromJsonAsync<List<SwCharacter>>();
        if(characters is null)
            Assert.Fail();

        foreach (var ch in characters)
        {
            var responseRemove = await client.DeleteAsync($"/sw-characters/{ch.Id}");
            Assert.Equal(HttpStatusCode.OK, responseRemove.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        Assert.Equal(character.Name, addedCharacter?.Name);
        Assert.Equal(HttpStatusCode.Created, responseAdd2.StatusCode);
        Assert.Equal(character.Name, addedCharacter2?.Name);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    }

    [Fact]
    public async Task T1_AddPutRemoveRemove()
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
        var addedCharacter = await responseAdd.Content.ReadFromJsonAsync<SwCharacter>();

        var responsePut = await client.PutAsJsonAsync($"/sw-characters/{addedCharacter?.Id}", new SwCharacter
        {
            Name = "Test2",
            Faction = "Test2",
            Homeworld = "Test2",
            Species = "Test2"
        });

        var changedCharacter = await responsePut.Content.ReadFromJsonAsync<SwCharacter>();

        var responseRemove = await client.DeleteAsync($"/sw-characters/{addedCharacter?.Id}");

        var responseRemove2 = await client.DeleteAsync($"/sw-characters/{addedCharacter?.Id}");

        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        Assert.Equal(character.Name, addedCharacter?.Name);
        Assert.Equal(HttpStatusCode.OK, responsePut.StatusCode);
        Assert.Equal("Test2", changedCharacter?.Name);
        Assert.Equal(HttpStatusCode.OK, responseRemove.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, responseRemove2.StatusCode);
    }

    [Fact]
    public async Task T1_UpdateTwice()
    {
        var client = _factory.CreateClient();
        var character = new SwCharacter()
        {
            Name = "Test",
            Faction = "Test",
            Homeworld = "Test",
            Species = "Test"
        };
        var response = await client.PostAsJsonAsync("/sw-characters", character);

        var addedCharacter = await response.Content.ReadFromJsonAsync<SwCharacter>();

        addedCharacter.Name = "Test2";

        var responsePut = await client.PutAsJsonAsync($"/sw-characters/{addedCharacter.Id}", addedCharacter);
        var responsePut2 = await client.PutAsJsonAsync($"/sw-characters/{addedCharacter.Id}", addedCharacter);

        var getNewCharacter = await client.GetAsync($"/sw-characters/{addedCharacter.Id}");
        var newCharacter = await getNewCharacter.Content.ReadFromJsonAsync<SwCharacter>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responsePut.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responsePut2.StatusCode);
        Assert.Equal("Test2", newCharacter?.Name);

    }

    [Fact]
    public async Task T1_AddTwice()
    {
        var client = _factory.CreateClient();
        await RemoveAllEntries(client);
        var character = new SwCharacter()
        {
            Name = "TestCharacterForDoubleAdd",
            Faction = "Test",
            Homeworld = "Test",
            Species = "Test"
        };

        var responseAdd1 = await client.PostAsJsonAsync("/sw-characters", character);
        var responseAdd2 = await client.PostAsJsonAsync("/sw-characters", character);

        var charactersAdded = await client.GetAsync("/sw-characters?name=TestCharacterForDoubleAdd");
        var characters = await charactersAdded.Content.ReadFromJsonAsync<List<SwCharacter>>();

        Assert.Equal(HttpStatusCode.Created, responseAdd1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, responseAdd2.StatusCode);
        Assert.Equal(2, characters?.Count);
    }

    [Fact]
    public async Task T1_GetNonExistingCharacter()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/sw-characters/999999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task T1_UpdateNonExistingCharacter()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/sw-characters/999999999", new SwCharacter
        {
            Name = "Test",
            Faction = "Test",
            Homeworld = "Test",
            Species = "Test"
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task T1_DeleteNonExistingCharacter()
    {
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/sw-characters/999999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task T1_GetCharactersByParameters()
    {
        var client = _factory.CreateClient();
        await RemoveAllEntries(client);
        SwCharacter character = new SwCharacter()
        {
            Name = "GetByParameters",
            Faction = "GetByParameters2",
            Homeworld = "GetByParameters3",
            Species = "GetByParameters4"
        };

        var responseAdd = await client.PostAsJsonAsync("/sw-characters", character);
        var responseAdd2 = await client.PostAsJsonAsync("/sw-characters", character);
        var responseAdd3 = await client.PostAsJsonAsync("/sw-characters", character);

        var responseGetNames = await client.GetAsync("sw-characters?name=GetByParameters");
        var responseGetFactions = await client.GetAsync("sw-characters?faction=GetByParameters2");
        var responseGetHomeworlds = await client.GetAsync("sw-characters?homeland=GetByParameters3");
        var responseGetSpecies = await client.GetAsync("sw-characters?species=GetByParameters4");

        var charactersNames = await responseGetNames.Content.ReadFromJsonAsync<List<SwCharacter>>();
        var charactersFactions = await responseGetFactions.Content.ReadFromJsonAsync<List<SwCharacter>>();
        var charactersHomeworlds = await responseGetHomeworlds.Content.ReadFromJsonAsync<List<SwCharacter>>();
        var charactersSpecies = await responseGetSpecies.Content.ReadFromJsonAsync<List<SwCharacter>>();

        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        Assert.Equal(HttpStatusCode.Created, responseAdd2.StatusCode);
        Assert.Equal(HttpStatusCode.Created, responseAdd3.StatusCode);
        Assert.Equal(3, charactersNames?.Count);
        Assert.Equal(3, charactersFactions?.Count);
        Assert.Equal(3, charactersHomeworlds?.Count);
        Assert.Equal(3, charactersSpecies?.Count);
    }

    [Fact]
    public async Task T1_GetCharacterWithMixedParameters()
    {
        var client = _factory.CreateClient();
        await RemoveAllEntries(client);

        SwCharacter character = new SwCharacter()
        {
            Name = "GetByParameters",
            Faction = "GetByParameters2",
            Homeworld = "GetByParameters3",
            Species = "GetByParameters4"
        };

        SwCharacter character2 = new SwCharacter()
        {
            Name = "AlsoGetByParameters",
            Faction = "GetByParameters2",
            Homeworld = "GetByParameters3",
            Species = "GetByParameters4"
        };
        var responseAdd = await client.PostAsJsonAsync("/sw-characters", character);
        var responseAdd2 = await client.PostAsJsonAsync("/sw-characters", character2);

        var responseGetTwo = await client.GetAsync("sw-characters?species=GetByParameters4&homeland=GetByParameters3");
        var responseGetOne = await client.GetAsync("sw-characters?name=AlsoGetByParameters&homeland=GetByParameters3&species=GetByParameters4");

        var charactersTwo = await responseGetTwo.Content.ReadFromJsonAsync<List<SwCharacter>>();
        var charactersOne = await responseGetOne.Content.ReadFromJsonAsync<List<SwCharacter>>();

        Assert.Equal(HttpStatusCode.Created, responseAdd.StatusCode);
        Assert.Equal(HttpStatusCode.Created, responseAdd2.StatusCode);
        Assert.Equal(2, charactersTwo?.Count);
        Assert.Equal(1, charactersOne?.Count);
    }


    // Universal methods

    private async Task<SwCharacter?> GetOneCharacter(HttpClient client)
    {
        var response =  await client.GetAsync("/sw-characters");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var characters = await response.Content.ReadFromJsonAsync<List<SwCharacter>>();
            return characters?.FirstOrDefault();
        }
        return null;

    }

    private async Task RemoveAllEntries(HttpClient client)
    {
        var response =  await client.GetAsync("/sw-characters");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var characters = await response.Content.ReadFromJsonAsync<List<SwCharacter>>();
            if (characters is not null)
            {
                foreach (var ch in characters)
                {
                    await client.DeleteAsync($"/sw-characters/{ch.Id}");
                }
            }
        }
    }

}