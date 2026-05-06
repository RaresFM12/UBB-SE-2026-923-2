using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
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
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public EvaluationsRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public IReadOnlyList<MedicalEvaluation> GetAllEvaluations()
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.MedicalEvaluations
                .AsNoTracking()
                .Include(e => e.Evaluator)
                .ToList();
        }

        public void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk)
        {
            using var db = dbContextFactory.CreateDbContext();

            var evaluation = new MedicalEvaluation
            {
                DoctorId = doctorId == 0 ? null : doctorId,
                PatientId = patientId.ToString(),
                Symptoms = diagnosis,
                Notes = notes,
                MedicationsList = medications,
                EvaluationDate = DateTime.UtcNow,
            };

            db.MedicalEvaluations.Add(evaluation);
            db.SaveChanges();
        }

        public void UpdateEvaluation(int evaluationId, string diagnosis, string notes, string medications)
        {
            using var db = dbContextFactory.CreateDbContext();
            var evaluation = db.MedicalEvaluations.FirstOrDefault(e => e.EvaluationID == evaluationId);
            if (evaluation is null)
            {
                return;
            }

            evaluation.Symptoms = diagnosis ?? string.Empty;
            evaluation.Notes = notes ?? string.Empty;
            evaluation.MedicationsList = medications ?? string.Empty;
            db.SaveChanges();
        }

        public void DeleteEvaluation(int evaluationId)
        {
            using var db = dbContextFactory.CreateDbContext();
            var evaluation = db.MedicalEvaluations.FirstOrDefault(e => e.EvaluationID == evaluationId);
            if (evaluation is null)
            {
                return;
            }

            db.MedicalEvaluations.Remove(evaluation);
            db.SaveChanges();
        }
    }
}
