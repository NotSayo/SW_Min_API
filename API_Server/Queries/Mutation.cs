using DBData;

namespace API_Server.Queries;

public class Mutation
{
    public async Task<SwCharacter> AddCharacter([Service] SwContext context, CharacterInput character)
    {
        SwCharacter newCharacter = new SwCharacter(character.Name, character.Faction, character.Homeworld, character.Species);
        await context.SW_CHARACTERS.AddAsync(newCharacter);
        await context.SaveChangesAsync();
        return newCharacter;
    }
}