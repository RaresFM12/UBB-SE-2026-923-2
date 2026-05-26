#nullable disable

namespace UBB_SE_2026_923_2.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Staff
                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffId = 1)
                BEGIN
                    SET IDENTITY_INSERT dbo.Staff ON;
        
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate) VALUES
                    (1, N'house@hospital.local',  N'hash', N'Doctor',     N'Diagnostics',  N'Gregory', N'House',  N'555-0101', 1, N'LIC-1001', N'Diagnostician', N'AVAILABLE', N'Board Certified', 10, 150.0),
                    (2, N'wilson@hospital.local', N'hash', N'Doctor',     N'Oncology',     N'James',   N'Wilson', N'555-0102', 1, N'LIC-1002', N'Diagnostician', N'AVAILABLE', N'Board Certified',  8, 140.0),
                    (3, N'cuddy@hospital.local',  N'hash', N'Doctor',     N'Admin',        N'Lisa',    N'Cuddy',  N'555-0103', 1, N'LIC-1003', N'Surgery',       N'AVAILABLE', N'Board Certified', 12, 160.0),
                    (4, N'jamie@hospital.local',  N'hash', N'Pharmacist', N'Pharmacy',     N'Jamie',   N'Chen',   N'555-0104', 1, N'LIC-1004', N'Nurse',         N'AVAILABLE', N'Compounding',      4,  80.0),
                    (5, N'pat@hospital.local',    N'hash', N'Pharmacist', N'Pharmacy',     N'Pat',     N'Moore',  N'555-0105', 1, N'LIC-1005', N'Doctor',        N'AVAILABLE', N'Hospital',         6,  85.0);
        
                    SET IDENTITY_INSERT dbo.Staff OFF;
                END

                -- MedicalEvaluations
                IF NOT EXISTS (SELECT 1 FROM dbo.MedicalEvaluations)
                BEGIN
                    INSERT INTO dbo.MedicalEvaluations (PatientId, Symptoms, MedicationsList, Notes, EvaluationDate, DoctorId) VALUES
                    (N'PAT-001', N'Headache, fever, fatigue',        N'Panadol Extra',                    N'Patient advised to rest and hydrate',       '2026-04-10 09:00', 1),
                    (N'PAT-002', N'Joint pain, swelling',            N'Nurofen Express',                  N'Referred for physiotherapy',                '2026-04-11 10:30', 1),
                    (N'PAT-003', N'Nausea, abdominal pain',          N'Espumisan',                        N'Follow-up in 1 week',                       '2026-04-12 11:00', 2),
                    (N'PAT-004', N'Shortness of breath, chest pain', N'None',                             N'Urgent cardiology referral',                '2026-04-13 08:00', 3),
                    (N'PAT-005', N'Skin rash, itching',              N'Zyrtec',                           N'Allergy testing recommended',               '2026-04-14 14:00', 1),
                    (N'PAT-006', N'Insomnia, anxiety',               N'Melatonin Sleep',                  N'Stress management counseling suggested',    '2026-04-15 15:30', 2),
                    (N'PAT-007', N'Runny nose, sneezing',            N'Claritine, Olynth Nasal Spray',    N'Seasonal allergies, reassess in spring',    '2026-04-16 09:30', 3),
                    (N'PAT-008', N'Back pain, muscle stiffness',     N'Voltaren Gel, Nurofen Express',    N'Advised to avoid heavy lifting',            '2026-04-17 11:00', 1),
                    (N'PAT-009', N'Diarrhea, stomach cramps',        N'Imodium, Smecta',                  N'Dietary changes recommended',               '2026-04-18 10:00', 2),
                    (N'PAT-010', N'Productive cough, mucus',         N'ACC 600',                          N'Follow-up if no improvement in 5 days',    '2026-04-19 13:00', 3);
                END

                -- Shifts
                IF NOT EXISTS (SELECT 1 FROM dbo.Shifts)
                BEGIN
                    DECLARE @TomorrowStart DATETIME2 = DATEADD(DAY, 1, CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2));
                    INSERT INTO dbo.Shifts (StaffId, Location, StartTime, EndTime, Status) VALUES
                    (1, N'Clinic',   DATEADD(HOUR, 9, @TomorrowStart), DATEADD(HOUR, 17, @TomorrowStart), N'SCHEDULED'),
                    (2, N'ER',       DATEADD(HOUR, 18, @TomorrowStart), DATEADD(HOUR, 23, @TomorrowStart), N'SCHEDULED'),
                    (3, N'ER',       DATEADD(HOUR, 9, @TomorrowStart), DATEADD(HOUR, 17, @TomorrowStart), N'SCHEDULED'),
                    (4, N'Pharmacy', DATEADD(HOUR, 9, @TomorrowStart), DATEADD(HOUR, 17, @TomorrowStart), N'SCHEDULED'),
                    (5, N'Pharmacy', DATEADD(HOUR, 18, @TomorrowStart), DATEADD(HOUR, 23, @TomorrowStart), N'SCHEDULED');
                END

               -- Substances
                INSERT INTO dbo.Substances (Name, LethalDose, Description)
                SELECT Name, LethalDose, Description FROM (VALUES
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
                    (N'Acetylcysteine',  3000.00, N'Mucolytic agent to clear mucus')
                ) AS src(Name, LethalDose, Description)
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Substances WHERE Name = src.Name);

                -- Items (36 products explicitly mapped by ID)
                IF NOT EXISTS (SELECT 1 FROM dbo.Items)
                BEGIN
                    SET IDENTITY_INSERT dbo.Items ON;
                    
                    INSERT INTO dbo.Items (Id, Name, Price, Category, NumberOfPills, Producer, ImagePath, Quantity, Label, Description, DiscountPercentage) VALUES
                    (1, N'Nurofen Express',          28.50, N'pain relief',  20, N'Reckitt',      N'Assets/nurofen.png',       40, N'Fast pain relief',    N'Ibuprofen capsules for pain and inflammation',          0),
                    (2, N'Panadol Extra',            19.99, N'pain relief',  16, N'GSK',          N'Assets/panadol.png',       0, N'Extra strength',      N'Paracetamol tablets for headaches and fever',          10),
                    (3, N'Magne B6',                 32.00, N'wellness',     50, N'Sanofi',       N'Assets/magneb6.png',       25, N'Magnesium support',   N'Magnesium and vitamin B6 supplement',                   0),
                    (4, N'Feroglobin',               36.50, N'wellness',     30, N'Vitabiotics',  N'Assets/feroglobin.png',    18, N'Iron formula',        N'Iron supplement for energy and blood health',           5),
                    (5, N'Vitamin C 1000',           22.00, N'wellness',     20, N'NaturPharma',  N'Assets/vitaminc.png',      50, N'Immune support',      N'High strength vitamin C tablets',                       0),
                    (6, N'Calcium + D3',             27.50, N'wellness',     30, N'BioFarm',      N'Assets/calciumd3.png',     22, N'Bone support',        N'Calcium and vitamin D3 supplement',                    15),
                    (7, N'Omega 3 Forte',            45.00, N'wellness',     60, N'Doppelherz',   N'Assets/omega3.png',        14, N'Heart support',       N'Omega 3 capsules for heart and brain',                  0),
                    (8, N'Melatonin Sleep',          18.00, N'wellness',     30, N'Walmark',      N'Assets/melatonin.png',     12, N'Sleep support',       N'Melatonin tablets for better sleep',                    0),
                    (9, N'Probiotic Balance',        39.99, N'wellness',     20, N'Secom',        N'Assets/probiotic.png',     0, N'Digestive comfort',   N'Daily probiotic capsules',                             20),
                    (10, N'Zinc Complex',             21.50, N'wellness',     30, N'NaturMil',     N'Assets/zinc.png',          28, N'Immune defense',      N'Zinc supplement for immune support',                    0),
                    (11, N'Coldrex MaxGrip',          31.00, N'cold and flu', 10, N'GSK',          N'Assets/coldrex.png',       20, N'Cold relief',         N'Powder for cold and flu symptoms',                      0),
                    (12, N'Strepsils Intensive',      24.00, N'cold and flu', 24, N'Reckitt',      N'Assets/strepsils.png',     17, N'Sore throat relief',  N'Lozenges for sore throat',                              0),
                    (13, N'No-Spa Forte',             26.00, N'pain relief',  24, N'Sanofi',       N'Assets/nospa.png',         30, N'Cramp relief',        N'Drotaverine tablets for cramps',                        0),
                    (14, N'Femina Comfort',           29.50, N'wellness',     30, N'HerbalLab',    N'Assets/femina.png',        0, N'Period wellness',     N'Supplement designed for menstrual comfort',            10),
                    (15, N'Herbal Relax Tea Capsules',23.50, N'wellness',     20, N'PlantMed',     N'Assets/herbalrelax.png',   21, N'Relax support',       N'Natural calming capsules for stress relief',            0),
                    (16, N'Zyrtec',                   25.50, N'allergy',      20, N'UCB',          N'Assets/zyrtec.png',        40, N'24 Hour Relief',      N'Cetirizine tablets for indoor and outdoor allergies',   0),
                    (17, N'Claritine',                24.00, N'allergy',      30, N'Bayer',        N'Assets/claritine.png',     35, N'Non-Drowsy',          N'Loratadine allergy relief tablets',                    10),
                    (18, N'Imodium',                  18.50, N'digestion',    12, N'J&J',          N'Assets/imodium.png',       50, N'Fast Acting',         N'Loperamide capsules for diarrhea relief',               0),
                    (19, N'Espumisan',                22.00, N'digestion',    50, N'Berlin-Chemie',N'Assets/espumisan.png',     60, N'Anti-Bloating',       N'Simethicone capsules for gas relief',                   5),
                    (20, N'Colebil',                  15.00, N'digestion',    20, N'Biofarm',      N'Assets/colebil.png',       0, N'Bile Support',        N'Digestive supplement after heavy meals',                0),
                    (21, N'Smecta',                   19.50, N'digestion',    10, N'Ipsen',        N'Assets/smecta.png',        30, N'Digestive Protectant',N'Powder for oral suspension',                            0),
                    (22, N'Voltaren Gel',             35.00, N'pain relief',   1, N'GSK',          N'Assets/voltaren.png',      0, N'Targeted Pain Relief',N'Diclofenac topical gel for joint and muscle pain',     15),
                    (23, N'Bepanthen Ointment',       28.00, N'skincare',      1, N'Bayer',        N'Assets/bepanthen.png',     40, N'Skin Repair',         N'Dexpanthenol ointment for skin irritation',             0),
                    (24, N'Sudocrem',                 26.50, N'skincare',      1, N'Teva',         N'Assets/sudocrem.png',      55, N'Healing Cream',       N'Antiseptic healing cream for diaper rash and eczema',   0),
                    (25, N'Cerave Cleanser',          55.00, N'skincare',      1, N'L''Oreal',     N'Assets/cerave.png',        20, N'Hydrating Formula',   N'Daily facial cleanser with ceramides',                 20),
                    (26, N'Centrum Men',              65.00, N'wellness',     30, N'GSK',          N'Assets/centrum_men.png',   15, N'Multivitamin',        N'Complete daily multivitamin for men',                   0),
                    (27, N'Centrum Women',            65.00, N'wellness',     30, N'GSK',          N'Assets/centrum_women.png', 15, N'Multivitamin',        N'Complete daily multivitamin for women',                 0),
                    (28, N'Supradyn Energy',          48.00, N'wellness',     30, N'Bayer',        N'Assets/supradyn.png',      22, N'Energy Support',      N'Vitamins with CoQ10 for energy release',               10),
                    (29, N'Vitamin D3 2000 IU',       15.99, N'wellness',     60, N'NaturPharma',  N'Assets/vitamind3.png',     80, N'Sun Vitamin',         N'High-dose Vitamin D3 softgels',                         0),
                    (30, N'B-Complex Forte',          21.00, N'wellness',     30, N'Zentiva',      N'Assets/bcomplex.png',      40, N'Nerve Support',       N'High strength B-vitamins',                              0),
                    (31, N'Betadine Solution',        18.00, N'first aid',     1, N'Egis',         N'Assets/betadine.png',      0, N'Antiseptic',          N'Povidone-iodine topical solution for wound care',        0),
                    (32, N'Sterile Plasters',         12.50, N'first aid',    50, N'Urgo',         N'Assets/plasters.png',     100, N'Waterproof',          N'Assorted sizes of waterproof bandages',                 0),
                    (33, N'Olynth Nasal Spray',       16.50, N'cold and flu',  1, N'J&J',          N'Assets/olynth.png',        0, N'Decongestant',        N'Xylometazoline spray for unblocking the nose',           0),
                    (34, N'ACC 600',                  29.00, N'cold and flu', 10, N'Sandoz',       N'Assets/acc600.png',        30, N'Mucus Clearance',     N'Effervescent tablets for productive coughs',             0),
                    (35, N'Theraflu Extra',           33.00, N'cold and flu', 10, N'GSK',          N'Assets/theraflu.png',      25, N'Severe Cold',         N'Hot liquid powder for severe cold symptoms',            10),
                    (36, N'Paracetamol Generic', 9.99, N'pain relief', 16, N'Generic Pharma', N'Assets/paracetamol.png', 30, N'Generic', N'Generic paracetamol tablets', 0);

                    SET IDENTITY_INSERT dbo.Items OFF;
                END

                -- ItemSubstances (link items to their active substances)
                IF NOT EXISTS (SELECT 1 FROM dbo.ItemSubstances)
                BEGIN
                    INSERT INTO dbo.ItemSubstances (ItemId, SubstanceName, Concentration) VALUES
                    (1,  N'Ibuprofen',      400.0),
                    (2,  N'Paracetamol',    500.0),
                    (3,  N'Magnesium',      150.0),
                    (4,  N'Iron',            14.0),
                    (5,  N'Vitamin C',     1000.0),
                    (6,  N'Calcium',        500.0),
                    (6,  N'Vitamin D3',      10.0),
                    (7,  N'Omega 3',        500.0),
                    (8,  N'Melatonin',        3.0),
                    (9,  N'Probiotics',     100.0),
                    (10, N'Zinc',            10.0),
                    (14, N'Magnesium',      150.0),
                    (15, N'Probiotics',     100.0),
                    (16, N'Cetirizine',      10.0),
                    (17, N'Loratadine',      10.0),
                    (18, N'Loperamide',       2.0),
                    (19, N'Simethicone',     40.0),
                    (22, N'Diclofenac',      10.0),
                    (23, N'Dexpanthenol',    50.0),
                    (29, N'Vitamin D3',      10.0),
                    (33, N'Xylometazoline',   0.1),
                    (34, N'Acetylcysteine', 600.0),
                    (36, N'Paracetamol', 500.0);
                END
                
                -- Adding item batches
                IF NOT EXISTS (SELECT 1 FROM dbo.ItemBatches)
                BEGIN
                    INSERT INTO dbo.ItemBatches (ItemId, ExpirationDate, NumberOfPacks) VALUES
                    (1,  '2027-01-01', 40),
                    (2,  '2027-01-01', 35),
                    (3,  '2027-01-01', 25),
                    (4,  '2027-01-01', 18),
                    (5,  '2027-01-01', 50),
                    (6,  '2027-01-01', 22),
                    (7,  '2027-01-01', 14),
                    (8,  '2027-01-01', 12),
                    (9,  '2027-01-01', 16),
                    (10, '2027-01-01', 28),
                    (11, '2027-01-01', 20),
                    (12, '2027-01-01', 17),
                    (13, '2027-01-01', 30),
                    (14, '2027-01-01', 19),
                    (15, '2027-01-01', 21),
                    (16, '2027-01-01', 40),
                    (17, '2027-01-01', 35),
                    (18, '2027-01-01', 50),
                    (19, '2027-01-01', 60),
                    (20, '2027-01-01', 45),
                    (21, '2027-01-01', 30),
                    (22, '2027-01-01', 25),
                    (23, '2027-01-01', 40),
                    (24, '2027-01-01', 55),
                    (25, '2027-01-01', 20),
                    (26, '2027-01-01', 15),
                    (27, '2027-01-01', 15),
                    (28, '2027-01-01', 22),
                    (29, '2027-01-01', 80),
                    (30, '2027-01-01', 40),
                    (31, '2027-01-01', 30),
                    (32, '2027-01-01', 100),
                    (33, '2027-01-01', 45),
                    (34, '2027-01-01', 30),
                    (35, '2027-01-01', 25);
                END

                -- Users (explicitly mapped by ID)
                IF NOT EXISTS (SELECT 1 FROM dbo.Users)
                BEGIN
                    SET IDENTITY_INSERT dbo.Users ON;
                    
                    INSERT INTO dbo.Users (Id, Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption) VALUES
                    (1, N'admin@pharmacy.local', N'0700000000', N'hashed_pwd_admin', 0, 1, N'admin_super',  N'Admin',      1, 1000, '1900-01-01', 28, 5, 0),
                    (2, N'johndoe@test.com',     N'0711111111', N'hashed_pwd_john',  0, 0, N'johndoe',      N'Client',     1,  150, '1900-01-01', 28, 5, 0),
                    (3, N'janedoe@test.com',     N'0722222222', N'hashed_pwd_jane',  0, 0, N'janedoe',      N'Client',     0,   45, '2026-01-01', 28, 5, 0),
                    (4, N'house@hospital.local', N'0733333333', N'hashed_pwd_house', 0, 0, N'dr_house',     N'Doctor',     0,    0, '1900-01-01', 28, 5, 0),
                    (5, N'jamie@hospital.local', N'0744444444', N'hashed_pwd_jamie', 0, 0, N'jamie_pharm',  N'Pharmacist', 0,    0, '1900-01-01', 28, 5, 0),
                    (6, N'paul@gmail.com', N'0744444444', N'abc123', 0, 1, N'paul',  N'Admin', 0,    0, '1900-01-01', 28, 5, 0),
                    (7, N'paull@gmail.com', N'0744444445', N'abc123', 0, 0, N'Paul',  N'Client', 0,    0, '1900-01-01', 28, 5, 0);
                    
                    SET IDENTITY_INSERT dbo.Users OFF;
                END

                -- UserDiscounts
                IF NOT EXISTS (SELECT 1 FROM dbo.UserDiscounts)
                BEGIN
                    INSERT INTO dbo.UserDiscounts (UserId, ItemId, DiscountPercentage) VALUES
                    (2,  1,  5.00),
                    (3, 14, 15.00);
                END

                -- UserNotifications
                IF NOT EXISTS (SELECT 1 FROM dbo.UserNotifications)
                BEGIN
                    INSERT INTO dbo.UserNotifications (UserId, ItemId, IsFavorite, IsStockAlert) VALUES
                    (2,  5, 1, 0),
                    (2, 11, 0, 1),
                    (3, 14, 1, 1);
                END

                -- PeriodNotes
                IF NOT EXISTS (SELECT 1 FROM dbo.PeriodNotes)
                BEGIN
                    INSERT INTO dbo.PeriodNotes (UserId, NoteId, NoteBody, IsDone) VALUES
                    (3, 1, N'Take magnesium supplement', 1),
                    (3, 2, N'Drink herbal relax tea',     0),
                    (3, 3, N'Buy more Femina Comfort',    0);
                END

                -- Orders
                IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
                BEGIN
                    SET IDENTITY_INSERT dbo.Orders ON;
                    
                    INSERT INTO dbo.Orders (Id, ClientId, IsCompleted, IsExpired, PickUpDate) VALUES
                    (1, 2, 1, 0, '2026-04-15'),
                    (2, 3, 0, 0, '2026-04-25'),
                    (3, 2, 0, 1, '2026-03-10');
                    
                    SET IDENTITY_INSERT dbo.Orders OFF;
                END

                -- OrderItems
                IF NOT EXISTS (SELECT 1 FROM dbo.OrderItems)
                BEGIN
                    INSERT INTO dbo.OrderItems (OrderId, ItemId, OrderQuantity, Price) VALUES
                    (1,  1, 2, 28.50),
                    (1,  5, 1, 22.00),
                    (2, 14, 1, 29.50),
                    (2, 15, 2, 23.50),
                    (3, 11, 1, 31.00);
                END

            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional: You can put DELETE statements here if you want to undo the seed.
        }
    }
}
