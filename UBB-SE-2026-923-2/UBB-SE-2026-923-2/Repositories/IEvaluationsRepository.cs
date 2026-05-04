using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IEvaluationsRepository
    {
        IReadOnlyList<MedicalEvaluation> GetAllEvaluations();
        void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk);
        void DeleteEvaluation(int evaluationId);
    }
}
