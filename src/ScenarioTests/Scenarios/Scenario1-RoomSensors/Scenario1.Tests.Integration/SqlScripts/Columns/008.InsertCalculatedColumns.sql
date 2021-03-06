-- select * from reportcolumnmapping  where uniquename like 'Calculated_%'
-- delete from reportcolumnmapping where uniquename like 'Calculated_%'

  
INSERT INTO [dbo].[ReportColumnMapping]
           ([DataSourceTypeId]
           ,[UniqueName]
           ,[DisplayName]
           ,[MainCategory]
           ,[KnownTable]
           ,[FieldName]
           ,[IsCalculated]
           ,[FieldAggregationMethod]
           ,[DbType]
		   ,[Selectable]
           ) 
SELECT 
1 as [DataSourceTypeId]
, 'Calculated_TemperaturePerCubicMetre' as [UniqueName]
, 'Temperature Per CubicMetre' as [DisplayName]
, 'Calculated' as [MainCategory]
, 'RoomSensor' as [KnownTable]
, 'RoomSensor_TemperatureCelcius_Sum / Room_VolumeCubicMetres_Sum' as [FieldName]
, 1 as [IsCalculated]
, 'Average' as [FieldAggregationMethod]
, 'Decimal' as [DbType]
, 1 as [Selectable] 

UNION

SELECT 
1 as [DataSourceTypeId]
, 'Calculated_IsLightCount' as [UniqueName]
, 'IsLight Count' as [DisplayName]
, 'Calculated' as [MainCategory]
, 'RoomSensor' as [KnownTable]
, 'IFTHENELSE(RoomSensor_IsLight == 1,1,0)' as [FieldName]
, 1 as [IsCalculated]
, 'Sum' as [FieldAggregationMethod]
, 'Int32' as [DbType]
, 1 as [Selectable] 

UNION 

SELECT 
1 as [DataSourceTypeId]
, 'Calculated_HumidityPercentTimesTemperatureCelciusMin' as [UniqueName]
, 'Humidity Times Temperature' as [DisplayName]
, 'Calculated' as [MainCategory]
, 'RoomSensor' as [KnownTable]
, 'RoomSensor_HumidityPercent * RoomSensor_TemperatureCelcius' as [FieldName]
, 1 as [IsCalculated]
, 'Average' as [FieldAggregationMethod]
, 'Decimal' as [DbType]
, 1 as [Selectable] 

UNION 

SELECT 
1 as [DataSourceTypeId]
, 'Calculated_TemperatureVariance' as [UniqueName]
, 'Temperature Variance' as [DisplayName]
, 'Calculated' as [MainCategory]
, 'RoomSensor' as [KnownTable]
, 'RoomSensor_TemperatureCelcius_Max - RoomSensor_TemperatureCelcius' as [FieldName]
, 1 as [IsCalculated]
, 'Max' as [FieldAggregationMethod]
, 'Int32' as [DbType]
, 1 as [Selectable] 
