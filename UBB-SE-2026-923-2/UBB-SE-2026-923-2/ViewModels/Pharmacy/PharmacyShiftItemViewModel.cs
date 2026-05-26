namespace UBB_SE_2026_923_2.ViewModels.Pharmacy;

using System;
using System.Globalization;
using UBB_SE_2026_923_2.Models;

public sealed class PharmacyShiftItemViewModel
{
    private const string EnglishCultureCode = "en-US";
    private const string TimeFormat = "HH:mm";
    private const string DayLabelFormat = "ddd, dd MMM yyyy";

    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo(EnglishCultureCode);

    public PharmacyShiftItemViewModel(Shift shift)
    {
        this.RotationAssignment = shift.Location;
        this.ShiftStartTime = shift.StartTime;
        this.ShiftEndTime = shift.EndTime;
        this.Status = shift.Status;
    }

    public string RotationAssignment { get; }

    public DateTime ShiftStartTime { get; }

    public DateTime? ShiftEndTime { get; }

    public string ShiftStartTimeText => this.ShiftStartTime.ToString(TimeFormat);

    public string ShiftEndTimeText => this.ShiftEndTime.HasValue ? this.ShiftEndTime.Value.ToString(TimeFormat) : "—";

    public string DayLabel => this.ShiftStartTime.ToString(DayLabelFormat, EnglishCulture);

    public string TimeRangeDetail =>
        $"Shift start: {this.ShiftStartTimeText}  ·  Shift end: {this.ShiftEndTimeText}  ·  Duration: {this.DurationText}";

    public string DurationText
    {
        get
        {
            if (!this.ShiftEndTime.HasValue)
            {
                return "Open-ended";
            }

            var span = this.ShiftEndTime.Value - this.ShiftStartTime;
            if (span.TotalMinutes <= 0)
            {
                return "—";
            }

            return $"{(int)span.TotalHours}h {span.Minutes}m";
        }
    }

    public string StatusDisplay
    {
        get
        {
            return this.Status switch
            {
                ShiftStatus.SCHEDULED => "Scheduled",
                ShiftStatus.ACTIVE => "Active",
                ShiftStatus.COMPLETED => "Completed",
                ShiftStatus.CANCELLED => "Cancelled",
                var other => other.ToString(),
            };
        }
    }

    private ShiftStatus Status { get; }
}
