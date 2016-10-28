IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc3' AND  TABLE_NAME = 'Player'))
BEGIN
       
	CREATE TABLE [sc3].[Player](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[TeamID] [bigint] NOT NULL, 
		[Name] [nvarchar](255) NOT NULL 
	)   
	
END
 