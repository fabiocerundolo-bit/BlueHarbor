USE BlueHarbor;
GO

-- Ruoli
INSERT INTO Ruolo (NomeRuolo) VALUES ('Operatore');
INSERT INTO Ruolo (NomeRuolo) VALUES ('Scheduler');
GO

-- Dimensioni
INSERT INTO Dimensione (NomeDimensione) VALUES ('XL');
INSERT INTO Dimensione (NomeDimensione) VALUES ('L');
INSERT INTO Dimensione (NomeDimensione) VALUES ('M');
INSERT INTO Dimensione (NomeDimensione) VALUES ('S');
GO

-- Banchine (1 XL, 1 L, 2 M, 4 S)
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina XL1', 1);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina L1', 2);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina M1', 3);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina M2', 3);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina S1', 4);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina S2', 4);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina S3', 4);
INSERT INTO Banchina (NomeBanchina, IdDimensione) VALUES ('Banchina S4', 4);
