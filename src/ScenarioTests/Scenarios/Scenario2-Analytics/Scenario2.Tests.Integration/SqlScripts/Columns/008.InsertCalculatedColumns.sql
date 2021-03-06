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
, 'Calculated_HtmlBytes' as [UniqueName]
, 'Html Bytes' as [DisplayName]
, 'Calculated' as [MainCategory]
, 'LocationHit' as [KnownTable]
, 'IFTHENELSE(LocationHit_IsHtml == 1, LocationHit_ResponseSizeBytes_Sum,0)' as [FieldName]
, 1 as [IsCalculated]
, 'Average' as [FieldAggregationMethod]
, 'Decimal' as [DbType]
, 1 as [Selectable] 


 