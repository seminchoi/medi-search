IF
NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Hospital')
BEGIN
CREATE TABLE Hospital
(
    Id       INT PRIMARY KEY IDENTITY(1,1),
    Code     NVARCHAR(100),
    Name     NVARCHAR(100) NOT NULL,
    Address  NVARCHAR(200),
    Location GEOGRAPHY
);

CREATE
SPATIAL INDEX IX_Location ON Hospital(Location);
END

IF
NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HospitalHour')
BEGIN
CREATE TABLE HospitalHour
(
    Id         INT PRIMARY KEY IDENTITY(1,1),
    HospitalId INT NOT NULL,
    MonStart   NVARCHAR(4),
    MonEnd     NVARCHAR(4),
    TuesStart  NVARCHAR(4),
    TuesEnd    NVARCHAR(4),
    WedStart   NVARCHAR(4),
    WedEnd     NVARCHAR(4),
    ThursStart NVARCHAR(4),
    ThursEnd   NVARCHAR(4),
    FriStart   NVARCHAR(4),
    FriEnd     NVARCHAR(4),
    SatStart   NVARCHAR(4),
    SatEnd     NVARCHAR(4),
    SunStart   NVARCHAR(4),
    SunEnd     NVARCHAR(4),

    CONSTRAINT FK_Hospital FOREIGN KEY (HospitalId) REFERENCES Hospital (Id)
);
END

IF
NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DrugStore')
BEGIN
CREATE TABLE DrugStore
(
    Id       INT PRIMARY KEY IDENTITY(1,1),
    Name     NVARCHAR(100) NOT NULL,
    Code     NVARCHAR(100),
    Address  NVARCHAR(200),
    Location GEOGRAPHY
);

CREATE
SPATIAL INDEX Ix_Location ON DrugStore(Location);
END

IF
NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DrugStoreHour')
BEGIN
CREATE TABLE DrugStoreHour
(
    Id          INT PRIMARY KEY IDENTITY(1,1),
    DrugStoreId INT NOT NULL,
    MonStart    NVARCHAR(4),
    MonEnd      NVARCHAR(4),
    TuesStart   NVARCHAR(4),
    TuesEnd     NVARCHAR(4),
    WedStart    NVARCHAR(4),
    WedEnd      NVARCHAR(4),
    ThursStart  NVARCHAR(4),
    ThursEnd    NVARCHAR(4),
    FriStart    NVARCHAR(4),
    FriEnd      NVARCHAR(4),
    SatStart    NVARCHAR(4),
    SatEnd      NVARCHAR(4),
    SunStart    NVARCHAR(4),
    SunEnd      NVARCHAR(4),

    CONSTRAINT FK_DrugStore FOREIGN KEY (DrugStoreId) REFERENCES DrugStore (Id)
);
END