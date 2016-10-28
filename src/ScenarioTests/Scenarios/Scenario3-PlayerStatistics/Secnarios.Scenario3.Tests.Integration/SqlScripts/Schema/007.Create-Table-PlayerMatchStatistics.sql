IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc3' AND  TABLE_NAME = 'PlayerMatchStatistics'))
BEGIN
       
	CREATE TABLE [sc3].[PlayerMatchStatistics](
		[ID] [bigint] IDENTITY(1,1) NOT NULL, 
		[MatchID] [bigint] NOT NULL,   
		[PlayerID] [bigint] NOT NULL,   
		[StartMinute] [int] NOT NULL,
		[EndMinute] [int] NOT NULL,
		[Goals] [int] NOT NULL,
		[Assists] [int] NOT NULL,
		[Fouls] [int] NOT NULL,
		[Tackles] [int] NOT NULL,
		[YellowCard] [bit] NOT NULL,
		[RedCard] [bit] NOT NULL,		 
		[DistanceCoveredKilometres] [decimal](6,3) NOT NULL,
		[PerformanceRating] [decimal](6,3) NOT NULL,
		[ManOfTheMatch] [bit] NOT NULL
	)  
	
END
    
 
 