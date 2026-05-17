namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IPeriodTrackerService
    {
        User GetCurrentUser();

        PeriodTrackerState GetTrackerState();

        PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int monthOffset);

        Dictionary<int, Tuple<string, bool>> GetNotes();

        int GetMaxNoteId();

        void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption);

        void AddNote(string noteBody);

        void UpdateNote(int noteId, string noteBody, bool isDone);

        void DeleteNote(int noteId);

        void SaveCurrentUser();
    }
}