namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

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

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        private DateTime currentDate;

        public DateTime CurrentDate
        {
            get => this.currentDate;
            set
            {
                if (this.currentDate == value)
                {
                    return;
                }

                this.currentDate = value;
                this.OnPropertyChanged();
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
            DateTime.Today >= this.CurrentBeginningPeriodDate &&
            DateTime.Today <= this.CurrentEndPeriodDate;

        private string currentMonth;

        public string CurrentMonth
        {
            get => this.currentMonth;
            set
            {
                if (this.currentMonth == value)
                {
                    return;
                }

                this.currentMonth = value;
                this.OnPropertyChanged();
            }
        }

        private string periodInterval;

        public string PeriodInterval
        {
            get => this.periodInterval;
            set
            {
                if (this.periodInterval == value)
                {
                    return;
                }

                this.periodInterval = value;
                this.OnPropertyChanged();
            }
        }

        private string lowFertilityInterval;

        public string LowFertilityInterval
        {
            get => this.lowFertilityInterval;
            set
            {
                if (this.lowFertilityInterval == value)
                {
                    return;
                }

                this.lowFertilityInterval = value;
                this.OnPropertyChanged();
            }
        }

        private string ovulationInterval;

        public string OvulationInterval
        {
            get => this.ovulationInterval;
            set
            {
                if (this.ovulationInterval == value)
                {
                    return;
                }

                this.ovulationInterval = value;
                this.OnPropertyChanged();
            }
        }

        private string premenstrualSyndromeInterval;

        public string PremenstrualSyndromeInterval
        {
            get => this.premenstrualSyndromeInterval;
            set
            {
                if (this.premenstrualSyndromeInterval == value)
                {
                    return;
                }

                this.premenstrualSyndromeInterval = value;
                this.OnPropertyChanged();
            }
        }

        private string pastOvulationString;

        public string PastOvulationString
        {
            get => this.pastOvulationString;
            set
            {
                if (this.pastOvulationString == value)
                {
                    return;
                }

                this.pastOvulationString = value;
                this.OnPropertyChanged();
            }
        }

        private string nextPeriodDateString;

        public string NextPeriodDateString
        {
            get => this.nextPeriodDateString;
            set
            {
                if (this.nextPeriodDateString == value)
                {
                    return;
                }

                this.nextPeriodDateString = value;
                this.OnPropertyChanged();
            }
        }

        private string currentPhaseString;

        public string CurrentPhaseString
        {
            get => this.currentPhaseString;
            set
            {
                if (this.currentPhaseString == value)
                {
                    return;
                }

                this.currentPhaseString = value;
                this.OnPropertyChanged();
            }
        }

        private string literallyTodayString;

        public string LiterallyTodayString
        {
            get => this.literallyTodayString;
            set
            {
                if (this.literallyTodayString == value)
                {
                    return;
                }

                this.literallyTodayString = value;
                this.OnPropertyChanged();
            }
        }

        private string nextPeriodDistanceString;

        public string NextPeriodDistanceString
        {
            get => this.nextPeriodDistanceString;
            set
            {
                if (this.nextPeriodDistanceString == value)
                {
                    return;
                }

                this.nextPeriodDistanceString = value;
                this.OnPropertyChanged();
            }
        }

        private string currentOvulationDateString;

        public string CurrentOvulationDateString
        {
            get => this.currentOvulationDateString;
            set
            {
                if (this.currentOvulationDateString == value)
                {
                    return;
                }

                this.currentOvulationDateString = value;
                this.OnPropertyChanged();
            }
        }

        public void CalculatePeriodTracker(DateTime startDate, int cycleDays, int periodLengthInDays, int premenstrualSyndromeOption)
        {
            this.configuredStartPeriodDate = startDate.Date;
            this.configuredCycleDays = cycleDays;
            this.configuredPeriodLengthInDays = periodLengthInDays;
            this.configuredPremenstrualSyndromeOption = premenstrualSyndromeOption;

            this.CurrentDate = DateTime.Today;
            this.CurrentBeginningPeriodDate = this.configuredStartPeriodDate;

            while (this.CurrentBeginningPeriodDate.AddDays(this.configuredCycleDays) <= DateTime.Today)
            {
                this.CurrentBeginningPeriodDate = this.CurrentBeginningPeriodDate.AddDays(this.configuredCycleDays);
            }

            while (this.CurrentBeginningPeriodDate > DateTime.Today)
            {
                this.CurrentBeginningPeriodDate = this.CurrentBeginningPeriodDate.AddDays(-this.configuredCycleDays);
            }

            this.RecalculateDisplayValues();
        }

        public void UpdatePeriodTracker(bool navigateToNextCycle)
        {
            if (this.configuredCycleDays <= 0)
            {
                return;
            }

            int cycleOffsetInDays = navigateToNextCycle ? this.configuredCycleDays : -this.configuredCycleDays;
            this.CurrentBeginningPeriodDate = this.CurrentBeginningPeriodDate.AddDays(cycleOffsetInDays);

            this.RecalculateDisplayValues();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RecalculateDisplayValues()
        {
            this.LiterallyTodayString = DateTime.Today.ToString("d");
            this.CurrentMonth = this.CurrentBeginningPeriodDate.ToString("MMMM", CultureInfo.InvariantCulture);

            this.CurrentEndPeriodDate = this.CurrentBeginningPeriodDate.AddDays(this.configuredPeriodLengthInDays);

            this.PeriodInterval = BuildIntervalText("Period Days", this.CurrentBeginningPeriodDate, this.CurrentEndPeriodDate);

            DateTime nextPeriodDate = this.CurrentBeginningPeriodDate.AddDays(this.configuredCycleDays);
            this.NextPeriodDateString = nextPeriodDate.ToString("d");

            double remainingDaysUntilNextPeriod = Math.Max(
                0,
                Math.Ceiling((nextPeriodDate - DateTime.Today).TotalDays));

            this.NextPeriodDistanceString = $"In {remainingDaysUntilNextPeriod} days";

            this.RecalculateLowFertilityDates();
            this.RecalculateOvulationDates();
            this.RecalculatePmsDates();
            this.RecalculateCurrentPhase();

            this.OnPropertyChanged(nameof(this.IsInMenstrualPhase));
        }

        private void RecalculateLowFertilityDates()
        {
            if (this.configuredPeriodLengthInDays < LowFertilityMaximumPeriodLength)
            {
                this.CurrentBeginningLowFertilityDate = this.CurrentEndPeriodDate.AddDays(LowFertilityStartOffsetDays);
                this.CurrentEndLowFertilityDate = this.CurrentBeginningPeriodDate.AddDays(LowFertilityEndOffsetDays);

                this.LowFertilityInterval = BuildIntervalText(
                    "Low Fertility Days",
                    this.CurrentBeginningLowFertilityDate,
                    this.CurrentEndLowFertilityDate);

                return;
            }

            this.LowFertilityInterval = NoLowFertilityDaysText;
        }

        private void RecalculateOvulationDates()
        {
            this.CurrentBeginningOvulationDate = this.CurrentBeginningPeriodDate.AddDays(OvulationStartOffsetDays);
            this.CurrentEndOvulationDate = this.CurrentBeginningPeriodDate.AddDays(OvulationEndOffsetDays);

            this.OvulationInterval = BuildIntervalText(
                "Ovulation Days",
                this.CurrentBeginningOvulationDate,
                this.CurrentEndOvulationDate);

            this.CurrentOvulationDateString = this.CurrentBeginningOvulationDate.ToString("d");

            if (DateTime.Today >= this.CurrentBeginningOvulationDate && DateTime.Today <= this.CurrentEndOvulationDate)
            {
                this.PastOvulationString = CurrentOvulationStatusNow;
            }
            else if (DateTime.Today > this.CurrentEndOvulationDate)
            {
                this.PastOvulationString = CurrentOvulationStatusPassed;
            }
            else
            {
                this.PastOvulationString = CurrentOvulationStatusUpcoming;
            }
        }

        private void RecalculatePmsDates()
        {
            if (this.configuredPremenstrualSyndromeOption == NoPremenstrualSyndromeOption)
            {
                this.PremenstrualSyndromeInterval = NoPmsDaysText;
                return;
            }

            this.CurrentBeginningPmsDate = this.CurrentBeginningPeriodDate.AddDays(this.configuredCycleDays - 1);
            this.CurrentBeginningPmsDate = this.CurrentBeginningPmsDate.AddDays(-this.GetPmsOffsetInDays(this.configuredPremenstrualSyndromeOption));
            this.CurrentEndPmsDate = this.CurrentBeginningPeriodDate.AddDays(this.configuredCycleDays);

            this.PremenstrualSyndromeInterval = BuildIntervalText("PMS Days", this.CurrentBeginningPmsDate, this.CurrentEndPmsDate);
        }

        private int GetPmsOffsetInDays(int premenstrualSyndromeOption)
        {
            if (premenstrualSyndromeOption == MildPremenstrualSyndromeOption)
            {
                return this.randomNumberGenerator.Next(
                    MinimumMildPmsDaysBeforePeriod,
                    MaximumMildPmsDaysBeforePeriodExclusive);
            }

            if (premenstrualSyndromeOption == ModeratePremenstrualSyndromeOption)
            {
                return this.randomNumberGenerator.Next(
                    MinimumModeratePmsDaysBeforePeriod,
                    MaximumModeratePmsDaysBeforePeriodExclusive);
            }

            return this.randomNumberGenerator.Next(
                MinimumSeverePmsDaysBeforePeriod,
                MaximumSeverePmsDaysBeforePeriodExclusive);
        }

        private void RecalculateCurrentPhase()
        {
            DateTime today = DateTime.Today;
            DateTime nextCycleBeginningDate = this.CurrentBeginningPeriodDate.AddDays(this.configuredCycleDays);

            if (today >= this.CurrentBeginningPeriodDate && today <= this.CurrentEndPeriodDate)
            {
                this.CurrentPhaseString = MenstrualPhaseText;
            }
            else if (today > this.CurrentEndPeriodDate && today < this.CurrentBeginningOvulationDate)
            {
                this.CurrentPhaseString = FollicularPhaseText;
            }
            else if (today >= this.CurrentBeginningOvulationDate && today <= this.CurrentEndOvulationDate)
            {
                this.CurrentPhaseString = OvulationPhaseText;
            }
            else if (today > this.CurrentEndOvulationDate && today < nextCycleBeginningDate)
            {
                this.CurrentPhaseString = LutealPhaseText;
            }
            else
            {
                this.CurrentPhaseString = NotCalculatedPhaseText;
            }
        }

        private static string BuildIntervalText(string label, DateTime startDate, DateTime endDate)
        {
            return $"{label}: {startDate.Day} {startDate:MMMM} {startDate.Year} - " +
                   $"{endDate.Day} {endDate:MMMM} {endDate.Year}";
        }
    }
}