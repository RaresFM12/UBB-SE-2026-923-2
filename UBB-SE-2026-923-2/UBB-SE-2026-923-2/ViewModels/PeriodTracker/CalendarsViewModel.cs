using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    public class CalendarsViewModel : INotifyPropertyChanged
    {
        private const int NoPremenstrualSyndromeOption = 0;
        private const int MildPremenstrualSyndromeOption = 1;
        private const int ModeratePremenstrualSyndromeOption = 2;

        private const int LowFertilityMaximumPeriodLength = 9;
        private const int LowFertilityStartOffsetDays = 1;
        private const int LowFertilityEndOffsetDays = 8;

        private const int OvulationStartOffsetDays = 11;
        private const int OvulationEndOffsetDays = 15;

        private const int MinimumMildPmsDaysBeforePeriod = 1;
        private const int MaximumMildPmsDaysBeforePeriodExclusive = 4;

        private const int MinimumModeratePmsDaysBeforePeriod = 4;
        private const int MaximumModeratePmsDaysBeforePeriodExclusive = 8;

        private const int MinimumSeverePmsDaysBeforePeriod = 7;
        private const int MaximumSeverePmsDaysBeforePeriodExclusive = 15;

        private const string NoLowFertilityDaysText = "Low Fertility Days: No such days";
        private const string NoPmsDaysText = "PMS Days: No such days";

        private const string CurrentOvulationStatusNow = "Now";
        private const string CurrentOvulationStatusPassed = "Passed";
        private const string CurrentOvulationStatusUpcoming = "Upcoming";

        private const string MenstrualPhaseText = "Menstrual Phase";
        private const string FollicularPhaseText = "Follicular Phase";
        private const string OvulationPhaseText = "Ovulation Phase";
        private const string LutealPhaseText = "Luteal Phase";
        private const string NotCalculatedPhaseText = "Not calculated for this month";

        private readonly Random randomNumberGenerator = new Random();

        private DateTime configuredStartPeriodDate;
        private int configuredCycleDays;
        private int configuredPeriodLengthInDays;
        private int configuredPremenstrualSyndromeOption;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private DateTime currentDate;
        public DateTime CurrentDate
        {
            get => currentDate;
            set
            {
                if (currentDate == value)
                {
                    return;
                }

                currentDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime CurrentBeginningPeriodDate { get; private set; }
        public DateTime CurrentEndPeriodDate { get; private set; }
        public DateTime CurrentBeginningLowFertilityDate { get; private set; }
        public DateTime CurrentEndLowFertilityDate { get; private set; }
        public DateTime CurrentBeginningOvulationDate { get; private set; }
        public DateTime CurrentEndOvulationDate { get; private set; }
        public DateTime CurrentBeginningPmsDate { get; private set; }
        public DateTime CurrentEndPmsDate { get; private set; }

        public bool IsInMenstrualPhase =>
            DateTime.Today >= CurrentBeginningPeriodDate &&
            DateTime.Today <= CurrentEndPeriodDate;

        private string currentMonth;
        public string CurrentMonth
        {
            get => currentMonth;
            set
            {
                if (currentMonth == value)
                {
                    return;
                }

                currentMonth = value;
                OnPropertyChanged();
            }
        }

        private string periodInterval;
        public string PeriodInterval
        {
            get => periodInterval;
            set
            {
                if (periodInterval == value)
                {
                    return;
                }

                periodInterval = value;
                OnPropertyChanged();
            }
        }

        private string lowFertilityInterval;
        public string LowFertilityInterval
        {
            get => lowFertilityInterval;
            set
            {
                if (lowFertilityInterval == value)
                {
                    return;
                }

                lowFertilityInterval = value;
                OnPropertyChanged();
            }
        }

        private string ovulationInterval;
        public string OvulationInterval
        {
            get => ovulationInterval;
            set
            {
                if (ovulationInterval == value)
                {
                    return;
                }

                ovulationInterval = value;
                OnPropertyChanged();
            }
        }

        private string premenstrualSyndromeInterval;
        public string PremenstrualSyndromeInterval
        {
            get => premenstrualSyndromeInterval;
            set
            {
                if (premenstrualSyndromeInterval == value)
                {
                    return;
                }

                premenstrualSyndromeInterval = value;
                OnPropertyChanged();
            }
        }

        private string pastOvulationString;
        public string PastOvulationString
        {
            get => pastOvulationString;
            set
            {
                if (pastOvulationString == value)
                {
                    return;
                }

                pastOvulationString = value;
                OnPropertyChanged();
            }
        }

        private string nextPeriodDateString;
        public string NextPeriodDateString
        {
            get => nextPeriodDateString;
            set
            {
                if (nextPeriodDateString == value)
                {
                    return;
                }

                nextPeriodDateString = value;
                OnPropertyChanged();
            }
        }

        private string currentPhaseString;
        public string CurrentPhaseString
        {
            get => currentPhaseString;
            set
            {
                if (currentPhaseString == value)
                {
                    return;
                }

                currentPhaseString = value;
                OnPropertyChanged();
            }
        }

        private string literallyTodayString;
        public string LiterallyTodayString
        {
            get => literallyTodayString;
            set
            {
                if (literallyTodayString == value)
                {
                    return;
                }

                literallyTodayString = value;
                OnPropertyChanged();
            }
        }

        private string nextPeriodDistanceString;
        public string NextPeriodDistanceString
        {
            get => nextPeriodDistanceString;
            set
            {
                if (nextPeriodDistanceString == value)
                {
                    return;
                }

                nextPeriodDistanceString = value;
                OnPropertyChanged();
            }
        }

        private string currentOvulationDateString;
        public string CurrentOvulationDateString
        {
            get => currentOvulationDateString;
            set
            {
                if (currentOvulationDateString == value)
                {
                    return;
                }

                currentOvulationDateString = value;
                OnPropertyChanged();
            }
        }

        public void CalculatePeriodTracker(DateTime startDate, int cycleDays, int periodLengthInDays, int premenstrualSyndromeOption)
        {
            configuredStartPeriodDate = startDate.Date;
            configuredCycleDays = cycleDays;
            configuredPeriodLengthInDays = periodLengthInDays;
            configuredPremenstrualSyndromeOption = premenstrualSyndromeOption;

            CurrentDate = DateTime.Today;
            CurrentBeginningPeriodDate = configuredStartPeriodDate;

            while (CurrentBeginningPeriodDate.AddDays(configuredCycleDays) <= DateTime.Today)
            {
                CurrentBeginningPeriodDate = CurrentBeginningPeriodDate.AddDays(configuredCycleDays);
            }

            while (CurrentBeginningPeriodDate > DateTime.Today)
            {
                CurrentBeginningPeriodDate = CurrentBeginningPeriodDate.AddDays(-configuredCycleDays);
            }

            RecalculateDisplayValues();
        }

        public void UpdatePeriodTracker(bool navigateToNextCycle)
        {
            if (configuredCycleDays <= 0)
            {
                return;
            }

            int cycleOffsetInDays = navigateToNextCycle ? configuredCycleDays : -configuredCycleDays;
            CurrentBeginningPeriodDate = CurrentBeginningPeriodDate.AddDays(cycleOffsetInDays);

            RecalculateDisplayValues();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RecalculateDisplayValues()
        {
            LiterallyTodayString = DateTime.Today.ToString("d");
            CurrentMonth = CurrentBeginningPeriodDate.ToString("MMMM", CultureInfo.InvariantCulture);

            CurrentEndPeriodDate = CurrentBeginningPeriodDate.AddDays(configuredPeriodLengthInDays);

            PeriodInterval = BuildIntervalText("Period Days", CurrentBeginningPeriodDate, CurrentEndPeriodDate);

            DateTime nextPeriodDate = CurrentBeginningPeriodDate.AddDays(configuredCycleDays);
            NextPeriodDateString = nextPeriodDate.ToString("d");

            double remainingDaysUntilNextPeriod = Math.Max(
                0,
                Math.Ceiling((nextPeriodDate - DateTime.Today).TotalDays));

            NextPeriodDistanceString = $"In {remainingDaysUntilNextPeriod} days";

            RecalculateLowFertilityDates();
            RecalculateOvulationDates();
            RecalculatePmsDates();
            RecalculateCurrentPhase();

            OnPropertyChanged(nameof(IsInMenstrualPhase));
        }

        private void RecalculateLowFertilityDates()
        {
            if (configuredPeriodLengthInDays < LowFertilityMaximumPeriodLength)
            {
                CurrentBeginningLowFertilityDate = CurrentEndPeriodDate.AddDays(LowFertilityStartOffsetDays);
                CurrentEndLowFertilityDate = CurrentBeginningPeriodDate.AddDays(LowFertilityEndOffsetDays);

                LowFertilityInterval = BuildIntervalText(
                    "Low Fertility Days",
                    CurrentBeginningLowFertilityDate,
                    CurrentEndLowFertilityDate);

                return;
            }

            LowFertilityInterval = NoLowFertilityDaysText;
        }

        private void RecalculateOvulationDates()
        {
            CurrentBeginningOvulationDate = CurrentBeginningPeriodDate.AddDays(OvulationStartOffsetDays);
            CurrentEndOvulationDate = CurrentBeginningPeriodDate.AddDays(OvulationEndOffsetDays);

            OvulationInterval = BuildIntervalText(
                "Ovulation Days",
                CurrentBeginningOvulationDate,
                CurrentEndOvulationDate);

            CurrentOvulationDateString = CurrentBeginningOvulationDate.ToString("d");

            if (DateTime.Today >= CurrentBeginningOvulationDate && DateTime.Today <= CurrentEndOvulationDate)
            {
                PastOvulationString = CurrentOvulationStatusNow;
            }
            else if (DateTime.Today > CurrentEndOvulationDate)
            {
                PastOvulationString = CurrentOvulationStatusPassed;
            }
            else
            {
                PastOvulationString = CurrentOvulationStatusUpcoming;
            }
        }

        private void RecalculatePmsDates()
        {
            if (configuredPremenstrualSyndromeOption == NoPremenstrualSyndromeOption)
            {
                PremenstrualSyndromeInterval = NoPmsDaysText;
                return;
            }

            CurrentBeginningPmsDate = CurrentBeginningPeriodDate.AddDays(configuredCycleDays - 1);
            CurrentBeginningPmsDate = CurrentBeginningPmsDate.AddDays(-GetPmsOffsetInDays(configuredPremenstrualSyndromeOption));
            CurrentEndPmsDate = CurrentBeginningPeriodDate.AddDays(configuredCycleDays);

            PremenstrualSyndromeInterval = BuildIntervalText("PMS Days", CurrentBeginningPmsDate, CurrentEndPmsDate);
        }

        private int GetPmsOffsetInDays(int premenstrualSyndromeOption)
        {
            if (premenstrualSyndromeOption == MildPremenstrualSyndromeOption)
            {
                return randomNumberGenerator.Next(
                    MinimumMildPmsDaysBeforePeriod,
                    MaximumMildPmsDaysBeforePeriodExclusive);
            }

            if (premenstrualSyndromeOption == ModeratePremenstrualSyndromeOption)
            {
                return randomNumberGenerator.Next(
                    MinimumModeratePmsDaysBeforePeriod,
                    MaximumModeratePmsDaysBeforePeriodExclusive);
            }

            return randomNumberGenerator.Next(
                MinimumSeverePmsDaysBeforePeriod,
                MaximumSeverePmsDaysBeforePeriodExclusive);
        }

        private void RecalculateCurrentPhase()
        {
            DateTime today = DateTime.Today;
            DateTime nextCycleBeginningDate = CurrentBeginningPeriodDate.AddDays(configuredCycleDays);

            if (today >= CurrentBeginningPeriodDate && today <= CurrentEndPeriodDate)
            {
                CurrentPhaseString = MenstrualPhaseText;
            }
            else if (today > CurrentEndPeriodDate && today < CurrentBeginningOvulationDate)
            {
                CurrentPhaseString = FollicularPhaseText;
            }
            else if (today >= CurrentBeginningOvulationDate && today <= CurrentEndOvulationDate)
            {
                CurrentPhaseString = OvulationPhaseText;
            }
            else if (today > CurrentEndOvulationDate && today < nextCycleBeginningDate)
            {
                CurrentPhaseString = LutealPhaseText;
            }
            else
            {
                CurrentPhaseString = NotCalculatedPhaseText;
            }
        }

        private static string BuildIntervalText(string label, DateTime startDate, DateTime endDate)
        {
            return $"{label}: {startDate.Day} {startDate:MMMM} {startDate.Year} - " +
                   $"{endDate.Day} {endDate:MMMM} {endDate.Year}";
        }
    }
}