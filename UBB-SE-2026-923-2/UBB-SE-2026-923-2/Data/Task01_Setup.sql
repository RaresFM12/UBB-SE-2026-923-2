CREATE TABLE Medical_Evaluations (
    EvaluationID INT PRIMARY KEY IDENTITY(1,1),
    PatientID INT NOT NULL,           -- Link to Patient Management Team data
    DoctorID INT NOT NULL,            -- Link to Staff/Login ID
    Symptoms NVARCHAR(MAX),
    DiagnosisResult NVARCHAR(MAX),
    MedsList NVARCHAR(MAX),           -- Shared with Pharmacist Team
    DoctorNotes NVARCHAR(MAX),        -- Instructions for Pharmacist/Patient
    EvaluationDate DATETIME DEFAULT GETDATE(),
    Source NVARCHAR(50) DEFAULT 'PATIENT' -- Can be 'ER' or 'PATIENT'
);