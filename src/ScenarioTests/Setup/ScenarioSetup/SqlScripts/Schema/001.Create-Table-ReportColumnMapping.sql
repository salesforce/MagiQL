IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ReportColumnMapping'))
BEGIN
   


	CREATE TABLE [dbo].[ReportColumnMapping](
		[ID] [int] IDENTITY(10000,1) NOT NULL,
		[DataSourceTypeId] [int] NOT NULL,
		[OrganizationId] [int] NULL,
		[CreatedByUserId] [int] NULL,
		[IsPrivate] [bit] NOT NULL CONSTRAINT [DF_ReportColumnMapping_IsPrivate]  DEFAULT ((0)),
		[UniqueName] [nvarchar](100) NOT NULL,
		[DisplayName] [nvarchar](100) NOT NULL,
		[MainCategory] [nvarchar](100) NOT NULL,
		[SubCategory] [nvarchar](100) NULL,
		[KnownTable] [nvarchar](50) NOT NULL,
		[FieldName] [nvarchar](1000) NOT NULL,
		[IsCalculated] [bit] NOT NULL CONSTRAINT [DF_ReportColumnMapping_IsCalculated]  DEFAULT ((0)),
		[LifetimeFieldName] [nvarchar](1000) NULL,
		[FieldAggregationMethod] [nvarchar](50) NULL,
		[DbType] [nvarchar](255) NOT NULL,
		[Precision] [int] NULL,
		[DataFormat] [nvarchar](50) NULL,
		[Selectable] [bit] NOT NULL,
		[ActionSpecId] [int] NULL,
		[Description] [nvarchar](1000) NULL,
		[AdditionalMetaDataJson] [nvarchar](max) NULL,
		[OldId] [int] NULL,
	 CONSTRAINT [PK_ReportColumnMapping] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


END

