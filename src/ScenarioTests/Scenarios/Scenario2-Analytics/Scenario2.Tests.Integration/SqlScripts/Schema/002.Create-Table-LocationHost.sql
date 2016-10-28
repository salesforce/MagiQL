IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc2' AND  TABLE_NAME = 'LocationHost'))
BEGIN      
 
	CREATE TABLE [sc2].[LocationHost](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[Host] [nvarchar](255) NOT NULL
	)  
	
END