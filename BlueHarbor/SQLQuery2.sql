USE BlueHarbor;
GO

-- Roles
INSERT INTO Role (RoleName) VALUES ('Operator');
INSERT INTO Role (RoleName) VALUES ('Scheduler');
GO

-- Sizes
INSERT INTO Size (SizeName) VALUES ('XL');
INSERT INTO Size (SizeName) VALUES ('L');
INSERT INTO Size (SizeName) VALUES ('M');
INSERT INTO Size (SizeName) VALUES ('S');
GO

-- Berths (1 XL, 1 L, 2 M, 4 S)
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth XL1', 1);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth L1', 2);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth M1', 3);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth M2', 3);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth S1', 4);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth S2', 4);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth S3', 4);
INSERT INTO Berth (BerthName, SizeId) VALUES ('Berth S4', 4);
