/*
  Unified setup for the merged 923-2 app.
  Target DB: HospitalDatabase (matches AppSettings.ConnectionString).
  Run as a single batch with sqlcmd, SSMS, or Azure Data Studio.

  Sections:
    1. Database
    2. Hospital-side tables (snake_case columns)
    3. Pharmacy-side tables  (camelCase columns)
    4. Seed data
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
-- 4. SEED DATA
-- =======================================================================

-- Staff (matches role-pick UI: at least one Doctor, one Pharmacist, one Admin)
IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE staff_id = 1)
BEGIN
    INSERT INTO dbo.Staff (staff_id, role, first_name, last_name, contact_info, is_available, license_number, specialization, status, certification, years_of_experience) VALUES
        (1, N'Doctor',     N'Gregory', N'House',  N'house@hospital.local',  1, N'LIC-1001', N'Diagnostician', N'AVAILABLE', NULL,            10),
        (2, N'Doctor',     N'James',   N'Wilson', N'wilson@hospital.local', 1, N'LIC-1002', N'Oncology',      N'AVAILABLE', NULL,            8),
        (3, N'Doctor',     N'Lisa',    N'Cuddy',  N'cuddy@hospital.local',  1, N'LIC-1003', N'Surgery',       N'AVAILABLE', NULL,            12),
        (4, N'Pharmacist', N'Jamie',   N'Chen',   N'jamie@hospital.local',  1, NULL,        NULL,             NULL,         N'Compounding',  4),
        (5, N'Pharmacist', N'Pat',     N'Moore',  N'pat@hospital.local',    1, NULL,        NULL,             NULL,         N'Hospital',     6);
END
GO

-- One sample shift per staff member, 8 hours, today
IF NOT EXISTS (SELECT 1 FROM dbo.Shifts)
BEGIN
    DECLARE @TodayStart DATETIME2 = CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2);
    INSERT INTO dbo.Shifts (staff_id, location, start_time, end_time, status, is_active) VALUES
        (1, N'Clinic',        @TodayStart, DATEADD(HOUR, 8, @TodayStart), N'ACTIVE',    1),
        (2, N'ER',             @TodayStart, DATEADD(HOUR, 8, @TodayStart), N'SCHEDULED', 0),
        (3, N'ER',             @TodayStart, DATEADD(HOUR, 8, @TodayStart), N'SCHEDULED', 0),
        (4, N'Pharmacy',       @TodayStart, DATEADD(HOUR, 8, @TodayStart), N'ACTIVE',    1),
        (5, N'Pharmacy',       @TodayStart, DATEADD(HOUR, 8, @TodayStart), N'SCHEDULED', 0);
END
GO

-- Substances
IF NOT EXISTS (SELECT 1 FROM dbo.Substances)
BEGIN
    INSERT INTO dbo.Substances (name, lethalDose, description) VALUES
        (N'Ibuprofen',   3200.00, N'Anti-inflammatory pain reliever'),
        (N'Paracetamol', 4000.00, N'Pain reliever and fever reducer'),
        (N'Magnesium',   2500.00, N'Mineral supplement for muscle and nerve support'),
        (N'Vitamin C',   2000.00, N'Vitamin supplement for immune support'),
        (N'Cetirizine',   500.00, N'Antihistamine for allergy relief');
END
GO

-- Items (let identity assign itemIds 1..5)
IF NOT EXISTS (SELECT 1 FROM dbo.Items)
BEGIN
    INSERT INTO dbo.Items (name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage) VALUES
        (N'Nurofen Express',  28.50, N'pain relief', 20, N'Reckitt',     N'Assets/nurofen.png',  40, N'Fast pain relief', N'Ibuprofen capsules',           0),
        (N'Panadol Extra',    19.99, N'pain relief', 16, N'GSK',         N'Assets/panadol.png',  35, N'Extra strength',   N'Paracetamol tablets',         10),
        (N'Magne B6',         32.00, N'wellness',    50, N'Sanofi',      N'Assets/magneb6.png',  25, N'Magnesium support',N'Magnesium + B6 supplement',    0),
        (N'Vitamin C 1000',   22.00, N'wellness',    20, N'NaturPharma', N'Assets/vitaminc.png', 50, N'Immune support',   N'Vitamin C tablets',            0),
        (N'Zyrtec',           25.50, N'allergy',     20, N'UCB',         N'Assets/zyrtec.png',   40, N'24h relief',       N'Cetirizine for allergies',     0);
END
GO

-- Item / substance links
IF NOT EXISTS (SELECT 1 FROM dbo.ItemSubstances)
BEGIN
    INSERT INTO dbo.ItemSubstances (itemId, name, concentration) VALUES
        (1, N'Ibuprofen',   400.00),
        (2, N'Paracetamol', 500.00),
        (3, N'Magnesium',   250.00),
        (4, N'Vitamin C',  1000.00),
        (5, N'Cetirizine',   10.00);
END
GO

-- Item batches (future expiration so seed orders are placeable)
IF NOT EXISTS (SELECT 1 FROM dbo.ItemExpirationDates)
BEGIN
    INSERT INTO dbo.ItemExpirationDates (itemId, expirationDate, numberOfPacks) VALUES
        (1, '2027-01-10', 20),
        (2, '2027-02-15', 20),
        (3, '2027-03-01', 15),
        (4, '2027-01-25', 25),
        (5, '2027-04-10', 20);
END
GO

-- Users (admin + 2 customers; passwords are plaintext placeholders)
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (email, phoneNumber, passwordHash, isDisabled, isAdmin, username, discountNotifications, loyaltyPoints) VALUES
        (N'admin@pharmacy.local', N'0700000000', N'hashed_pwd_admin', 0, 1, N'admin_super', 1, 1000),
        (N'john@test.com',        N'0711111111', N'hashed_pwd_john',  0, 0, N'johndoe',     1,  150),
        (N'jane@test.com',        N'0722222222', N'hashed_pwd_jane',  0, 0, N'janedoe',     0,   45);
END
GO

-- A medical evaluation whose evaluation_id can be entered as a "prescription ID"
-- in the basket to demo F2.7 against real data (tests fix #1).
IF NOT EXISTS (SELECT 1 FROM dbo.Medical_Evaluations)
BEGIN
    INSERT INTO dbo.Medical_Evaluations (doctor_id, patient_id, diagnosis, doctor_notes, medications, source, assumed_risk) VALUES
        (1, 2, N'Mild headache and fever',
               N'Take with food, twice a day for 3 days',
               N'Nurofen Express, Panadol Extra',
               N'PATIENT', 0),
        (1, 3, N'Allergy flare-up',
               N'Once daily before bed',
               N'Zyrtec',
               N'PATIENT', 0);
END
GO

-- High-risk medicine list (used by MedicalEvaluationService.CheckMedicineConflict)
IF NOT EXISTS (SELECT 1 FROM dbo.High_Risk_Medicines)
BEGIN
    INSERT INTO dbo.High_Risk_Medicines (medicine_name, warning_message) VALUES
        (N'Warfarin',   N'Anticoagulant - check INR before prescribing.'),
        (N'Methotrexate', N'Hepatotoxic - confirm dosing and weekly schedule.');
END
GO

-- One sample order for a customer (lets order history page render something)
IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    INSERT INTO dbo.Orders (clientId, isCompleted, isExpired, pickUpDate) VALUES
        (2, 0, 0, DATEADD(DAY, 2, CAST(SYSDATETIME() AS DATE)));

    DECLARE @SeedOrderId INT = SCOPE_IDENTITY();
    INSERT INTO dbo.OrderItems (orderId, itemId, orderQuantity, price) VALUES
        (@SeedOrderId, 1, 2, 57.00),  -- 2x Nurofen
        (@SeedOrderId, 4, 1, 22.00);  -- 1x Vitamin C
END
GO

-- Verification (optional)
SELECT 'Staff'                AS [table], COUNT(*) AS rows FROM dbo.Staff
UNION ALL SELECT 'Shifts',                 COUNT(*) FROM dbo.Shifts
UNION ALL SELECT 'Appointments',           COUNT(*) FROM dbo.Appointments
UNION ALL SELECT 'Medical_Evaluations',    COUNT(*) FROM dbo.Medical_Evaluations
UNION ALL SELECT 'ER_Requests',            COUNT(*) FROM dbo.ER_Requests
UNION ALL SELECT 'Hangouts',               COUNT(*) FROM dbo.Hangouts
UNION ALL SELECT 'Hangout_Participants',   COUNT(*) FROM dbo.Hangout_Participants
UNION ALL SELECT 'ShiftSwapRequests',      COUNT(*) FROM dbo.ShiftSwapRequests
UNION ALL SELECT 'High_Risk_Medicines',    COUNT(*) FROM dbo.High_Risk_Medicines
UNION ALL SELECT 'Notifications',          COUNT(*) FROM dbo.Notifications
UNION ALL SELECT 'PharmacyHandover',       COUNT(*) FROM dbo.PharmacyHandover
UNION ALL SELECT 'Substances',             COUNT(*) FROM dbo.Substances
UNION ALL SELECT 'Items',                  COUNT(*) FROM dbo.Items
UNION ALL SELECT 'ItemSubstances',         COUNT(*) FROM dbo.ItemSubstances
UNION ALL SELECT 'ItemExpirationDates',    COUNT(*) FROM dbo.ItemExpirationDates
UNION ALL SELECT 'Users',                  COUNT(*) FROM dbo.Users
UNION ALL SELECT 'UserDiscounts',          COUNT(*) FROM dbo.UserDiscounts
UNION ALL SELECT 'UserNotifications',      COUNT(*) FROM dbo.UserNotifications
UNION ALL SELECT 'PeriodTrackers',         COUNT(*) FROM dbo.PeriodTrackers
UNION ALL SELECT 'PeriodNotes',            COUNT(*) FROM dbo.PeriodNotes
UNION ALL SELECT 'Orders',                 COUNT(*) FROM dbo.Orders
UNION ALL SELECT 'OrderItems',             COUNT(*) FROM dbo.OrderItems;
GO
