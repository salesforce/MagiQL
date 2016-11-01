CREATE TABLE [dbo].[ReportColumnMapping](
	[ID] [INT] IDENTITY(10000,1) NOT NULL,
	[DataSourceTypeId] [INT] NOT NULL,
	[OrganizationId] [INT] NULL,
	[CreatedByUserId] [INT] NULL,
	[IsPrivate] [BIT] NOT NULL CONSTRAINT [DF_ReportColumnMapping_IsPrivate]  DEFAULT ((0)),
	[UniqueName] [NVARCHAR](100) NOT NULL,
	[DisplayName] [NVARCHAR](100) NOT NULL,
	[MainCategory] [NVARCHAR](100) NOT NULL,
	[SubCategory] [NVARCHAR](100) NULL,
	[KnownTable] [NVARCHAR](50) NOT NULL,
	[FieldName] [NVARCHAR](1000) NOT NULL,
	[IsCalculated] [BIT] NOT NULL CONSTRAINT [DF_ReportColumnMapping_IsCalculated]  DEFAULT ((0)),
	[LifetimeFieldName] [NVARCHAR](1000) NULL,
	[FieldAggregationMethod] [NVARCHAR](50) NULL,
	[DbType] [NVARCHAR](255) NOT NULL,
	[Precision] [INT] NULL,
	[DataFormat] [NVARCHAR](50) NULL,
	[Selectable] [BIT] NOT NULL,
	[ActionSpecId] [INT] NULL,
	[Description] [NVARCHAR](1000) NULL,
	[AdditionalMetaDataJson] [NVARCHAR](MAX) NULL,
	[OldId] [INT] NULL,
 CONSTRAINT [PK_ReportColumnMapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
