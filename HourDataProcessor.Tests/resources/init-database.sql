IF
NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TestLogApp')
    CREATE DATABASE TestLogApp;