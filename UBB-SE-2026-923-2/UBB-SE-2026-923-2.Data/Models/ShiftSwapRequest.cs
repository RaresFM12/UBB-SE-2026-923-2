namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.Text.Json.Serialization;

    public enum ShiftSwapRequestStatus
    {
        PENDING,
        ACCEPTED,
        REJECTED,
        CANCELLED,
    }

    public class ShiftSwapRequest
    {
        public int SwapId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public ShiftSwapRequestStatus Status { get; set; } = ShiftSwapRequestStatus.PENDING;

        // EF Core navigation properties — persisted via shadow FK columns
        // "ShiftId", "RequesterId", "ColleagueId". JsonIgnore prevents the full
        // nested Staff/Shift graph from cycling back through the object tree.
        [JsonIgnore]
        public Shift Shift { get; set; } = null!;

        [JsonIgnore]
        public Staff Requester { get; set; } = null!;

        [JsonIgnore]
        public Staff Colleague { get; set; } = null!;

        public ShiftSwapRequest()
        {
        }

        public ShiftSwapRequest(int swapIdentifier, Shift shift, Staff requester, Staff colleague)
        {
            this.SwapId = swapIdentifier;
            this.Shift = shift;
            this.Requester = requester;
            this.Colleague = colleague;
            this.RequestedAt = DateTime.UtcNow;
            this.Status = ShiftSwapRequestStatus.PENDING;
        }
    }
}
