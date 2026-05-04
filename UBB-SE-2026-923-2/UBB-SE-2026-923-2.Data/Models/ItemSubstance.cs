namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Active substance attached to an <see cref="Item"/> with a concentration.
    /// Composite key (ItemId, SubstanceName). Replaces the legacy
    /// <see cref="Item.ActiveSubstances"/> dictionary.
    /// </summary>
    public class ItemSubstance
    {
        public int ItemId { get; set; }
        public string SubstanceName { get; set; } = string.Empty;
        public float Concentration { get; set; }

        public Item? Item { get; set; }
        public Substance? Substance { get; set; }
    }
}
