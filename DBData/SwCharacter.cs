using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBData;
[Table("SW_CHARACTERS")]
public class SwCharacter
{
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

    public static SwCharacter Parse(string Name, string Faction, string Homewold, string Species) =>
        new() {
            Name = Name,
            Faction = Faction,
            Homeworld = Homewold,
            Species = Species
        };
}

