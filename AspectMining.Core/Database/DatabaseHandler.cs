using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectMining.Core.Database
{
    public class DatabaseHandler
    {
        public DatabaseType DBType;

        public String ConnectionString
        {
            get
            {
                switch (DBType)
                {
                    case DatabaseType.SqlCompactEdition35:
                        return SQLCompactEdition35.ConnectionString;
                    case DatabaseType.SqlServer:
                        return SQLServer.ConnectionString;

                    default:
                        return null;
                }
            }
        }

        private AspectMiningContext context;
        public AspectMiningContext Context
        {
            get
            {
                if (context == null) context = new AspectMiningContext(ConnectionString);
                return context;
            }
        }

        public DatabaseHandler(DatabaseType dbType)
        {
            DBType = dbType;
        }

        public void Initialize()
        {
            switch (DBType)
            {
                case DatabaseType.SqlServer:
                    //Do nothing database should be created by the provided MSSQLGenerate.sql file
                    break;
                case DatabaseType.SqlCompactEdition35:
                    //TODO: THE GENERATION QUERIES ARE OUTDATED!!! UPDATE FROM THE MSSQL SCHEMA
                    String dbWorkingCopy = ConfigurationManager.AppSettings["SQLCE3.5"];

                    if (!File.Exists(dbWorkingCopy))
                    {
                        File.Delete(dbWorkingCopy);

                        SqlCeEngine dbInstance = new SqlCeEngine(ConnectionString);
                        dbInstance.CreateDatabase();

                        List<String> sqlCEGenerateQueries = new List<string>();

                        sqlCEGenerateQueries.Add(@"CREATE TABLE [Product] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [Title] [nvarchar](1000) NULL,
	                                                [Description] [ntext] NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [Review] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [ProductId] [int] NOT NULL,
	                                                [Title] [nvarchar](1000) NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [Review] ADD  CONSTRAINT [FK_Review_Product] FOREIGN KEY([ProductId]) REFERENCES [Product] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [Sentence] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [ReviewId] [int] NOT NULL,
	                                                [Text] [ntext] NULL,
	                                                [TextPOS] [ntext] NULL,
	                                                [ManualResults] [ntext] NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [Sentence] ADD CONSTRAINT [FK_Sentence_Review] FOREIGN KEY([ReviewId]) REFERENCES [Review] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [Term] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [Text] [nvarchar](500) NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [Aspect] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [Text] [nvarchar](1500) NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [AspectTermMapping](
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [AspectId] [int] NOT NULL,
	                                                [TermId] [int] NOT NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [AspectTermMapping] ADD CONSTRAINT [FK_AspectTermMapping_Aspect] FOREIGN KEY([AspectId]) REFERENCES [Aspect] ([Id]);");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [AspectTermMapping] ADD CONSTRAINT [FK_AspectTermMapping_Term] FOREIGN KEY([TermId]) REFERENCES [Term] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [SentenceAspectMapping] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [SentenceId] [int] NOT NULL,
	                                                [AspectId] [int] NOT NULL,
                                                    [IsGenerated] [bit] NOT NULL DEFAULT(0)
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [SentenceAspectMapping] ADD CONSTRAINT [FK_SentenceAspectMapping_Sentence] FOREIGN KEY([SentenceId]) REFERENCES [Sentence] ([Id]);");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [SentenceAspectMapping] ADD CONSTRAINT [FK_SentenceAspectMapping_Aspect] FOREIGN KEY([AspectId]) REFERENCES [Aspect] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [SentenceAspectTermMapping](
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [SentenceAspectId] [int] NOT NULL,
	                                                [AspectTermId] [int] NOT NULL,
	                                                [TermOrder] [int] NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [SentenceAspectTermMapping] ADD CONSTRAINT [FK_SentenceAspectTermMapping_SentenceAspectMapping] FOREIGN KEY([SentenceAspectId]) REFERENCES [SentenceAspectMapping] ([Id]);");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [SentenceAspectTermMapping] ADD CONSTRAINT [FK_SentenceAspectTermMapping_AspectTermMapping] FOREIGN KEY([AspectTermId]) REFERENCES [AspectTermMapping] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [AspectProductMapping](
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [AspectId] [int] NOT NULL,
	                                                [ProductId] [int] NOT NULL,
                                                    [AspectText] [nvarchar](1500) NULL,
                                                    [Frequency] [int] NOT NULL DEFAULT(0),
                                                    [Compactness] [int] DEFAULT(0),
                                                    [PSupport] [int] DEFAULT(0),
                                                    [IsPruned] [bit] NOT NULL DEFAULT(0)
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [AspectProductMapping] ADD CONSTRAINT [FK_AspectProductMapping_Aspect] FOREIGN KEY([AspectId]) REFERENCES [Aspect] ([Id]);");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [AspectProductMapping] ADD CONSTRAINT [FK_AspectProductMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Product] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [ManualAspectProductMapping](
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [AspectId] [int] NOT NULL,
	                                                [ProductId] [int] NOT NULL,
                                                    [AspectText] [nvarchar](1500) NULL,
                                                    [Frequency] [int] NOT NULL DEFAULT(1)
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [ManualAspectProductMapping] ADD CONSTRAINT [FK_ManualAspectProductMapping_Aspect] FOREIGN KEY([AspectId]) REFERENCES [Aspect] ([Id]);");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [ManualAspectProductMapping] ADD CONSTRAINT [FK_ManualAspectProductMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Product] ([Id]);");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [OpinionWord] (
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [Text] [nvarchar](500) NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"CREATE TABLE [OpinionWordProductMapping](
	                                                [Id] [int] PRIMARY KEY IDENTITY(1,1),
	                                                [OpinionWordId] [int] NOT NULL,
	                                                [ProductId] [int] NOT NULL
                                                );");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [OpinionWordProductMapping] ADD CONSTRAINT [FK_OpinionWordProductMapping_OpinionWord] FOREIGN KEY([OpinionWordId]) REFERENCES [OpinionWord] ([Id]);");
                        sqlCEGenerateQueries.Add(@"ALTER TABLE [OpinionWordProductMapping] ADD CONSTRAINT [FK_OpinionWordProductMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Product] ([Id]);");

                        for (int i = 0; i < sqlCEGenerateQueries.Count; i++)
                        {
                            SQLCompactEdition35.ExecuteNonQuery(sqlCEGenerateQueries[i]);
                        }
                    }
                    break;
            }
        }

        public void ResetData(int? id)
        {
            List<String> sqlResetQueries = new List<string>();

            switch (DBType)
            {
                case DatabaseType.SqlCompactEdition35:
                    //TODO: THE RESET QUERIES ARE OUTDATED!!! UPDATE FROM THE MSSQL SCHEMA
                    sqlResetQueries.Add(string.Format(@"DELETE FROM SentenceAspectTermMapping WHERE Id IN (
                                                        SELECT satm.Id FROM SentenceAspectTermMapping satm
												        INNER JOIN SentenceAspectMapping sam on satm.SentenceAspectId = sam.Id
                                                        INNER JOIN Sentence s on s.Id = sam.SentenceId
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0});", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM SentenceAspectTermMapping WHERE Id IN (
                                                        SELECT satm.Id FROM SentenceAspectTermMapping satm
                                                        INNER JOIN SentenceAspectMapping sam on satm.SentenceAspectId = sam.Id
                                                        INNER JOIN Sentence s on s.Id = sam.SentenceId
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0});", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM SentenceAspectMapping WHERE Id IN (
                                                        SELECT sam.Id FROM SentenceAspectMapping sam
                                                        INNER JOIN Sentence s on s.Id = sam.SentenceId
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0});", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM Sentence WHERE Id IN (
                                                        SELECT s.Id FROM Sentence s 
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0});", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM Review WHERE Id IN (
                                                        SELECT r.Id FROM Review r
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0});", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM AspectProductMapping WHERE ProductId = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM ManualAspectProductMapping WHERE ProductId = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE FROM Product WHERE Id = {0};", id));
                    break;
                case DatabaseType.SqlServer:
                    sqlResetQueries.Add(string.Format(@"DELETE sptm FROM SentencePhraseTermMapping sptm
                                                        INNER JOIN SentencePhraseMapping spm on sptm.SentencePhraseId = spm.Id
                                                        INNER JOIN Sentence s on s.Id = spm.SentenceId
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE spm FROM SentencePhraseMapping spm
                                                        INNER JOIN Sentence s on s.Id = spm.SentenceId
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE ppsm FROM PhrasePolaritySentenceMapping ppsm
                                                        INNER JOIN Sentence s on s.Id = ppsm.SentenceId
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE s FROM Sentence s
                                                        INNER JOIN Review r on r.Id = s.ReviewId
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE r FROM Review r
                                                        INNER JOIN Product p on p.Id = r.ProductId
                                                        WHERE p.Id = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE ppm FROM PhraseProductMapping ppm WHERE ppm.ProductId = {0};", id));
                    sqlResetQueries.Add(string.Format(@"DELETE p FROM Product p WHERE p.Id = {0};", id));
                    break;
            }

            for (int i = 0; i < sqlResetQueries.Count; i++)
            {
                ExecuteNonQuery(sqlResetQueries[i]);
            }
        }

        public DataTable ExecuteDataTable(String query, List<SqlParameter> paramerters = null, SqlConnection cnn = null)
        {
            if (paramerters == null) paramerters = new List<SqlParameter>();

            switch (DBType)
            {
                case DatabaseType.SqlCompactEdition35:
                    return SQLCompactEdition35.ExecuteDataTable(query);
                case DatabaseType.SqlServer:
                    if (cnn != null)
                        return SQLServer.ExecuteDataTable(cnn, query, paramerters);
                    else
                        return SQLServer.ExecuteDataTable(query, paramerters);
                default:
                    return null;
            }
        }

        public void ExecuteNonQuery(String query, List<SqlParameter> paramerters = null, SqlConnection cnn = null)
        {
            if (paramerters == null) paramerters = new List<SqlParameter>();

            switch (DBType)
            {
                case DatabaseType.SqlCompactEdition35:
                    SQLCompactEdition35.ExecuteNonQuery(query);
                    break;
                case DatabaseType.SqlServer:
                    if (cnn != null)
                        SQLServer.ExecuteNonQuery(cnn, query, paramerters);
                    else
                        SQLServer.ExecuteNonQuery(query, paramerters);
                    break;
            }
        }
    }

    public enum DatabaseType
    {
        SqlServer,
        SqlCompactEdition35
    }
}
