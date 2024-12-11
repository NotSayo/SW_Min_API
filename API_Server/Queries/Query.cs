using DBData;

namespace API_Server.Queries;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<SwCharacter> GetCharacters([Service] SwContext context) =>
        context.SW_CHARACTERS;

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<SwCharacter> GetCharacterById([Service] SwContext context, int id) =>
        context.SW_CHARACTERS.Where(c => c.Id == id);
}