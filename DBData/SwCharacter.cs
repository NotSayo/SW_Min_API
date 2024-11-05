using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBData;
[Table("SW_CHARACTERS")]
public class SwCharacter
{
    public SwCharacter()
    {
    }
    public SwCharacter(string name, string faction, string homeworld, string species)
    {
        Name = name;
        Faction = faction;
        Homeworld = homeworld;
        Species = species;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Faction { get; set; }
    [Required]
    public string Homeworld {get; set;}
    [Required]
    public string Species { get; set; }
}

