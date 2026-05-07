namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Wire-friendly projection used by the top-items endpoint. Mirrors the
    /// Tuple&lt;int, string, int&gt; shape of the repository contract but as a
    /// record so it serializes cleanly through the Web API.
    /// </summary>
    public sealed record ItemPopularitySummary(int Id, string Name, int OrdersCount);
}
