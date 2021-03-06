-- select * from reportcolumnmapping
--delete from reportcolumnmapping where datasourcetypeid = 2

  
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
2 as [DataSourceTypeId]
, CONCAT(c.table_name, '_', c.column_name) as [UniqueName]
, CONCAT(c.table_name, ' ', c.column_name) as [DisplayName]
, c.table_name as [MainCategory]
, c.table_name as [KnownTable]
, c.column_name as [FieldName]
, 0 as [IsCalculated]
,CASE 
	WHEN  data_type = 'bit' then 'Bit'    
	ELSE 'Min'  END
	as [FieldAggregationMethod]
, CASE
WHEN   data_type = 'int' then 'Int32'
WHEN   data_type = 'bigint' then 'Int64'
WHEN   data_type = 'decimal' then 'Decimal'
WHEN   data_type = 'datetime' then 'DateTime'
WHEN   data_type = 'bit' then 'Boolean'  
ELSE 'String' END 
as [DbType]
, 1 as [Selectable]
FROM [MagiQL].[INFORMATION_SCHEMA].[COLUMNS] c
 
 where not exists (select 1 from reportcolumnmapping where knowntable = c.table_name and fieldname = c.column_name)
 and  table_name in ('LocationHost','Location','LocationHit')
 order by table_name, ordinal_position

 
update [dbo].[ReportColumnMapping] 
set selectable = 0
where datasourcetypeid = 2 and uniquename = 'LocationHit_LocationID'