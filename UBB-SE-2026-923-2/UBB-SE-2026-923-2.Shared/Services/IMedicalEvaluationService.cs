namespace UBB_SE_2026_923_2.Services
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IMedicalEvaluationService
    {
        List<Doctor> GetAllDoctors();

        List<Appointment> GetAppointmentsByDoctor(int doctorId);

        List<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId);

        void SaveEvaluation(MedicalEvaluation record);

        void UpdateEvaluation(MedicalEvaluation record);

        void DeleteEvaluation(int evaluationId);

        bool IsDoctorFatigued(string doctorId);

        void RaiseFatigueIntervention(int doctorId, string doctorName);

        string? CheckMedicineConflict(string patientId, string medications);

        MedicalEvaluation? GetEvaluationById(int evaluationId);
    }
}
