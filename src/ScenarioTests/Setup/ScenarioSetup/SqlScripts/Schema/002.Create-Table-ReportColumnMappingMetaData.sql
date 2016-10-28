IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ReportColumnMappingMetaData'))
BEGIN
   
 
	CREATE TABLE [dbo].[ReportColumnMappingMetaData](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[ReportColumnMappingID] [int] NOT NULL,
		[Name] [nvarchar](100) NOT NULL,
		[Value] [nvarchar](100) NOT NULL,
	 CONSTRAINT [PK_ReportColumnMappingMetaData] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC,
		[ReportColumnMappingID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	--GO

	ALTER TABLE [dbo].[ReportColumnMappingMetaData]  WITH CHECK ADD  CONSTRAINT [FK_ReportColumnMappingMetaData_ReportColumnMapping__ReportColumnMappingID] FOREIGN KEY([ReportColumnMappingID])
	REFERENCES [dbo].[ReportColumnMapping] ([ID])
	--GO

	ALTER TABLE [dbo].[ReportColumnMappingMetaData] CHECK CONSTRAINT [FK_ReportColumnMappingMetaData_ReportColumnMapping__ReportColumnMappingID]
	--GO


END

