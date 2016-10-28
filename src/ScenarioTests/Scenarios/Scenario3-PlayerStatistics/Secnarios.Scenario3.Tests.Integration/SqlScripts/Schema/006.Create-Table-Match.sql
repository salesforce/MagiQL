IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc3' AND  TABLE_NAME = 'Match'))
BEGIN
       
	CREATE TABLE [sc3].[Match](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[HomeTeamID] [bigint] NOT NULL, 
		[AwayTeamID] [bigint] NOT NULL, 
		[KickOffTimeUTC] [DateTime] NULL 
	)   
	
END 