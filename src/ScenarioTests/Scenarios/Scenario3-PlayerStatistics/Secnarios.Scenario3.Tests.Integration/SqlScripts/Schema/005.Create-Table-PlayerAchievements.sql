IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc3' AND  TABLE_NAME = 'PlayerAchievements'))
BEGIN
       
	CREATE TABLE [sc3].[PlayerAchievements](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[PlayerID] [bigint] NOT NULL, 
		[AwardedDate] [smalldatetime] NOT NULL, 
		[Description] [nvarchar](255) NOT NULL,
		[PrizeMoney] [decimal](6,2)
	)    
	
END
 