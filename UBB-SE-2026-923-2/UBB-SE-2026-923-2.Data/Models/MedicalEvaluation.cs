namespace UBB_SE_2026_923_2.Models
{
    using System;

    public class MedicalEvaluation
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm";

        public int EvaluationID { get; set; }

        // External patient reference. Kept as string because the rest of the
        // app (services, view models, search) treats it as free-text — there
        // is no internal Patient entity. Code-first means this becomes an
        // NVARCHAR column.
        public string PatientId { get; set; } = string.Empty;

        public string Symptoms { get; set; } = string.Empty;

        public string MedicationsList { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime EvaluationDate { get; set; }

        // ---- EF Core foreign key + navigation to Doctor (TPH subtype of Staff) ----
        public int? DoctorId { get; set; }

        public Doctor? Evaluator { get; set; }

        public string FormattedDate => this.EvaluationDate.ToString(DateFormat);

        public string FormattedTime => this.EvaluationDate.ToString(TimeFormat);
    }
}