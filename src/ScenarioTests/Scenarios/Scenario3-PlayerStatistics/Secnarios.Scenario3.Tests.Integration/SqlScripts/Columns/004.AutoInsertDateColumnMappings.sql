-- delete from reportcolumnmapping where maincategory = 'date'

/*
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
VALUES(
3 --as [DataSourceTypeId]
, 'Stat_Day' --as [UniqueName]
, 'Stat Day' --as [DisplayName]
, 'Date' --as [MainCategory]
, 'LocationHit' --as [KnownTable]
, 'ROUNDDATE(DAY,LocationHit_TimeStampUTC)' --as [FieldName]
, 1 --as [IsCalculated]
, 'Min' --as [FieldAggregationMethod]
, 'DateTime' --as [DbType]
, 1 --as [Selectable]
),
(
2 --as [DataSourceTypeId]
, 'Stat_Hour' --as [UniqueName]
, 'Stat Hour' --as [DisplayName]
, 'Date' --as [MainCategory]
, 'LocationHit' --as [KnownTable]
, 'ROUNDDATE(HOUR,LocationHit_TimeStampUTC)' --as [FieldName]
, 1 --as [IsCalculated]
, 'Min' --as [FieldAggregationMethod]
, 'DateTime' --as [DbType]
, 1 --as [Selectable]
)
*/
  