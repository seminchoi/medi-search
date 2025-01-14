IF
NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Institution')
BEGIN
CREATE TABLE Institution
(
    Id       INT PRIMARY KEY IDENTITY(1,1),
    Code     NVARCHAR(100),
    Name     NVARCHAR(100) NOT NULL,
    Address  NVARCHAR(200),
    PhoneNumber VARCHAR(30),
    InstitutionType VARCHAR(16),
    Location GEOGRAPHY
);

CREATE SPATIAL INDEX IX_Location ON Institution(Location);
CREATE NONCLUSTERED INDEX IX_Name ON Institution(Name);
CREATE NONCLUSTERED INDEX IX_Code ON Institution(Code);
END

IF
NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'InstitutionHour')
BEGIN
CREATE TABLE InstitutionHour
(
    Id         INT PRIMARY KEY IDENTITY(1,1),
    InstitutionId INT NOT NULL,
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

    CONSTRAINT FK_Institution FOREIGN KEY (InstitutionId) REFERENCES Institution (Id)
);
END