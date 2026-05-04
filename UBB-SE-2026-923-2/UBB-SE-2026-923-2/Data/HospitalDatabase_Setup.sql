/*
  Unified setup for the merged 923-2 app.
  Target DB : HospitalDatabase  (matches AppSettings.ConnectionString)
  Run once  : sqlcmd, SSMS, or Azure Data Studio — fully idempotent.

  Sections:
    1.  Database
    2.  Hospital-side tables
    3.  Pharmacy-side tables
    4.  Salary / handover tables
    5.  Seed data
*/

-- =======================================================================
-- 1. DATABASE
-- =======================================================================
IF DB_ID(N'HospitalDatabase') IS NULL
    CREATE DATABASE HospitalDatabase;
GO

USE HospitalDatabase;
GO

-- =======================================================================
-- 2. HOSPITAL-SIDE TABLES
-- =======================================================================

IF OBJECT_ID(N'dbo.Staff', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Staff (
        staff_id            INT             NOT NULL PRIMARY KEY,
        role                NVARCHAR(50)    NOT NULL,
        first_name          NVARCHAR(100)   NOT NULL,
        last_name           NVARCHAR(100)   NOT NULL,
        contact_info        NVARCHAR(200)   NULL,
        is_available        BIT             NOT NULL CONSTRAINT DF_Staff_IsAvailable DEFAULT (1),
        license_number      NVARCHAR(100)   NULL,
        specialization      NVARCHAR(100)   NULL,
        status              NVARCHAR(50)    NULL,
        certification       NVARCHAR(100)   NULL,
        years_of_experience INT             NOT NULL CONSTRAINT DF_Staff_Years DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.Shifts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shifts (
        shift_id    INT             IDENTITY(1,1) PRIMARY KEY,
        staff_id    INT             NOT NULL,
        location    NVARCHAR(100)   NULL,
        start_time  DATETIME2       NOT NULL,
        end_time    DATETIME2       NOT NULL,
        status      NVARCHAR(50)    NOT NULL,
        is_active   BIT             NOT NULL CONSTRAINT DF_Shifts_IsActive DEFAULT (0),
        CONSTRAINT FK_Shifts_Staff FOREIGN KEY (staff_id) REFERENCES dbo.Staff (staff_id)
    );
    CREATE INDEX IX_Shifts_Staff_Status ON dbo.Shifts (staff_id, status);
END
GO

IF OBJECT_ID(N'dbo.Appointments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments (
        appointment_id  INT             IDENTITY(1,1) PRIMARY KEY,
        patient_id      INT             NOT NULL,
        doctor_id       INT             NOT NULL,
        start_time      DATETIME2       NOT NULL,
        end_time        DATETIME2       NOT NULL,
        status          NVARCHAR(50)    NOT NULL,
        CONSTRAINT FK_Appointments_Doctor FOREIGN KEY (doctor_id) REFERENCES dbo.Staff (staff_id)
    );
END
GO

IF OBJECT_ID(N'dbo.Medical_Evaluations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Medical_Evaluations (
        evaluation_id   INT             IDENTITY(1,1) PRIMARY KEY,
        doctor_id       INT             NOT NULL,
        patient_id      INT             NOT NULL,
        diagnosis       NVARCHAR(MAX)   NULL,
        doctor_notes    NVARCHAR(MAX)   NULL,
        medications     NVARCHAR(MAX)   NULL,
        source          NVARCHAR(50)    NOT NULL CONSTRAINT DF_Eval_Source DEFAULT (N'PATIENT'),
        assumed_risk    BIT             NOT NULL CONSTRAINT DF_Eval_AssumedRisk DEFAULT (0),
        evaluation_date DATETIME2       NOT NULL CONSTRAINT DF_Eval_Date DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Eval_Doctor FOREIGN KEY (doctor_id) REFERENCES dbo.Staff (staff_id)
    );
END
GO

IF OBJECT_ID(N'dbo.ER_Requests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ER_Requests (
        request_id              INT             IDENTITY(1,1) PRIMARY KEY,
        specialization          NVARCHAR(100)   NOT NULL,
        [location]              NVARCHAR(100)   NOT NULL,
        created_at              DATETIME2       NOT NULL CONSTRAINT DF_ER_CreatedAt DEFAULT (SYSDATETIME()),
        [status]                NVARCHAR(50)    NOT NULL,
        assigned_doctor_id      INT             NULL,
        assigned_doctor_name    NVARCHAR(200)   NULL,
        CONSTRAINT FK_ER_Doctor FOREIGN KEY (assigned_doctor_id) REFERENCES dbo.Staff (staff_id)
    );
END
GO

IF OBJECT_ID(N'dbo.Hangouts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hangouts (
        hangout_id  INT             IDENTITY(1,1) PRIMARY KEY,
        title       NVARCHAR(100)   NOT NULL,
        description NVARCHAR(500)   NULL,
        date_time   DATETIME2       NOT NULL,
        max_staff   INT             NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Hangout_Participants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hangout_Participants (
        hangout_id  INT NOT NULL,
        staff_id    INT NOT NULL,
        CONSTRAINT PK_HangoutParticipants PRIMARY KEY (hangout_id, staff_id),
        CONSTRAINT FK_HangoutParticipants_Hangout FOREIGN KEY (hangout_id) REFERENCES dbo.Hangouts (hangout_id),
        CONSTRAINT FK_HangoutParticipants_Staff   FOREIGN KEY (staff_id)   REFERENCES dbo.Staff (staff_id)
    );
END
GO

IF OBJECT_ID(N'dbo.ShiftSwapRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShiftSwapRequests (
        swap_id         INT             IDENTITY(1,1) PRIMARY KEY,
        shift_id        INT             NOT NULL,
        requester_id    INT             NOT NULL,
        colleague_id    INT             NOT NULL,
        requested_at    DATETIME2       NOT NULL CONSTRAINT DF_Swap_RequestedAt DEFAULT (SYSDATETIME()),
        status          NVARCHAR(50)    NOT NULL,
        CONSTRAINT FK_Swap_Shift     FOREIGN KEY (shift_id)     REFERENCES dbo.Shifts (shift_id),
        CONSTRAINT FK_Swap_Requester FOREIGN KEY (requester_id) REFERENCES dbo.Staff  (staff_id),
        CONSTRAINT FK_Swap_Colleague FOREIGN KEY (colleague_id) REFERENCES dbo.Staff  (staff_id)
    );
END
GO

IF OBJECT_ID(N'dbo.High_Risk_Medicines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.High_Risk_Medicines (
        medicine_name   NVARCHAR(200)   NOT NULL PRIMARY KEY,
        warning_message NVARCHAR(500)   NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        notification_id     INT             IDENTITY(1,1) PRIMARY KEY,
        recipient_staff_id  INT             NOT NULL,
        title               NVARCHAR(200)   NOT NULL,
        message             NVARCHAR(MAX)   NOT NULL,
        created_at          DATETIME2       NOT NULL CONSTRAINT DF_Notif_CreatedAt DEFAULT (SYSDATETIME()),
        is_read             BIT             NOT NULL CONSTRAINT DF_Notif_IsRead DEFAULT (0),
        CONSTRAINT FK_Notif_Staff FOREIGN KEY (recipient_staff_id) REFERENCES dbo.Staff (staff_id)
    );
END
GO

-- Read by SalaryComputationService.CountMedicinesSoldForPharmacist
IF OBJECT_ID(N'dbo.PharmacyHandover', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PharmacyHandover (
        PharmacistID    INT             NOT NULL,
        HandoverDate    DATETIME2       NOT NULL,
        CONSTRAINT FK_Handover_Staff FOREIGN KEY (PharmacistID) REFERENCES dbo.Staff (staff_id)
    );
    CREATE INDEX IX_PharmacyHandover_Pharmacist_Date ON dbo.PharmacyHandover (PharmacistID, HandoverDate);
END
GO

-- =======================================================================
-- 3. PHARMACY-SIDE TABLES
-- =======================================================================

IF OBJECT_ID(N'dbo.Substances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Substances (
        name        VARCHAR(255)    NOT NULL PRIMARY KEY,
        lethalDose  DECIMAL(10,2)   NULL,
        description VARCHAR(200)    NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Items (
        itemId              INT             IDENTITY(1,1) PRIMARY KEY,
        name                VARCHAR(255)    NOT NULL,
        price               DECIMAL(10,2)   NULL,
        category            VARCHAR(255)    NULL,
        numberOfPills       INT             NULL,
        producer            VARCHAR(255)    NULL,
        imagePath           VARCHAR(255)    NULL,
        quantity            INT             NULL,
        label               VARCHAR(255)    NULL,
        description         VARCHAR(255)    NULL,
        discountPercentage  DECIMAL(10,2)   NULL
    );
END
GO

IF OBJECT_ID(N'dbo.ItemSubstances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemSubstances (
        itemId          INT             NOT NULL,
        name            VARCHAR(255)    NOT NULL,
        concentration   DECIMAL(10,2)   NULL,
        CONSTRAINT PK_ItemSubstances PRIMARY KEY (itemId, name),
        CONSTRAINT FK_ItemSubstances_Item      FOREIGN KEY (itemId) REFERENCES dbo.Items      (itemId),
        CONSTRAINT FK_ItemSubstances_Substance FOREIGN KEY (name)   REFERENCES dbo.Substances (name)
    );
END
GO

IF OBJECT_ID(N'dbo.ItemExpirationDates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemExpirationDates (
        itemId          INT     NOT NULL,
        expirationDate  DATE    NOT NULL,
        numberOfPacks   INT     NULL,
        CONSTRAINT PK_ItemExpirationDates PRIMARY KEY (itemId, expirationDate),
        CONSTRAINT FK_ItemExpirationDates_Item FOREIGN KEY (itemId) REFERENCES dbo.Items (itemId)
    );
END
GO

-- IMPORTANT: column order matches `INSERT INTO Users VALUES (...)` in SQLUsersRepository.AddUser
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        userId                  INT             IDENTITY(1,1) PRIMARY KEY,
        email                   VARCHAR(255)    NOT NULL UNIQUE,
        phoneNumber             VARCHAR(255)    NULL,
        passwordHash            VARCHAR(255)    NULL,
        isDisabled              BIT             NOT NULL CONSTRAINT DF_Users_IsDisabled DEFAULT (0),
        isAdmin                 BIT             NOT NULL CONSTRAINT DF_Users_IsAdmin    DEFAULT (0),
        username                VARCHAR(255)    NULL,
        discountNotifications   BIT             NOT NULL CONSTRAINT DF_Users_DiscountNotif DEFAULT (0),
        loyaltyPoints           INT             NOT NULL CONSTRAINT DF_Users_Loyalty       DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.UserDiscounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserDiscounts (
        userId                 INT             NOT NULL,
        itemId                 INT             NOT NULL,
        itemDiscountPercentage DECIMAL(10,2)   NULL,
        CONSTRAINT PK_UserDiscounts PRIMARY KEY (userId, itemId),
        CONSTRAINT FK_UserDiscounts_User FOREIGN KEY (userId) REFERENCES dbo.Users (userId),
        CONSTRAINT FK_UserDiscounts_Item FOREIGN KEY (itemId) REFERENCES dbo.Items (itemId)
    );
END
GO

-- columns favouriteItem/stockAlert match `INSERT INTO UserNotifications VALUES (...)` in code
IF OBJECT_ID(N'dbo.UserNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserNotifications (
        userId          INT NOT NULL,
        itemId          INT NOT NULL,
        favouriteItem   BIT NOT NULL CONSTRAINT DF_UN_Fav   DEFAULT (0),
        stockAlert      BIT NOT NULL CONSTRAINT DF_UN_Alert DEFAULT (0),
        CONSTRAINT PK_UserNotifications PRIMARY KEY (userId, itemId),
        CONSTRAINT FK_UserNotifications_User FOREIGN KEY (userId) REFERENCES dbo.Users (userId),
        CONSTRAINT FK_UserNotifications_Item FOREIGN KEY (itemId) REFERENCES dbo.Items (itemId)
    );
END
GO

-- IMPORTANT: column is `PMSOption` (the code reads trackerRow["PMSOption"]).
-- Column order matches `INSERT INTO PeriodTrackers VALUES (...)` in code.
IF OBJECT_ID(N'dbo.PeriodTrackers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PeriodTrackers (
        userId          INT     NOT NULL PRIMARY KEY,
        startPeriodDate DATE    NULL,
        cycleDays       INT     NULL,
        periodLasts     INT     NULL,
        PMSOption       INT     NULL,
        CONSTRAINT FK_PeriodTrackers_User FOREIGN KEY (userId) REFERENCES dbo.Users (userId)
    );
END
GO

IF OBJECT_ID(N'dbo.PeriodNotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PeriodNotes (
        userId      INT             NOT NULL,
        noteId      INT             NOT NULL,
        noteBody    VARCHAR(255)    NULL,
        isDone      BIT             NOT NULL CONSTRAINT DF_PN_IsDone DEFAULT (0),
        CONSTRAINT PK_PeriodNotes PRIMARY KEY (userId, noteId),
        CONSTRAINT FK_PeriodNotes_User FOREIGN KEY (userId) REFERENCES dbo.Users (userId)
    );
END
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        orderId     INT     IDENTITY(1,1) PRIMARY KEY,
        clientId    INT     NOT NULL,
        isCompleted BIT     NOT NULL CONSTRAINT DF_Orders_Completed DEFAULT (0),
        isExpired   BIT     NOT NULL CONSTRAINT DF_Orders_Expired   DEFAULT (0),
        pickUpDate  DATE    NOT NULL,
        CONSTRAINT FK_Orders_User FOREIGN KEY (clientId) REFERENCES dbo.Users (userId)
    );
END
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        orderId         INT             NOT NULL,
        itemId          INT             NOT NULL,
        orderQuantity   INT             NOT NULL,
        price           DECIMAL(10,2)   NOT NULL,
        CONSTRAINT PK_OrderItems PRIMARY KEY (orderId, itemId),
        CONSTRAINT FK_OrderItems_Order FOREIGN KEY (orderId) REFERENCES dbo.Orders (orderId),
        CONSTRAINT FK_OrderItems_Item  FOREIGN KEY (itemId)  REFERENCES dbo.Items  (itemId)
    );
END
GO

-- =======================================================================
-- 4. SALARY / HANDOVER TABLES
-- =======================================================================

IF OBJECT_ID(N'dbo.Doctors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Doctors (
        StaffID     INT             NOT NULL PRIMARY KEY,
        FirstName   NVARCHAR(100)   NOT NULL,
        LastName    NVARCHAR(100)   NOT NULL,
        IsAvailable BIT             NOT NULL CONSTRAINT DF_Doctors_IsAvailable DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.MedicineSales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MedicineSales (
        SaleId          INT         IDENTITY(1,1) PRIMARY KEY,
        PharmacistID    INT         NOT NULL,
        SaleDate        DATETIME2   NOT NULL,
        Quantity        INT         NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.PharmacyStaff', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PharmacyStaff (
        StaffID     INT             NOT NULL PRIMARY KEY,
        DisplayName NVARCHAR(200)   NOT NULL,
        IsAvailable BIT             NOT NULL CONSTRAINT DF_PharmacyStaff_IsAvailable DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.Pending_Medications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pending_Medications (
        Id                  INT             IDENTITY(1,1) PRIMARY KEY,
        ResponsibleStaffID  INT             NOT NULL,
        OrderStatus         NVARCHAR(50)    NOT NULL,
        CONSTRAINT FK_Pending_Medications_Staff FOREIGN KEY (ResponsibleStaffID)
            REFERENCES dbo.PharmacyStaff (StaffID)
    );
    CREATE INDEX IX_Pending_Medications_Staff_Status ON dbo.Pending_Medications (ResponsibleStaffID, OrderStatus);
END
GO

IF OBJECT_ID(N'dbo.PharmacyShifts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PharmacyShifts (
        ShiftId         INT             IDENTITY(1,1) PRIMARY KEY,
        StaffID         INT             NOT NULL,
        StartDateTime   DATETIME2       NOT NULL,
        EndDateTime     DATETIME2       NOT NULL,
        Status          NVARCHAR(20)    NOT NULL,
        CONSTRAINT FK_PharmacyShifts_Staff FOREIGN KEY (StaffID)
            REFERENCES dbo.PharmacyStaff (StaffID)
    );
    CREATE INDEX IX_PharmacyShifts_Staff_Status ON dbo.PharmacyShifts (StaffID, Status);
END
GO

-- =======================================================================
-- 5. SEED DATA
-- =======================================================================

-- Staff
IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE staff_id = 1)
BEGIN
    INSERT INTO dbo.Staff (staff_id, role, first_name, last_name, contact_info, is_available, license_number, specialization, status, certification, years_of_experience) VALUES
        (1, N'Doctor',     N'Gregory', N'House',  N'house@hospital.local',  1, N'LIC-1001', N'Diagnostician', N'AVAILABLE', NULL,            10),
        (2, N'Doctor',     N'James',   N'Wilson', N'wilson@hospital.local', 1, N'LIC-1002', N'Oncology',      N'AVAILABLE', NULL,             8),
        (3, N'Doctor',     N'Lisa',    N'Cuddy',  N'cuddy@hospital.local',  1, N'LIC-1003', N'Surgery',       N'AVAILABLE', NULL,            12),
        (4, N'Pharmacist', N'Jamie',   N'Chen',   N'jamie@hospital.local',  1, NULL,        NULL,             NULL,         N'Compounding',   4),
        (5, N'Pharmacist', N'Pat',     N'Moore',  N'pat@hospital.local',    1, NULL,        NULL,             NULL,         N'Hospital',      6);
END
GO

-- Shifts
IF NOT EXISTS (SELECT 1 FROM dbo.Shifts)
BEGIN
    DECLARE @TodayStart DATETIME2 = CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2);
    INSERT INTO dbo.Shifts (staff_id, location, start_time, end_time, status, is_active) VALUES
        (1, N'Clinic',   @TodayStart, DATEADD(HOUR,8,@TodayStart), N'ACTIVE',    1),
        (2, N'ER',       @TodayStart, DATEADD(HOUR,8,@TodayStart), N'SCHEDULED', 0),
        (3, N'ER',       @TodayStart, DATEADD(HOUR,8,@TodayStart), N'SCHEDULED', 0),
        (4, N'Pharmacy', @TodayStart, DATEADD(HOUR,8,@TodayStart), N'ACTIVE',    1),
        (5, N'Pharmacy', @TodayStart, DATEADD(HOUR,8,@TodayStart), N'SCHEDULED', 0);
END
GO

-- Substances (full set from init.sql)
IF NOT EXISTS (SELECT 1 FROM dbo.Substances)
BEGIN
    INSERT INTO dbo.Substances (name, lethalDose, description) VALUES
        (N'Ibuprofen',       3200.00, N'Anti-inflammatory pain reliever'),
        (N'Paracetamol',     4000.00, N'Pain reliever and fever reducer'),
        (N'Magnesium',       2500.00, N'Mineral supplement for muscle and nerve support'),
        (N'Iron',              45.00, N'Mineral supplement used for iron deficiency'),
        (N'Vitamin C',       2000.00, N'Vitamin supplement for immune support'),
        (N'Calcium',         2500.00, N'Mineral supplement for bones and muscles'),
        (N'Omega 3',         3000.00, N'Fatty acid supplement for heart and brain health'),
        (N'Melatonin',         10.00, N'Sleep support supplement'),
        (N'Probiotics',      1000.00, N'Digestive support supplement'),
        (N'Zinc',              40.00, N'Mineral supplement for immunity'),
        (N'Cetirizine',       500.00, N'Antihistamine for allergy relief'),
        (N'Loratadine',      1000.00, N'Non-drowsy antihistamine'),
        (N'Loperamide',        60.00, N'Medication to decrease frequency of diarrhea'),
        (N'Simethicone',     2000.00, N'Anti-foaming agent to reduce bloating and gas'),
        (N'Diclofenac',      1500.00, N'Nonsteroidal anti-inflammatory drug (NSAID)'),
        (N'Dexpanthenol',    5000.00, N'Skin protectant and moisturizer'),
        (N'Vitamin D3',        50.00, N'Essential vitamin for bone health and immunity'),
        (N'Xylometazoline',    10.00, N'Decongestant for nasal passages'),
        (N'Acetylcysteine',  3000.00, N'Mucolytic agent to clear mucus');
END
GO

-- Items (35 products from init.sql)
IF NOT EXISTS (SELECT 1 FROM dbo.Items)
BEGIN
    INSERT INTO dbo.Items (name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage) VALUES
        (N'Nurofen Express',          28.50, N'pain relief',  20, N'Reckitt',      N'Assets/nurofen.png',       40, N'Fast pain relief',    N'Ibuprofen capsules for pain and inflammation',          0),
        (N'Panadol Extra',            19.99, N'pain relief',  16, N'GSK',          N'Assets/panadol.png',       35, N'Extra strength',      N'Paracetamol tablets for headaches and fever',          10),
        (N'Magne B6',                 32.00, N'wellness',     50, N'Sanofi',       N'Assets/magneb6.png',       25, N'Magnesium support',   N'Magnesium and vitamin B6 supplement',                   0),
        (N'Feroglobin',               36.50, N'wellness',     30, N'Vitabiotics',  N'Assets/feroglobin.png',    18, N'Iron formula',        N'Iron supplement for energy and blood health',           5),
        (N'Vitamin C 1000',           22.00, N'wellness',     20, N'NaturPharma',  N'Assets/vitaminc.png',      50, N'Immune support',      N'High strength vitamin C tablets',                       0),
        (N'Calcium + D3',             27.50, N'wellness',     30, N'BioFarm',      N'Assets/calciumd3.png',     22, N'Bone support',        N'Calcium and vitamin D3 supplement',                    15),
        (N'Omega 3 Forte',            45.00, N'wellness',     60, N'Doppelherz',   N'Assets/omega3.png',        14, N'Heart support',       N'Omega 3 capsules for heart and brain',                  0),
        (N'Melatonin Sleep',          18.00, N'wellness',     30, N'Walmark',      N'Assets/melatonin.png',     12, N'Sleep support',       N'Melatonin tablets for better sleep',                    0),
        (N'Probiotic Balance',        39.99, N'wellness',     20, N'Secom',        N'Assets/probiotic.png',     16, N'Digestive comfort',   N'Daily probiotic capsules',                             20),
        (N'Zinc Complex',             21.50, N'wellness',     30, N'NaturMil',     N'Assets/zinc.png',          28, N'Immune defense',      N'Zinc supplement for immune support',                    0),
        (N'Coldrex MaxGrip',          31.00, N'cold and flu', 10, N'GSK',          N'Assets/coldrex.png',       20, N'Cold relief',         N'Powder for cold and flu symptoms',                      0),
        (N'Strepsils Intensive',      24.00, N'cold and flu', 24, N'Reckitt',      N'Assets/strepsils.png',     17, N'Sore throat relief',  N'Lozenges for sore throat',                              0),
        (N'No-Spa Forte',             26.00, N'pain relief',  24, N'Sanofi',       N'Assets/nospa.png',         30, N'Cramp relief',        N'Drotaverine tablets for cramps',                        0),
        (N'Femina Comfort',           29.50, N'wellness',     30, N'HerbalLab',    N'Assets/femina.png',        19, N'Period wellness',     N'Supplement designed for menstrual comfort',             10),
        (N'Herbal Relax Tea Capsules',23.50, N'wellness',     20, N'PlantMed',     N'Assets/herbalrelax.png',   21, N'Relax support',       N'Natural calming capsules for stress relief',             0),
        (N'Zyrtec',                   25.50, N'allergy',      20, N'UCB',          N'Assets/zyrtec.png',        40, N'24 Hour Relief',      N'Cetirizine tablets for indoor and outdoor allergies',   0),
        (N'Claritine',                24.00, N'allergy',      30, N'Bayer',        N'Assets/claritine.png',     35, N'Non-Drowsy',          N'Loratadine allergy relief tablets',                    10),
        (N'Imodium',                  18.50, N'digestion',    12, N'J&J',          N'Assets/imodium.png',       50, N'Fast Acting',         N'Loperamide capsules for diarrhea relief',               0),
        (N'Espumisan',                22.00, N'digestion',    50, N'Berlin-Chemie',N'Assets/espumisan.png',     60, N'Anti-Bloating',       N'Simethicone capsules for gas relief',                   5),
        (N'Colebil',                  15.00, N'digestion',    20, N'Biofarm',      N'Assets/colebil.png',       45, N'Bile Support',        N'Digestive supplement after heavy meals',                0),
        (N'Smecta',                   19.50, N'digestion',    10, N'Ipsen',        N'Assets/smecta.png',        30, N'Digestive Protectant',N'Powder for oral suspension',                            0),
        (N'Voltaren Gel',             35.00, N'pain relief',   1, N'GSK',          N'Assets/voltaren.png',      25, N'Targeted Pain Relief',N'Diclofenac topical gel for joint and muscle pain',     15),
        (N'Bepanthen Ointment',       28.00, N'skincare',      1, N'Bayer',        N'Assets/bepanthen.png',     40, N'Skin Repair',         N'Dexpanthenol ointment for skin irritation',             0),
        (N'Sudocrem',                 26.50, N'skincare',      1, N'Teva',         N'Assets/sudocrem.png',      55, N'Healing Cream',       N'Antiseptic healing cream for diaper rash and eczema',   0),
        (N'Cerave Cleanser',          55.00, N'skincare',      1, N'L''Oreal',     N'Assets/cerave.png',        20, N'Hydrating Formula',   N'Daily facial cleanser with ceramides',                 20),
        (N'Centrum Men',              65.00, N'wellness',     30, N'GSK',          N'Assets/centrum_men.png',   15, N'Multivitamin',        N'Complete daily multivitamin for men',                   0),
        (N'Centrum Women',            65.00, N'wellness',     30, N'GSK',          N'Assets/centrum_women.png', 15, N'Multivitamin',        N'Complete daily multivitamin for women',                  0),
        (N'Supradyn Energy',          48.00, N'wellness',     30, N'Bayer',        N'Assets/supradyn.png',      22, N'Energy Support',      N'Vitamins with CoQ10 for energy release',               10),
        (N'Vitamin D3 2000 IU',       15.99, N'wellness',     60, N'NaturPharma',  N'Assets/vitamind3.png',     80, N'Sun Vitamin',         N'High-dose Vitamin D3 softgels',                         0),
        (N'B-Complex Forte',          21.00, N'wellness',     30, N'Zentiva',      N'Assets/bcomplex.png',      40, N'Nerve Support',       N'High strength B-vitamins',                              0),
        (N'Betadine Solution',        18.00, N'first aid',     1, N'Egis',         N'Assets/betadine.png',      30, N'Antiseptic',          N'Povidone-iodine topical solution for wound care',        0),
        (N'Sterile Plasters',         12.50, N'first aid',    50, N'Urgo',         N'Assets/plasters.png',     100, N'Waterproof',          N'Assorted sizes of waterproof bandages',                  0),
        (N'Olynth Nasal Spray',       16.50, N'cold and flu',  1, N'J&J',          N'Assets/olynth.png',        45, N'Decongestant',        N'Xylometazoline spray for unblocking the nose',           0),
        (N'ACC 600',                  29.00, N'cold and flu', 10, N'Sandoz',       N'Assets/acc600.png',        30, N'Mucus Clearance',     N'Effervescent tablets for productive coughs',             0),
        (N'Theraflu Extra',           33.00, N'cold and flu', 10, N'GSK',          N'Assets/theraflu.png',      25, N'Severe Cold',         N'Hot liquid powder for severe cold symptoms',            10);
END
GO

-- ItemSubstances
IF NOT EXISTS (SELECT 1 FROM dbo.ItemSubstances)
BEGIN
    INSERT INTO dbo.ItemSubstances (itemId, name, concentration) VALUES
        (1,  N'Ibuprofen',      400.00),
        (2,  N'Paracetamol',    500.00),
        (3,  N'Magnesium',      250.00),
        (4,  N'Iron',            14.00),
        (5,  N'Vitamin C',     1000.00),
        (6,  N'Calcium',        500.00),
        (7,  N'Omega 3',       1000.00),
        (8,  N'Melatonin',        5.00),
        (9,  N'Probiotics',     200.00),
        (10, N'Zinc',            10.00),
        (11, N'Paracetamol',   1000.00),
        (12, N'Ibuprofen',        8.75),
        (13, N'Ibuprofen',       80.00),
        (14, N'Magnesium',      150.00),
        (14, N'Vitamin C',       80.00),
        (15, N'Magnesium',      100.00),
        (16, N'Cetirizine',      10.00),
        (17, N'Loratadine',      10.00),
        (18, N'Loperamide',       2.00),
        (19, N'Simethicone',     40.00),
        (22, N'Diclofenac',       1.00),
        (23, N'Dexpanthenol',     5.00),
        (29, N'Vitamin D3',      50.00),
        (33, N'Xylometazoline',   0.10),
        (34, N'Acetylcysteine', 600.00),
        (35, N'Paracetamol',    650.00);
END
GO

-- ItemExpirationDates
IF NOT EXISTS (SELECT 1 FROM dbo.ItemExpirationDates)
BEGIN
    INSERT INTO dbo.ItemExpirationDates (itemId, expirationDate, numberOfPacks) VALUES
        (1, '2026-08-15',20),(1, '2027-01-10',20),
        (2, '2026-09-20',15),(2, '2027-02-15',20),
        (3, '2026-10-05',10),(3, '2027-03-01',15),
        (4, '2026-11-12', 8),(4, '2027-04-18',10),
        (5, '2026-07-30',25),(5, '2027-01-25',25),
        (6, '2026-12-10',10),(6, '2027-05-05',12),
        (7, '2026-09-01', 6),(7, '2027-06-14', 8),
        (8, '2026-08-22', 5),(8, '2027-02-28', 7),
        (9, '2026-10-18', 8),(9, '2027-03-20', 8),
        (10,'2026-11-30',12),(10,'2027-04-30',16),
        (11,'2026-09-09',10),(11,'2027-01-19',10),
        (12,'2026-10-25', 7),(12,'2027-05-10',10),
        (13,'2026-08-08',15),(13,'2027-02-02',15),
        (14,'2026-12-22', 9),(14,'2027-06-01',10),
        (15,'2026-09-17',10),(15,'2027-03-11',11),
        (16,'2027-01-10',20),(16,'2028-05-15',20),
        (17,'2026-11-20',15),(17,'2027-08-30',20),
        (18,'2026-10-15',25),(18,'2027-12-01',25),
        (19,'2026-09-05',30),(19,'2028-01-20',30),
        (20,'2026-04-12',20),(20,'2027-09-18',25),
        (21,'2027-03-22',15),(21,'2028-06-10',15),
        (22,'2026-12-30',12),(22,'2027-11-25',13),
        (23,'2027-02-14',20),(23,'2028-02-14',20),
        (24,'2026-07-01',25),(24,'2027-07-01',30),
        (25,'2026-08-18',10),(25,'2027-08-18',10),
        (26,'2027-05-05', 7),(26,'2028-04-10', 8),
        (27,'2026-11-11', 7),(27,'2027-10-15', 8),
        (28,'2027-01-22',11),(28,'2028-03-30',11),
        (29,'2026-09-09',40),(29,'2027-09-09',40),
        (30,'2026-12-01',20),(30,'2027-12-01',20),
        (31,'2027-04-20',15),(31,'2028-08-20',15),
        (32,'2030-01-01',50),(32,'2031-01-01',50),
        (33,'2026-10-31',20),(33,'2027-10-31',25),
        (34,'2027-01-15',15),(34,'2028-02-20',15),
        (35,'2026-11-30',10),(35,'2027-11-30',15);
END
GO

-- Users
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (email, phoneNumber, passwordHash, isDisabled, isAdmin, username, discountNotifications, loyaltyPoints) VALUES
        (N'admin@pharmacy.local', N'0700000000', N'hashed_pwd_admin', 0, 1, N'admin_super', 1, 1000),
        (N'johndoe@test.com',     N'0711111111', N'hashed_pwd_john',  0, 0, N'johndoe',     1,  150),
        (N'janedoe@test.com',     N'0722222222', N'hashed_pwd_jane',  0, 0, N'janedoe',     0,   45);
END
GO

-- UserDiscounts
IF NOT EXISTS (SELECT 1 FROM dbo.UserDiscounts)
BEGIN
    INSERT INTO dbo.UserDiscounts (userId, itemId, itemDiscountPercentage) VALUES
        (2,  1,  5.00),
        (3, 14, 15.00);
END
GO

-- UserNotifications
IF NOT EXISTS (SELECT 1 FROM dbo.UserNotifications)
BEGIN
    INSERT INTO dbo.UserNotifications (userId, itemId, favouriteItem, stockAlert) VALUES
        (2,  5, 1, 0),
        (2, 11, 0, 1),
        (3, 14, 1, 1);
END
GO

-- PeriodTrackers
IF NOT EXISTS (SELECT 1 FROM dbo.PeriodTrackers)
BEGIN
    INSERT INTO dbo.PeriodTrackers (userId, startPeriodDate, cycleDays, periodLasts, PMSOption) VALUES
        (3, '2026-04-10', 28, 5, 2);
END
GO

-- PeriodNotes
IF NOT EXISTS (SELECT 1 FROM dbo.PeriodNotes)
BEGIN
    INSERT INTO dbo.PeriodNotes (userId, noteId, noteBody, isDone) VALUES
        (3, 1, N'Take magnesium supplement', 1),
        (3, 2, N'Drink herbal relax tea',     0),
        (3, 3, N'Buy more Femina Comfort',    0);
END
GO

-- Orders
IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    INSERT INTO dbo.Orders (clientId, isCompleted, isExpired, pickUpDate) VALUES
        (2, 1, 0, '2026-04-15'),
        (3, 0, 0, '2026-04-25'),
        (2, 0, 1, '2026-03-10');
END
GO

-- OrderItems
IF NOT EXISTS (SELECT 1 FROM dbo.OrderItems)
BEGIN
    INSERT INTO dbo.OrderItems (orderId, itemId, orderQuantity, price) VALUES
        (1,  1, 2, 28.50),
        (1,  5, 1, 22.00),
        (2, 14, 1, 29.50),
        (2, 15, 2, 23.50),
        (3, 11, 1, 31.00);
END
GO

-- Medical evaluations
IF NOT EXISTS (SELECT 1 FROM dbo.Medical_Evaluations)
BEGIN
    INSERT INTO dbo.Medical_Evaluations (doctor_id, patient_id, diagnosis, doctor_notes, medications, source, assumed_risk) VALUES
        (1, 2, N'Mild headache and fever',  N'Take with food, twice a day for 3 days', N'Nurofen Express, Panadol Extra', N'PATIENT', 0),
        (1, 3, N'Allergy flare-up',         N'Once daily before bed',                  N'Zyrtec',                         N'PATIENT', 0);
END
GO

-- High-risk medicines
IF NOT EXISTS (SELECT 1 FROM dbo.High_Risk_Medicines)
BEGIN
    INSERT INTO dbo.High_Risk_Medicines (medicine_name, warning_message) VALUES
        (N'Warfarin',     N'Anticoagulant - check INR before prescribing.'),
        (N'Methotrexate', N'Hepatotoxic - confirm dosing and weekly schedule.');
END
GO

-- Doctors (salary feature)
IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE StaffID = 1)
BEGIN
    INSERT INTO dbo.Doctors (StaffID, FirstName, LastName, IsAvailable) VALUES
        (1, N'Gregory', N'House',  1),
        (2, N'James',   N'Wilson', 1),
        (3, N'Lisa',    N'Cuddy',  1);
END
GO

-- PharmacyStaff (salary + handover features)
IF NOT EXISTS (SELECT 1 FROM dbo.PharmacyStaff WHERE StaffID = 1)
BEGIN
    INSERT INTO dbo.PharmacyStaff (StaffID, DisplayName, IsAvailable) VALUES
        (1, N'Current User (demo)', 1),
        (4, N'Jamie Chen',          1),
        (5, N'Pat Moore',           1);
END
GO

-- Pending_Medications
IF NOT EXISTS (SELECT 1 FROM dbo.Pending_Medications)
BEGIN
    INSERT INTO dbo.Pending_Medications (ResponsibleStaffID, OrderStatus) VALUES
        (1, N'Processing'),
        (1, N'Processing'),
        (1, N'Completed');
END
GO

-- PharmacyShifts
IF NOT EXISTS (SELECT 1 FROM dbo.PharmacyShifts WHERE StaffID = 1 AND Status = N'Active')
BEGIN
    DECLARE @PSStart DATETIME2 = CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2);
    DECLARE @PSEnd   DATETIME2 = DATEADD(HOUR, 8, @PSStart);
    INSERT INTO dbo.PharmacyShifts (StaffID, StartDateTime, EndDateTime, Status) VALUES
        (1, @PSStart, @PSEnd, N'Active'),
        (4, @PSStart, @PSEnd, N'Active'),
        (5, @PSStart, @PSEnd, N'Scheduled');
END
GO

-- MedicineSales
IF NOT EXISTS (SELECT 1 FROM dbo.MedicineSales)
BEGIN
    DECLARE @Today DATETIME2 = SYSDATETIME();
    INSERT INTO dbo.MedicineSales (PharmacistID, SaleDate, Quantity) VALUES
        (4, @Today,                    50),
        (4, DATEADD(DAY,-2,@Today),    35),
        (4, DATEADD(DAY,-4,@Today),    20),
        (5, @Today,                   120),
        (5, DATEADD(DAY,-2,@Today),    80);
END
GO

-- PharmacyHandover
IF NOT EXISTS (SELECT 1 FROM dbo.PharmacyHandover)
BEGIN
    DECLARE @HandoverDate DATETIME2 = CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2);
    INSERT INTO dbo.PharmacyHandover (PharmacistID, HandoverDate) VALUES
        (4, @HandoverDate),
        (5, DATEADD(DAY,-1,@HandoverDate));
END
GO

-- =======================================================================
-- VERIFICATION
-- =======================================================================
SELECT [table] = 'Staff',                rows = COUNT(*) FROM dbo.Staff
UNION ALL SELECT 'Shifts',                        COUNT(*) FROM dbo.Shifts
UNION ALL SELECT 'Appointments',                  COUNT(*) FROM dbo.Appointments
UNION ALL SELECT 'Medical_Evaluations',           COUNT(*) FROM dbo.Medical_Evaluations
UNION ALL SELECT 'ER_Requests',                   COUNT(*) FROM dbo.ER_Requests
UNION ALL SELECT 'Hangouts',                      COUNT(*) FROM dbo.Hangouts
UNION ALL SELECT 'ShiftSwapRequests',             COUNT(*) FROM dbo.ShiftSwapRequests
UNION ALL SELECT 'High_Risk_Medicines',           COUNT(*) FROM dbo.High_Risk_Medicines
UNION ALL SELECT 'Substances',                    COUNT(*) FROM dbo.Substances
UNION ALL SELECT 'Items',                         COUNT(*) FROM dbo.Items
UNION ALL SELECT 'ItemSubstances',                COUNT(*) FROM dbo.ItemSubstances
UNION ALL SELECT 'ItemExpirationDates',           COUNT(*) FROM dbo.ItemExpirationDates
UNION ALL SELECT 'Users',                         COUNT(*) FROM dbo.Users
UNION ALL SELECT 'UserDiscounts',                 COUNT(*) FROM dbo.UserDiscounts
UNION ALL SELECT 'UserNotifications',             COUNT(*) FROM dbo.UserNotifications
UNION ALL SELECT 'PeriodTrackers',                COUNT(*) FROM dbo.PeriodTrackers
UNION ALL SELECT 'PeriodNotes',                   COUNT(*) FROM dbo.PeriodNotes
UNION ALL SELECT 'Orders',                        COUNT(*) FROM dbo.Orders
UNION ALL SELECT 'OrderItems',                    COUNT(*) FROM dbo.OrderItems
UNION ALL SELECT 'Doctors',                       COUNT(*) FROM dbo.Doctors
UNION ALL SELECT 'PharmacyStaff',                 COUNT(*) FROM dbo.PharmacyStaff
UNION ALL SELECT 'PharmacyShifts',                COUNT(*) FROM dbo.PharmacyShifts
UNION ALL SELECT 'Pending_Medications',           COUNT(*) FROM dbo.Pending_Medications
UNION ALL SELECT 'MedicineSales',                 COUNT(*) FROM dbo.MedicineSales
UNION ALL SELECT 'PharmacyHandover',              COUNT(*) FROM dbo.PharmacyHandover;
GO
