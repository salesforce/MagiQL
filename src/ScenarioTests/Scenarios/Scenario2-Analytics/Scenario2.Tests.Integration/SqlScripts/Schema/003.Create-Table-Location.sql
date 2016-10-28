IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc2' AND  TABLE_NAME = 'Location'))
BEGIN
       
	CREATE TABLE [sc2].[Location](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[LocationHostID] [bigint] NOT NULL, 
		[Path] [nvarchar](255) NOT NULL

	)  
	
END
 