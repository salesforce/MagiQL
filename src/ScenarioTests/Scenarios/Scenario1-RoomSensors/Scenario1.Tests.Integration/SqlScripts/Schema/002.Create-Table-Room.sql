IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc1' AND  TABLE_NAME = 'Room'))
BEGIN
      
 
	CREATE TABLE [sc1].[Room](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[HouseID] [bigint] NOT NULL,
		[Name] [nvarchar](255) NOT NULL,
		[VolumeCubicMetres] [decimal](5, 2) NOT NULL
	)  
	
END

