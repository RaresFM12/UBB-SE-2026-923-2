using System.Collections.Generic;

namespace UBB_SE_2026_923_2.Models
{
    public class Substance
    {
        public string Name { get; set; } = string.Empty;
        public float LethalDose { get; set; }
        public string Description { get; set; } = string.Empty;

        // ---- EF Core navigation collection (persisted) ----
        public ICollection<ItemSubstance> ItemSubstanceEntries { get; set; } = new List<ItemSubstance>();

        // Parameterless constructor required by EF Core when materializing entities.
        public Substance()
        {
        }

        public Substance(string name, float lethalDose, string description)
        {
            Name = name;
            LethalDose = lethalDose;
            Description = description;
        }
    }
}
