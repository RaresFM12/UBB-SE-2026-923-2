namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Active substance attached to an <see cref="Item"/> with a concentration.
    /// Surrogate primary key Id replaces the old composite (ItemId, SubstanceName) PK.
    /// FK columns are maintained as EF Core shadow properties.
    /// </summary>
    public class ItemSubstance
    {
        // Surrogate primary key — replaces the old composite (ItemId, SubstanceName) PK.
        public int Id { get; set; }

        public float Concentration { get; set; }

        // EF Core navigation properties — persisted via shadow FK columns
        // "ItemId" and "SubstanceName".
        public Item Item { get; set; } = null!;

        public Substance Substance { get; set; } = null!;
    }
}
