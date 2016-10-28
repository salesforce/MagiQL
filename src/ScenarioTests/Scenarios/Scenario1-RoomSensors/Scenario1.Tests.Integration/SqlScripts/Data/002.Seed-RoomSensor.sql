TRUNCATE TABLE sc1.roomsensor
 
INSERT INTO [sc1].[RoomSensor]
([RoomID],[LastUpdated],[TemperatureCelcius],[HumidityPercent],[NoiseDecibels],[IsLight])
VALUES
-- house 1
(1,  GetUtcDate(),  20, 30.5, 12.2, 1),
(2,  GetUtcDate(),  21, 37.2, 33.2, 1),
(3,  GetUtcDate(),  24, 32.1, 12.0, 0),
(4,  GetUtcDate(),  19, 28.8, 11.5, 0),
(5,  GetUtcDate(),  18, 28.9, 44.7, 1),
--house 2
(6,  GetUtcDate(),  22, 33.7, 15.5, 0),
(7,  GetUtcDate(),  20, 37.4, 29.9, 1),
(8,  GetUtcDate(),  26, 39.1, 13.2, 0)
 
