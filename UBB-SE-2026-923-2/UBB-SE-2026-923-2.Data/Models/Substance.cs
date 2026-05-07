namespace UBB_SE_2026_923_2.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class Substance
    {
        public string Name { get; set; } = string.Empty;

        public float LethalDose { get; set; }

        public string Description { get; set; } = string.Empty;

        // ---- EF Core navigation collection (persisted) ----
        // [JsonIgnore]: would create cycles back through Item over the wire.
        [JsonIgnore]
        public ICollection<ItemSubstance> ItemSubstanceEntries { get; set; } = new List<ItemSubstance>();

        // Parameterless constructor required by EF Core when materializing entities.
        public Substance()
        {
        }

        public Substance(string name, float lethalDose, string description)
        {
            this.Name = name;
            this.LethalDose = lethalDose;
            this.Description = description;
        }
    }
}
