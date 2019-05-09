using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectMining.Core;
using AspectMining.Core.Database;
using AspectMining.Core.Dataset;
using Tools;

namespace AspectMining.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            DatasetParserType parserType = DatasetParserType.Undefined;
            String filename = String.Empty;

            //args = new[] { "-f=SemEval\\SemEval_Laptops_Train.xml", "-p=SemEval" };

            foreach (string arg in args)
            {
                switch (arg.Substring(0, 3))
                {
                    case "-f=":
                        filename = arg.Substring(3);
                        break;
                    case "-p=":
                        switch (arg.Substring(3).ToLower())
                        {
                            case "huliu":
                                parserType = DatasetParserType.HuLiu;
                                break;
                            case "semeval":
                                parserType = DatasetParserType.SemEval;
                                break;
                            default:
                                throw new Exception("Invalid parser type! Available parsers: SemEval, HuLiu ");
                        }
                        break;
                }
            }

            if (String.IsNullOrEmpty(filename))
                throw new Exception("Filename not defined!");

            Stopwatch timer = Stopwatch.StartNew();

            //Set correct dataset filename and path
            String filePath = Path.GetFullPath(Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DatasetDirectory"] + filename);
            FileInfo fileInfo = new FileInfo(filePath);

            //Copy spellchecker files
            string[] hunspellFiles = Directory.GetFiles(Path.GetFullPath(Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["HunspellFiles"]));
            foreach (string hunspellFile in hunspellFiles)
            {
                FileInfo info = new FileInfo(hunspellFile);
                File.Copy(hunspellFile, Path.GetFullPath(Directory.GetCurrentDirectory() + "\\" + info.Name), true);
            }

            DatabaseHandler databaseHandler = new DatabaseHandler(DatabaseType.SqlServer);
            AspectMiningContext context = databaseHandler.Context;
            IDatasetParser parser;

            switch (parserType)
            {
                case DatasetParserType.HuLiu:
                    parser = new HLDatasetParser();
                    break;
                case DatasetParserType.SemEval:
                    parser = new SemEvalParser();
                    break;
                default:
                    throw new Exception("Parser type not defined or not implemented");
                    break;
            }
            
            Console.Write("Initializing database... ");
            timer.Restart();
            databaseHandler.Initialize();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Reseting records for {0}... ", filename);
            timer.Restart();
            databaseHandler.ResetData((from u in context.Product where u.Title == fileInfo.Name select u.Id).FirstOrDefault());
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Parsing dataset file: {0}... ", filename);
            timer.Restart();
            Product productSummary = parser.ParseDataset(filePath);
            if (productSummary == null || productSummary.Review.Count == 0)
                throw new Exception("Parsed dataset return an empty object. Check the dataset and/or the provided parser type.");
            context.Product.InsertOnSubmit(productSummary);
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Commiting data to the database... ");
            timer.Restart();
            context.SubmitChanges();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);
            
            AspectHandler aspectHandler = new AspectHandler(databaseHandler, (from u in context.Product where u.Title == fileInfo.Name select u.Id).FirstOrDefault());
            
            Console.Write("Starting Part-Of-Speech Tagging... ");
            timer.Restart();
            aspectHandler.ApplyPOSTagging(Path.GetFullPath(Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["OpenNLPModelsTokenizer"]),
                                          Path.GetFullPath(Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["OpenNLPModelsPOSTagger"]),
                                          Path.GetFullPath(Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["OpenNLPModelsChunker"]));
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Extracting aspects... ");
            timer.Restart();
            aspectHandler.ExtractAspects();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Generating additional aspects... ");
            timer.Restart();
            aspectHandler.GenerateAspects();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Extracting optimal results... ");
            timer.Restart();
            aspectHandler.ExtractOptimalResults();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Calculating aspect term order... ");
            timer.Restart();
            aspectHandler.CalculateTermOrder(DistinctChunkTags.NounPhrase);
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Appling fuzzy matching on aspects... ");
            timer.Restart();
            aspectHandler.ApplyAspectFuzzyMatching(DistinctChunkTags.NounPhrase);
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Extracting opinion words... ");
            timer.Restart();
            aspectHandler.ExtractOpinionWords();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Calculating opinion word order... ");
            timer.Restart();
            aspectHandler.CalculateTermOrder(DistinctChunkTags.AdjectivePhrase);
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Appling fuzzy matching on opinion words... ");
            timer.Restart();
            aspectHandler.ApplyAspectFuzzyMatching(DistinctChunkTags.AdjectivePhrase);
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Calculating compactness... ");
            timer.Restart();
            aspectHandler.CalculateCompactness();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Calculating p-support... ");
            timer.Restart();
            aspectHandler.CalculatePSupport();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Pruning aspects... ");
            timer.Restart();
            aspectHandler.PruneAspects();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Extracting infrequent aspects... ");
            timer.Restart();
            aspectHandler.ExtractInfrequentAspects();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);
            
            Console.Write("Initializing SentiWordNet... ");
            timer.Restart();
            String sentiWordNetFilePath = Path.GetFullPath(Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["SentiWordNetSource"]);
            aspectHandler.InitializeSentiWordNet(sentiWordNetFilePath);
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.Write("Calculating polarity... ");
            timer.Restart();
            aspectHandler.CalculatePolarity();
            timer.Stop();
            Console.WriteLine("OK ({0:mm}min {0:ss}sec {0:FF}ms)", timer.Elapsed);

            Console.WriteLine("Done! (Press any key to exit)");
            Console.ReadKey();
        }
    }
}
