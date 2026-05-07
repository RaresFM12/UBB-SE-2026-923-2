using System;
using System.Text.Json.Serialization;

namespace UBB_SE_2026_923_2.Models
{
    public class PharmacyHandover
    {
        public int PharmacistId { get; set; }
        public DateTime HandoverDate { get; set; }

        // ---- EF Core navigation property (persisted) ----
        // [JsonIgnore]: PharmacistId is enough across the wire; the full
        // Staff entity would balloon payload size and pull in TPH polymorphism.
        [JsonIgnore]
        public Staff? Pharmacist { get; set; }
    }
}
