IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'sc1' AND  TABLE_NAME = 'RoomSensor'))
BEGIN
       
	CREATE TABLE [sc1].[RoomSensor](
		[RoomID] [bigint] NOT NULL,
		LastUpdated [Datetime] NOT NULL,
		TemperatureCelcius [int] NOT NULL,
		HumidityPercent [decimal](5, 2) NOT NULL,
		NoiseDecibels  [decimal](5, 2) NOT NULL,
		IsLight [bit] NOT NULL
	)  
	
END

