namespace DBData;

public class SwRepository(SwContext db)
{

    public List<SwCharacter> GetCharacters(string? name, string faction, string homeland, string species)
    {
        var query = db.SW_CHARACTERS.AsQueryable();
        if(!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name));
        if(!string.IsNullOrWhiteSpace(faction))
            query = query.Where(s => s.Faction.Contains(faction));
        if(!string.IsNullOrWhiteSpace(homeland))
            query = query.Where(s => s.Homeworld.Contains(homeland));
        if(!string.IsNullOrWhiteSpace(species))
            query = query.Where(s => s.Species.Contains(species));
        return query.ToList();
    }
    public SwCharacter? GetCharacter(int id) => db.SW_CHARACTERS.FirstOrDefault(s => s.Id == id);

    public void AddCharacter(SwCharacter character)
    {
        character.Id = default;
        db.SW_CHARACTERS.Add(character);
        db.SaveChanges();
    }

    public bool ChangeCharacter(int id, SwCharacter character)
    {
        var oldchar = db.SW_CHARACTERS.FirstOrDefault(s => s.Id == id);
        if(oldchar == null) return false;
        oldchar.Faction = character.Faction;
        oldchar.Homeworld = character.Homeworld;
        oldchar.Name = character.Name;
        oldchar.Species = character.Species;
        db.SaveChanges();
        return true;
    }

    public bool DeleteCharacter(int id)
    {
        var oldchar = db.SW_CHARACTERS.FirstOrDefault(s => s.Id == id);
        if(oldchar == null) return false;
        db.SW_CHARACTERS.Remove(oldchar);
        db.SaveChanges();
        return true;
    }
}