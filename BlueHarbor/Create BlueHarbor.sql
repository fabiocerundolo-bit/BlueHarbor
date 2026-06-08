CREATE DATABASE BlueHarbor;
GO

USE BlueHarbor;
GO

CREATE TABLE Ruolo (
    IdRuolo INT PRIMARY KEY IDENTITY(1,1),
    NomeRuolo VARCHAR(50) NOT NULL
);
GO

CREATE TABLE Dimensione (
    IdDimensione INT PRIMARY KEY IDENTITY(1,1),
    NomeDimensione VARCHAR(2) NOT NULL CHECK (NomeDimensione IN ('XL', 'L', 'M', 'S'))
);
GO

CREATE TABLE Utente (
    IdUtente INT PRIMARY KEY IDENTITY(1,1),
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    IdRuolo INT NOT NULL FOREIGN KEY REFERENCES Ruolo(IdRuolo)
);
GO



CREATE TABLE Banchina (
    IdBanchina INT PRIMARY KEY IDENTITY(1,1),
    NomeBanchina VARCHAR(50) NOT NULL,
    IdDimensione INT NOT NULL FOREIGN KEY REFERENCES Dimensione(IdDimensione)
);
GO



CREATE TABLE Nave (
    IdNave INT PRIMARY KEY IDENTITY(1,1),
    NomeNave VARCHAR(100) NOT NULL,
    GiornoArrivo INT NOT NULL,
    DurataOccupazione INT NOT NULL,
    Stato VARCHAR(10) NOT NULL CHECK (Stato IN ('Pending', 'Assigned', 'Departed')),
    Note VARCHAR(500),
    IdDimensione INT NOT NULL FOREIGN KEY REFERENCES Dimensione(IdDimensione),
    IdUtente INT NOT NULL FOREIGN KEY REFERENCES Utente(IdUtente)
);
GO

CREATE TABLE Occupazione (
    IdOccupazione INT PRIMARY KEY IDENTITY(1,1),
    GiornoInizio INT NOT NULL,
    IdNave INT NOT NULL FOREIGN KEY REFERENCES Nave(IdNave),
    IdBanchina INT NOT NULL FOREIGN KEY REFERENCES Banchina(IdBanchina),
    IdUtente INT NOT NULL FOREIGN KEY REFERENCES Utente(IdUtente)
);
GO