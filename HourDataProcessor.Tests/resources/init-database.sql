IF
NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InitTestLogAppSem')
    CREATE DATABASE InitTestLogAppSem;
           
IF
NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TestLogAppSem')
    CREATE DATABASE TestLogAppSem;