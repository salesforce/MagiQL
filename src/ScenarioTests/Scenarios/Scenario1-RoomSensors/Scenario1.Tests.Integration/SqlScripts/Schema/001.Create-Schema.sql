IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'sc1')
BEGIN
EXEC('CREATE SCHEMA sc1')
END