IF
NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LogApp')
BEGIN
    CREATE
DATABASE LogApp;
END