namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IEvaluationsRepository"/>. The
    /// legacy SQL schema had <c>diagnosis</c>, <c>doctor_notes</c>,
    /// <c>medications</c>, <c>source</c> and <c>assumed_risk</c> columns; the
    /// code-first model only persists <see cref="MedicalEvaluation.Symptoms"/>
    /// (mapped from <c>diagnosis</c>), <see cref="MedicalEvaluation.Notes"/>
    /// and <see cref="MedicalEvaluation.MedicationsList"/>. Source/risk fields
    /// are not part of the domain model and are dropped on write.
    /// </summary>
    public class EvaluationsRepository : IEvaluationsRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public EvaluationsRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public IReadOnlyList<MedicalEvaluation> GetAllEvaluations()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.MedicalEvaluations
                .AsNoTracking()
                .Include(medicalEvaluation => medicalEvaluation.Evaluator)
                .ToList();
        }

        public void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            Doctor? evaluatingDoctor = doctorId == 0 ? null : databaseContext.Doctors.Find(doctorId);

            var newMedicalEvaluation = new MedicalEvaluation
            {
                Evaluator = evaluatingDoctor,
                PatientId = patientId.ToString(),
                Symptoms = diagnosis,
                Notes = notes,
                MedicationsList = medications,
                EvaluationDate = DateTime.UtcNow,
            };

            databaseContext.MedicalEvaluations.Add(newMedicalEvaluation);
            databaseContext.SaveChanges();
        }

        public void UpdateEvaluation(int evaluationId, string diagnosis, string notes, string medications)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var existingEvaluation = databaseContext.MedicalEvaluations
                .FirstOrDefault(medicalEvaluation => medicalEvaluation.EvaluationID == evaluationId);

            if (existingEvaluation is null)
            {
                return;
            }

            existingEvaluation.Symptoms = diagnosis ?? string.Empty;
            existingEvaluation.Notes = notes ?? string.Empty;
            existingEvaluation.MedicationsList = medications ?? string.Empty;
            databaseContext.SaveChanges();
        }

        public void DeleteEvaluation(int evaluationId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var evaluationToRemove = databaseContext.MedicalEvaluations
                .FirstOrDefault(medicalEvaluation => medicalEvaluation.EvaluationID == evaluationId);

            if (evaluationToRemove is null)
            {
                return;
            }

            databaseContext.MedicalEvaluations.Remove(evaluationToRemove);
            databaseContext.SaveChanges();
        }
    }
}
