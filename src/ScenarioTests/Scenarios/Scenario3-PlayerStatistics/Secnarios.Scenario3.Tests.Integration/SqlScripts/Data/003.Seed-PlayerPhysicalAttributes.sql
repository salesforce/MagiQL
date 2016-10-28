-- 5.5ft = 167cm     9st = 57kg
-- 6ft = 180cm       10st = 63kg
-- 6ft2 = 187cm      12 st = 76kg  

TRUNCATE TABLE [sc3].[PlayerPhysicalAttributes]
  
INSERT INTO [sc3].[PlayerPhysicalAttributes]
([PlayerID],[HeightCentimetres],[WeightKG],[IsMale])
	VALUES
-- TEAM A
(1 ,172 ,68.2 ,1),
(2 ,178 ,67.4 ,1),
(3 ,170 ,70.1 ,1),
(4 ,166 ,60.4 ,1),
(5 ,184 ,71.0 ,1),
-- TEAM B
(6 ,173 ,68.2 ,1),
(7 ,178 ,68.2 ,1),
(8 ,176 ,68.2 ,1),
(9 ,185 ,68.2 ,1),
(10,182 ,68.2 ,1),
-- TEAM C
(11,166 ,58.1 ,0),
(12,169 ,57.9 ,0),
(13,168 ,57.4 ,0),
(14,171 ,63.2 ,0),
(15,167 ,58.6 ,0),
-- TEAM D
(16,180 ,72.3 ,1),
(17,179 ,65.4 ,1),
(18,182 ,64.5 ,1),
(19,166 ,62.3 ,1),
(20,171 ,66.8 ,1)




