USE [AspectMining]
GO
/****** Object:  Table [dbo].[DatasetType]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DatasetType](
	[Id] [int] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DatasetType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Phrase]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phrase](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[POSId] [smallint] NOT NULL,
	[Text] [nvarchar](1500) NULL,
	[PreprocessedText] [nvarchar](1500) NULL,
 CONSTRAINT [PK_Phrase] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PhrasePolaritySentenceMapping]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhrasePolaritySentenceMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PhraseId] [int] NOT NULL,
	[SentenceId] [int] NOT NULL,
	[Polarity] [bit] NULL,
	[IsManual] [bit] NOT NULL,
 CONSTRAINT [PK_PhrasePolaritySentenceMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PhraseProductMapping]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhraseProductMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PhraseId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[PhraseText] [nvarchar](1500) NULL,
	[PhrasePreprocessedText] [nvarchar](1500) NULL,
	[Frequency] [int] NOT NULL,
	[Compactness] [int] NULL,
	[PSupport] [int] NULL,
	[IsPruned] [bit] NOT NULL,
	[IsManual] [bit] NOT NULL,
	[IsFrequent] [bit] NOT NULL,
	[PositiveReferences] [int] NOT NULL,
	[NegativeReferences] [int] NOT NULL,
	[NeutralReferences] [int] NOT NULL,
 CONSTRAINT [PK_PhraseProductMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PhraseTermMapping]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhraseTermMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PhraseId] [int] NOT NULL,
	[TermId] [int] NOT NULL,
 CONSTRAINT [PK_PhraseTermMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[POSType]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[POSType](
	[Id] [smallint] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_POSCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Product]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](1000) NULL,
	[Description] [ntext] NULL,
 CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Review]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Review](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Title] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Review] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Sentence]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sentence](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReviewId] [int] NOT NULL,
	[DatasetTypeId] [int] NOT NULL,
	[Text] [ntext] NULL,
	[TextPOS] [ntext] NULL,
	[ManualResults] [ntext] NULL,
 CONSTRAINT [PK_Sentence] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SentencePhraseMapping]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SentencePhraseMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SentenceId] [int] NOT NULL,
	[PhraseId] [int] NOT NULL,
	[IsGenerated] [bit] NOT NULL,
 CONSTRAINT [PK_SentencePhraseMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_SentencePhraseMapping] UNIQUE NONCLUSTERED 
(
	[SentenceId] ASC,
	[PhraseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SentencePhraseTermMapping]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SentencePhraseTermMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SentencePhraseId] [int] NOT NULL,
	[PhraseTermId] [int] NOT NULL,
	[TermOrder] [int] NULL,
 CONSTRAINT [PK_SentencePhraseTermMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Term]    Script Date: 18/4/2015 9:07:08 μμ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Term](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[POSId] [smallint] NOT NULL,
	[Text] [nvarchar](500) NULL,
	[PreprocessedText] [nvarchar](500) NULL,
 CONSTRAINT [PK_Term] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[PhrasePolaritySentenceMapping] ADD  CONSTRAINT [DF_PhrasePolaritySentenceMapping_IsManual]  DEFAULT ((0)) FOR [IsManual]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_Frequency]  DEFAULT ((0)) FOR [Frequency]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_Compactness]  DEFAULT ((0)) FOR [Compactness]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_PSupport]  DEFAULT ((0)) FOR [PSupport]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_IsPruned]  DEFAULT ((0)) FOR [IsPruned]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_IsManual]  DEFAULT ((0)) FOR [IsManual]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_IsFrequent]  DEFAULT ((0)) FOR [IsFrequent]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_PositiveReferences]  DEFAULT ((0)) FOR [PositiveReferences]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_NegativeReferences]  DEFAULT ((0)) FOR [NegativeReferences]
GO
ALTER TABLE [dbo].[PhraseProductMapping] ADD  CONSTRAINT [DF_PhraseProductMapping_NeutralReferences]  DEFAULT ((0)) FOR [NeutralReferences]
GO
ALTER TABLE [dbo].[SentencePhraseMapping] ADD  CONSTRAINT [DF_SentencePhraseMapping_Generated]  DEFAULT ((0)) FOR [IsGenerated]
GO
ALTER TABLE [dbo].[Phrase]  WITH CHECK ADD  CONSTRAINT [FK_Phrase_POSCode] FOREIGN KEY([POSId])
REFERENCES [dbo].[POSType] ([Id])
GO
ALTER TABLE [dbo].[Phrase] CHECK CONSTRAINT [FK_Phrase_POSCode]
GO
ALTER TABLE [dbo].[PhrasePolaritySentenceMapping]  WITH CHECK ADD  CONSTRAINT [FK_PhrasePolaritySentenceMapping_Phrase] FOREIGN KEY([PhraseId])
REFERENCES [dbo].[Phrase] ([Id])
GO
ALTER TABLE [dbo].[PhrasePolaritySentenceMapping] CHECK CONSTRAINT [FK_PhrasePolaritySentenceMapping_Phrase]
GO
ALTER TABLE [dbo].[PhrasePolaritySentenceMapping]  WITH CHECK ADD  CONSTRAINT [FK_PhrasePolaritySentenceMapping_Sentence] FOREIGN KEY([SentenceId])
REFERENCES [dbo].[Sentence] ([Id])
GO
ALTER TABLE [dbo].[PhrasePolaritySentenceMapping] CHECK CONSTRAINT [FK_PhrasePolaritySentenceMapping_Sentence]
GO
ALTER TABLE [dbo].[PhraseProductMapping]  WITH CHECK ADD  CONSTRAINT [FK_PhraseProductMapping_Phrase] FOREIGN KEY([PhraseId])
REFERENCES [dbo].[Phrase] ([Id])
GO
ALTER TABLE [dbo].[PhraseProductMapping] CHECK CONSTRAINT [FK_PhraseProductMapping_Phrase]
GO
ALTER TABLE [dbo].[PhraseProductMapping]  WITH CHECK ADD  CONSTRAINT [FK_PhraseProductMapping_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([Id])
GO
ALTER TABLE [dbo].[PhraseProductMapping] CHECK CONSTRAINT [FK_PhraseProductMapping_Product]
GO
ALTER TABLE [dbo].[PhraseTermMapping]  WITH CHECK ADD  CONSTRAINT [FK_PhraseTermMapping_Phrase] FOREIGN KEY([PhraseId])
REFERENCES [dbo].[Phrase] ([Id])
GO
ALTER TABLE [dbo].[PhraseTermMapping] CHECK CONSTRAINT [FK_PhraseTermMapping_Phrase]
GO
ALTER TABLE [dbo].[PhraseTermMapping]  WITH CHECK ADD  CONSTRAINT [FK_PhraseTermMapping_Term] FOREIGN KEY([TermId])
REFERENCES [dbo].[Term] ([Id])
GO
ALTER TABLE [dbo].[PhraseTermMapping] CHECK CONSTRAINT [FK_PhraseTermMapping_Term]
GO
ALTER TABLE [dbo].[Review]  WITH CHECK ADD  CONSTRAINT [FK_Review_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([Id])
GO
ALTER TABLE [dbo].[Review] CHECK CONSTRAINT [FK_Review_Product]
GO
ALTER TABLE [dbo].[Sentence]  WITH CHECK ADD  CONSTRAINT [FK_Sentence_DatasetType] FOREIGN KEY([DatasetTypeId])
REFERENCES [dbo].[DatasetType] ([Id])
GO
ALTER TABLE [dbo].[Sentence] CHECK CONSTRAINT [FK_Sentence_DatasetType]
GO
ALTER TABLE [dbo].[Sentence]  WITH CHECK ADD  CONSTRAINT [FK_Sentence_Review] FOREIGN KEY([ReviewId])
REFERENCES [dbo].[Review] ([Id])
GO
ALTER TABLE [dbo].[Sentence] CHECK CONSTRAINT [FK_Sentence_Review]
GO
ALTER TABLE [dbo].[SentencePhraseMapping]  WITH CHECK ADD  CONSTRAINT [FK_SentencePhraseMapping_Sentence] FOREIGN KEY([PhraseId])
REFERENCES [dbo].[Phrase] ([Id])
GO
ALTER TABLE [dbo].[SentencePhraseMapping] CHECK CONSTRAINT [FK_SentencePhraseMapping_Sentence]
GO
ALTER TABLE [dbo].[SentencePhraseMapping]  WITH CHECK ADD  CONSTRAINT [FK_SentencePhraseMapping_Sentence1] FOREIGN KEY([SentenceId])
REFERENCES [dbo].[Sentence] ([Id])
GO
ALTER TABLE [dbo].[SentencePhraseMapping] CHECK CONSTRAINT [FK_SentencePhraseMapping_Sentence1]
GO
ALTER TABLE [dbo].[SentencePhraseTermMapping]  WITH CHECK ADD  CONSTRAINT [FK_SentencePhraseTermMapping_PhraseTermMapping] FOREIGN KEY([PhraseTermId])
REFERENCES [dbo].[PhraseTermMapping] ([Id])
GO
ALTER TABLE [dbo].[SentencePhraseTermMapping] CHECK CONSTRAINT [FK_SentencePhraseTermMapping_PhraseTermMapping]
GO
ALTER TABLE [dbo].[SentencePhraseTermMapping]  WITH CHECK ADD  CONSTRAINT [FK_SentencePhraseTermMapping_SentencePhraseMapping] FOREIGN KEY([SentencePhraseId])
REFERENCES [dbo].[SentencePhraseMapping] ([Id])
GO
ALTER TABLE [dbo].[SentencePhraseTermMapping] CHECK CONSTRAINT [FK_SentencePhraseTermMapping_SentencePhraseMapping]
GO
ALTER TABLE [dbo].[Term]  WITH CHECK ADD  CONSTRAINT [FK_Term_POSCode] FOREIGN KEY([POSId])
REFERENCES [dbo].[POSType] ([Id])
GO
ALTER TABLE [dbo].[Term] CHECK CONSTRAINT [FK_Term_POSCode]
GO

INSERT INTO POSType VALUES (1, 'Noun Phrase')
INSERT INTO POSType VALUES (2, 'Adjective Phrase')
INSERT INTO POSType VALUES (3, 'Noun')
INSERT INTO POSType VALUES (4, 'Adjective')

INSERT INTO DatasetType VALUES (1, 'Hu & Liu')
INSERT INTO DatasetType VALUES (2, 'SemEval')