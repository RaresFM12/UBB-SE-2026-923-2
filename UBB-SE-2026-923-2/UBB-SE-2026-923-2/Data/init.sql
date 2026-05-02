create database Pharmacy
go
use Pharmacy
go

create table Substances(
	name varchar(255) primary key,
	lethalDose decimal(10,2),
	description varchar(200)
)

create table Items(
	--ids will be autoincremented
	itemId int identity(1,1) primary key,
	name varchar(255) not null,
	price decimal(10,2),
	category varchar(255),
	numberOfPills int,
	producer varchar(255),
	--image somehow
	imagePath varchar(255),
	quantity int,
	label varchar(255),
	description varchar(255),
	discountPercentage decimal(10,2)
)

create table ItemSubstances(
	itemId int references Items(itemId),
	name varchar(255) references Substances(name),
	concentration decimal(10,2),
	primary key (itemId,name)
)

create table ItemExpirationDates(
	itemId int references Items(itemId),
	expirationDate date,
	numberOfPacks int,
	primary key (itemId,expirationDate)
)

create table Users(
	userId int identity(1,1) primary key,
	email varchar(255) unique,
	phoneNumber varchar(255),
	passwordHash varchar(255),
	isDisabled bit not null,
	isAdmin bit not null,
	username varchar(255),
	discountNotifications bit not null,
	--loyalty points? do we have these? cannot find them in features
	loyaltyPoints int
)

SELECT * FROM Users

create table UserDiscounts(
	userId int references Users(userId),
	itemId int references Items(itemId),
	itemDiscountPercentage decimal(10,2),
	primary key(userId,itemId)
)

create table UserNotifications(
	userId int references Users(userId),
	itemId int references Items(itemId),
	--favouriteItem? noi mai avem astea macar?
	favouriteItem bit not null,
	stockAlert bit not null,
	primary key(userId,itemId)
)

create table PeriodNotes(
	userId int references Users(userId),
	noteId int,
	noteBody varchar(255),
	isDone bit not null,
	primary key(userId,noteId)

)
create table PeriodTrackers(
	userId int references Users(userId) primary key,
	startPeriodDate date,
	cycleDays int,
	periodLasts int,
	PremenstrualSyndromeOption int
)

DROP TABLE PeriodTrackers

create table Orders(
	orderId int identity(1,1) primary key,
	clientId int references Users(userId),
	isCompleted bit not null,
	isExpired bit not null,
	pickUpDate date
)

SELECT * FROM PeriodTrackers

create table OrderItems(
	orderId int references Orders(orderId),
	itemId int references Items(itemId),
	orderQuantity int,
	price decimal(10,2),
	primary key(orderId,itemId)
)

-- some inserts
INSERT INTO Substances(name, lethalDose, description)
VALUES
	('Ibuprofen', 3200.00, 'Anti-inflammatory pain reliever'),
	('Paracetamol', 4000.00, 'Pain reliever and fever reducer'),
	('Magnesium', 2500.00, 'Mineral supplement for muscle and nerve support'),
	('Iron', 45.00, 'Mineral supplement used for iron deficiency'),
	('Vitamin C', 2000.00, 'Vitamin supplement for immune support'),
	('Calcium', 2500.00, 'Mineral supplement for bones and muscles'),
	('Omega 3', 3000.00, 'Fatty acid supplement for heart and brain health'),
	('Melatonin', 10.00, 'Sleep support supplement'),
	('Probiotics', 1000.00, 'Digestive support supplement'),
	('Zinc', 40.00, 'Mineral supplement for immunity');

INSERT INTO Items
(name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage)
VALUES
	('Nurofen Express', 28.50, 'pain relief', 20, 'Reckitt', 'Assets/nurofen.png', 40, 'Fast pain relief', 'Ibuprofen capsules for pain and inflammation', 0),
	('Panadol Extra', 19.99, 'pain relief', 16, 'GSK', 'Assets/panadol.png', 35, 'Extra strength', 'Paracetamol tablets for headaches and fever', 10),
	('Magne B6', 32.00, 'wellness', 50, 'Sanofi', 'Assets/magneb6.png', 25, 'Magnesium support', 'Magnesium and vitamin B6 supplement', 0),
	('Feroglobin', 36.50, 'wellness', 30, 'Vitabiotics', 'Assets/feroglobin.png', 18, 'Iron formula', 'Iron supplement for energy and blood health', 5),
	('Vitamin C 1000', 22.00, 'wellness', 20, 'NaturPharma', 'Assets/vitaminc.png', 50, 'Immune support', 'High strength vitamin C tablets', 0),
	('Calcium + D3', 27.50, 'wellness', 30, 'BioFarm', 'Assets/calciumd3.png', 22, 'Bone support', 'Calcium and vitamin D3 supplement', 15),
	('Omega 3 Forte', 45.00, 'wellness', 60, 'Doppelherz', 'Assets/omega3.png', 14, 'Heart support', 'Omega 3 capsules for heart and brain', 0),
	('Melatonin Sleep', 18.00, 'wellness', 30, 'Walmark', 'Assets/melatonin.png', 12, 'Sleep support', 'Melatonin tablets for better sleep', 0),
	('Probiotic Balance', 39.99, 'wellness', 20, 'Secom', 'Assets/probiotic.png', 16, 'Digestive comfort', 'Daily probiotic capsules', 20),
	('Zinc Complex', 21.50, 'wellness', 30, 'NaturMil', 'Assets/zinc.png', 28, 'Immune defense', 'Zinc supplement for immune support', 0),
	('Coldrex MaxGrip', 31.00, 'cold and flu', 10, 'GSK', 'Assets/coldrex.png', 20, 'Cold relief', 'Powder for cold and flu symptoms', 0),
	('Strepsils Intensive', 24.00, 'cold and flu', 24, 'Reckitt', 'Assets/strepsils.png', 17, 'Sore throat relief', 'Lozenges for sore throat', 0),
	('No-Spa Forte', 26.00, 'pain relief', 24, 'Sanofi', 'Assets/nospa.png', 30, 'Cramp relief', 'Drotaverine tablets for cramps', 0),
	('Femina Comfort', 29.50, 'wellness', 30, 'HerbalLab', 'Assets/femina.png', 19, 'Period wellness', 'Supplement designed for menstrual comfort', 10),
	('Herbal Relax Tea Capsules', 23.50, 'wellness', 20, 'PlantMed', 'Assets/herbalrelax.png', 21, 'Relax support', 'Natural calming capsules for stress relief', 0);

INSERT INTO ItemSubstances(itemId, name, concentration)
VALUES
	(1, 'Ibuprofen', 400.00),
	(2, 'Paracetamol', 500.00),
	(3, 'Magnesium', 250.00),
	(4, 'Iron', 14.00),
	(5, 'Vitamin C', 1000.00),
	(6, 'Calcium', 500.00),
	(7, 'Omega 3', 1000.00),
	(8, 'Melatonin', 5.00),
	(9, 'Probiotics', 200.00),
	(10, 'Zinc', 10.00),
	(11, 'Paracetamol', 1000.00),
	(12, 'Ibuprofen', 8.75),
	(13, 'Ibuprofen', 80.00),
	(14, 'Magnesium', 150.00),
	(14, 'Vitamin C', 80.00),
	(15, 'Magnesium', 100.00);


INSERT INTO ItemExpirationDates(itemId, expirationDate, numberOfPacks)
VALUES
	(1, '2026-08-15', 20),
	(1, '2027-01-10', 20),

	(2, '2026-09-20', 15),
	(2, '2027-02-15', 20),

	(3, '2026-10-05', 10),
	(3, '2027-03-01', 15),

	(4, '2026-11-12', 8),
	(4, '2027-04-18', 10),

	(5, '2026-07-30', 25),
	(5, '2027-01-25', 25),

	(6, '2026-12-10', 10),
	(6, '2027-05-05', 12),

	(7, '2026-09-01', 6),
	(7, '2027-06-14', 8),

	(8, '2026-08-22', 5),
	(8, '2027-02-28', 7),

	(9, '2026-10-18', 8),
	(9, '2027-03-20', 8),

	(10, '2026-11-30', 12),
	(10, '2027-04-30', 16),

	(11, '2026-09-09', 10),
	(11, '2027-01-19', 10),

	(12, '2026-10-25', 7),
	(12, '2027-05-10', 10),

	(13, '2026-08-08', 15),
	(13, '2027-02-02', 15),

	(14, '2026-12-22', 9),
	(14, '2027-06-01', 10),

	(15, '2026-09-17', 10),
	(15, '2027-03-11', 11);

SELECT * FROM Items;
SELECT * FROM Items WHERE category = 'wellness';
SELECT * FROM ItemSubstances;
SELECT * FROM ItemExpirationDates;
SELECT * FROM PeriodTrackers;

SELECT * FROM Users

-- 1. Insert Users (1 Admin, 2 Regular Users)
INSERT INTO Users(email, phoneNumber, passwordHash, isDisabled, isAdmin, username, discountNotifications, loyaltyPoints)
VALUES
	('admin@pharmacy.local', '0700000000', 'hashed_pwd_admin', 0, 1, 'admin_super', 1, 1000),
	('johndoe@test.com', '0711111111', 'hashed_pwd_john', 0, 0, 'johndoe', 1, 150),
	('janedoe@test.com', '0722222222', 'hashed_pwd_jane', 0, 0, 'janedoe', 0, 45);

-- 2. Insert User Discounts (Giving specific users extra discounts on specific items)
-- Note: Assuming johndoe is userId 2, janedoe is userId 3
INSERT INTO UserDiscounts(userId, itemId, itemDiscountPercentage)
VALUES
	(2, 1, 5.00),  -- John gets 5% off Nurofen
	(3, 14, 15.00); -- Jane gets 15% off Femina Comfort

-- 3. Insert User Notifications (Favorites and Stock Alerts)
INSERT INTO UserNotifications(userId, itemId, favouriteItem, stockAlert)
VALUES
	(2, 5, 1, 0), -- John favorited Vitamin C
	(2, 11, 0, 1), -- John wants an alert when Coldrex is restocked
	(3, 14, 1, 1); -- Jane favorited Femina Comfort and wants stock alerts

-- 4. Insert Period Trackers (For female users/tracking features)
INSERT INTO PeriodTrackers(userId, startPeriodDate, cycleDays, periodLasts, PremenstrualSyndromeOption)
VALUES
	(3, '2026-04-10', 28, 5, 2); -- Jane's tracker data

-- 5. Insert Period Notes
INSERT INTO PeriodNotes(userId, noteId, noteBody, isDone)
VALUES
	(3, 1, 'Take magnesium supplement', 1),
	(3, 2, 'Drink herbal relax tea', 0),
	(3, 3, 'Buy more Femina Comfort', 0);

-- 6. Insert Orders 
-- Order 1: Completed, Order 2: Pending, Order 3: Expired/Abandoned
INSERT INTO Orders(clientId, isCompleted, isExpired, pickUpDate)
VALUES
	(2, 1, 0, '2026-04-15'), -- John's completed order
	(3, 0, 0, '2026-04-25'), -- Jane's pending order
	(2, 0, 1, '2026-03-10'); -- John's expired/forgotten order

-- 7. Insert Order Items (Linking items to the orders above)
-- OrderId 1 (John's completed order)
INSERT INTO OrderItems(orderId, itemId, orderQuantity, price)
VALUES
	(1, 1, 2, 28.50), -- 2x Nurofen
	(1, 5, 1, 22.00); -- 1x Vitamin C

-- OrderId 2 (Jane's pending order)
INSERT INTO OrderItems(orderId, itemId, orderQuantity, price)
VALUES
	(2, 14, 1, 29.50), -- 1x Femina Comfort
	(2, 15, 2, 23.50); -- 2x Herbal Relax Tea

-- OrderId 3 (John's expired order)
INSERT INTO OrderItems(orderId, itemId, orderQuantity, price)
VALUES
	(3, 11, 1, 31.00); -- 1x Coldrex

-- Verify the new data
SELECT * FROM Users;
SELECT * FROM Orders;
SELECT * FROM OrderItems;

-- 1. Insert New Substances
INSERT INTO Substances(name, lethalDose, description)
VALUES
	('Cetirizine', 500.00, 'Antihistamine for allergy relief'),
	('Loratadine', 1000.00, 'Non-drowsy antihistamine'),
	('Loperamide', 60.00, 'Medication to decrease frequency of diarrhea'),
	('Simethicone', 2000.00, 'Anti-foaming agent to reduce bloating and gas'),
	('Diclofenac', 1500.00, 'Nonsteroidal anti-inflammatory drug (NSAID)'),
	('Dexpanthenol', 5000.00, 'Skin protectant and moisturizer'),
	('Vitamin D3', 50.00, 'Essential vitamin for bone health and immunity'),
	('Xylometazoline', 10.00, 'Decongestant for nasal passages'),
	('Acetylcysteine', 3000.00, 'Mucolytic agent to clear mucus');

-- 2. Insert 20 New Products (Items 16 through 35)
UPDATE Items
SET quantity = 0
WHERE name = 'Zyrtec'

SELECT * FROM Items

INSERT INTO Items
(name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage)
VALUES
	-- Allergy
	('Zyrtec', 25.50, 'allergy', 20, 'UCB', 'Assets/zyrtec.png', 40, '24 Hour Relief', 'Cetirizine tablets for indoor and outdoor allergies', 0),
	('Claritine', 24.00, 'allergy', 30, 'Bayer', 'Assets/claritine.png', 35, 'Non-Drowsy', 'Loratadine allergy relief tablets', 10),
	
	-- Digestion
	('Imodium', 18.50, 'digestion', 12, 'J&J', 'Assets/imodium.png', 50, 'Fast Acting', 'Loperamide capsules for diarrhea relief', 0),
	('Espumisan', 22.00, 'digestion', 50, 'Berlin-Chemie', 'Assets/espumisan.png', 60, 'Anti-Bloating', 'Simethicone capsules for gas relief', 5),
	('Colebil', 15.00, 'digestion', 20, 'Biofarm', 'Assets/colebil.png', 45, 'Bile Support', 'Digestive supplement after heavy meals', 0),
	('Smecta', 19.50, 'digestion', 10, 'Ipsen', 'Assets/smecta.png', 30, 'Digestive Protectant', 'Powder for oral suspension', 0),

	-- Topical / Skincare
	('Voltaren Gel', 35.00, 'pain relief', 1, 'GSK', 'Assets/voltaren.png', 25, 'Targeted Pain Relief', 'Diclofenac topical gel for joint and muscle pain', 15),
	('Bepanthen Ointment', 28.00, 'skincare', 1, 'Bayer', 'Assets/bepanthen.png', 40, 'Skin Repair', 'Dexpanthenol ointment for skin irritation and tattoos', 0),
	('Sudocrem', 26.50, 'skincare', 1, 'Teva', 'Assets/sudocrem.png', 55, 'Healing Cream', 'Antiseptic healing cream for diaper rash and eczema', 0),
	('Cerave Cleanser', 55.00, 'skincare', 1, 'L''Oreal', 'Assets/cerave.png', 20, 'Hydrating Formula', 'Daily facial cleanser with ceramides', 20),

	-- Wellness Expanded
	('Centrum Men', 65.00, 'wellness', 30, 'GSK', 'Assets/centrum_men.png', 15, 'Multivitamin', 'Complete daily multivitamin for men', 0),
	('Centrum Women', 65.00, 'wellness', 30, 'GSK', 'Assets/centrum_women.png', 15, 'Multivitamin', 'Complete daily multivitamin for women', 0),
	('Supradyn Energy', 48.00, 'wellness', 30, 'Bayer', 'Assets/supradyn.png', 22, 'Energy Support', 'Vitamins with CoQ10 for energy release', 10),
	('Vitamin D3 2000 IU', 15.99, 'wellness', 60, 'NaturPharma', 'Assets/vitamind3.png', 80, 'Sun Vitamin', 'High-dose Vitamin D3 softgels', 0),
	('B-Complex Forte', 21.00, 'wellness', 30, 'Zentiva', 'Assets/bcomplex.png', 40, 'Nerve Support', 'High strength B-vitamins', 0),

	-- First Aid
	('Betadine Solution', 18.00, 'first aid', 1, 'Egis', 'Assets/betadine.png', 30, 'Antiseptic', 'Povidone-iodine topical solution for wound care', 0),
	('Sterile Plasters', 12.50, 'first aid', 50, 'Urgo', 'Assets/plasters.png', 100, 'Waterproof', 'Assorted sizes of waterproof bandages', 0),

	-- Cold and Flu Expanded
	('Olynth Nasal Spray', 16.50, 'cold and flu', 1, 'J&J', 'Assets/olynth.png', 45, 'Decongestant', 'Xylometazoline spray for unblocking the nose', 0),
	('ACC 600', 29.00, 'cold and flu', 10, 'Sandoz', 'Assets/acc600.png', 30, 'Mucus Clearance', 'Effervescent tablets for productive coughs', 0),
	('Theraflu Extra', 33.00, 'cold and flu', 10, 'GSK', 'Assets/theraflu.png', 25, 'Severe Cold', 'Hot liquid powder for severe cold symptoms', 10);

-- 3. Link Substances to the New Products
INSERT INTO ItemSubstances(itemId, name, concentration)
VALUES
	(16, 'Cetirizine', 10.00),
	(17, 'Loratadine', 10.00),
	(18, 'Loperamide', 2.00),
	(19, 'Simethicone', 40.00),
	(22, 'Diclofenac', 1.00), -- Voltaren
	(23, 'Dexpanthenol', 5.00), -- Bepanthen
	(29, 'Vitamin D3', 50.00), -- Vit D3 2000 IU = 50mcg
	(33, 'Xylometazoline', 0.10), -- Olynth
	(34, 'Acetylcysteine', 600.00), -- ACC 600
	(35, 'Paracetamol', 650.00); -- Theraflu Extra contains Paracetamol too

-- 4. Give the New Products Expiration Dates (Assuming current year is 2024, expiring 2026/2027)
UPDATE ItemExpirationDates
SET expirationDate = '2026-02-02'
WHERE itemId = 16


INSERT INTO ItemExpirationDates(itemId, expirationDate, numberOfPacks)
VALUES
	(16, '2027-01-10', 20), (16, '2028-05-15', 20),
	(17, '2026-11-20', 15), (17, '2027-08-30', 20),
	(18, '2026-10-15', 25), (18, '2027-12-01', 25),
	(19, '2026-09-05', 30), (19, '2028-01-20', 30),
	(20, '2026-04-12', 20), (20, '2027-09-18', 25),
	(21, '2027-03-22', 15), (21, '2028-06-10', 15),
	(22, '2026-12-30', 12), (22, '2027-11-25', 13),
	(23, '2027-02-14', 20), (23, '2028-02-14', 20),
	(24, '2026-07-01', 25), (24, '2027-07-01', 30),
	(25, '2026-08-18', 10), (25, '2027-08-18', 10),
	(26, '2027-05-05', 7),  (26, '2028-04-10', 8),
	(27, '2026-11-11', 7),  (27, '2027-10-15', 8),
	(28, '2027-01-22', 11), (28, '2028-03-30', 11),
	(29, '2026-09-09', 40), (29, '2027-09-09', 40),
	(30, '2026-12-01', 20), (30, '2027-12-01', 20),
	(31, '2027-04-20', 15), (31, '2028-08-20', 15),
	(32, '2030-01-01', 50), (32, '2031-01-01', 50), -- Plasters last a long time
	(33, '2026-10-31', 20), (33, '2027-10-31', 25),
	(34, '2027-01-15', 15), (34, '2028-02-20', 15),
	(35, '2026-11-30', 10), (35, '2027-11-30', 15);

	UPDATE Users
	SET isAdmin = 1
	WHERE userId = 2

	SELECT * FROM Items

	