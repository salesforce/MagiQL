-- select * from reportcolumnmapping  where uniquename like '%_Count'
-- delete from reportcolumnmapping where uniquename like '%_Count'

  
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
3 as [DataSourceTypeId]
, CONCAT(c.table_name, '_Count') as [UniqueName]
, CONCAT(c.table_name, ' Count') as [DisplayName]
, c.table_name as [MainCategory]
, c.table_name as [KnownTable]
, 'COUNT()' as [FieldName]
, 1 as [IsCalculated]
, 'Sum' as [FieldAggregationMethod]
, 'Int32' as [DbType]
, 1 as [Selectable]
FROM [MagiQL].[INFORMATION_SCHEMA].[COLUMNS] c
 
 where not exists (select 1 from reportcolumnmapping where knowntable = c.table_name and uniquename like '%_Count')
 and  table_name in ('Team','Player','Match', 'PlayerPhysicalAttributes', 'PlayerAchievements', 'PlayerMatchStatistics')
 group by table_name
  
 order by table_name 