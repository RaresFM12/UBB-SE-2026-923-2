using System;

namespace UBB_SE_2026_923_2.Models
{
    public class PharmacyHandover
    {
        public int PharmacistId { get; set; }
        public DateTime HandoverDate { get; set; }

        // ---- EF Core navigation property (persisted) ----
        public Staff? Pharmacist { get; set; }
    }
}
