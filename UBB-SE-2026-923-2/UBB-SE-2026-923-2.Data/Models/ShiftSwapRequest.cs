using System;
using System.Text.Json.Serialization;

namespace UBB_SE_2026_923_2.Models
{
    public enum ShiftSwapRequestStatus
    {
        PENDING,
        ACCEPTED,
        REJECTED,
        CANCELLED
    }

    public class ShiftSwapRequest
    {
        public int SwapId { get; set; }
        public int ShiftId { get; set; }
        public int RequesterId { get; set; }
        public int ColleagueId { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public ShiftSwapRequestStatus Status { get; set; } = ShiftSwapRequestStatus.PENDING;

        // ---- EF Core navigation properties (persisted) ----
        // [JsonIgnore]: ShiftId/RequesterId/ColleagueId already carry the FKs;
        // the full nested entities create cycles back through Staff.
        [JsonIgnore]
        public Shift? Shift { get; set; }
        [JsonIgnore]
        public Staff? Requester { get; set; }
        [JsonIgnore]
        public Staff? Colleague { get; set; }

        public ShiftSwapRequest()
        {
        }

        public ShiftSwapRequest(int swapId, int shiftId, int requesterId, int colleagueId)
        {
            SwapId = swapId;
            ShiftId = shiftId;
            RequesterId = requesterId;
            ColleagueId = colleagueId;
            RequestedAt = DateTime.UtcNow;
            Status = ShiftSwapRequestStatus.PENDING;
        }
    }
}