IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc3' AND  TABLE_NAME = 'PlayerPhysicalAttributes'))
BEGIN
       
	CREATE TABLE [sc3].[PlayerPhysicalAttributes](
		[PlayerID] [bigint] NOT NULL, 
		[HeightCentimetres] [int] NOT NULL, 
		[WeightKG] [decimal](6,2) NOT NULL,
		[IsMale] [bit]
	)   
	 

	
END
 