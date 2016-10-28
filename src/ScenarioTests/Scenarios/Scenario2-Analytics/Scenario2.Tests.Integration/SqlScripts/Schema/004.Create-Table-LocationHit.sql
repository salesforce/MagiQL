IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc2' AND  TABLE_NAME = 'LocationHit'))
BEGIN
       
	CREATE TABLE [sc2].[LocationHit](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[LocationID] [bigint] NOT NULL,   
		[TimeStampUTC] [DateTime] NOT NULL,
		[UserID] [uniqueidentifier] NOT NULL,
		[StatusCode] [int] NOT NULL,
		[ResponseDurationSeconds] [decimal](6,3) NOT NULL,
		[IsHtml] [bit] NOT NULL,
		[ResponseSizeBytes] [int] NOT NULL,
	)  
	
END