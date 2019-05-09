using System;
using java.io;
using opennlp.tools.chunker;
using opennlp.tools.postag;
using opennlp.tools.tokenize;

namespace JavaLibraries.OpenNLP
{
    public class OpenNLPChunker
    {
        public String TokenizerModel;
        public String POSModel;
        public String ChunkerModel;
        private Tokenizer tokenizer;
        private POSTagger tagger;
        private Chunker chunker;

        public OpenNLPChunker() { }
        public OpenNLPChunker(String tokenizerModel, String posModel, String chunkerModel)
        {
            TokenizerModel = tokenizerModel;
            POSModel = posModel;
            ChunkerModel = chunkerModel;
        }

        private void InitializeTokenizer()
        {
            InputStream modelIn = null;
            try
            {
                modelIn = new FileInputStream(TokenizerModel);
                TokenizerModel model = new TokenizerModel(modelIn);
                tokenizer = new TokenizerME(model);
            }
            catch (IOException ex)
            {
                tokenizer = null;
            }
            finally
            {
                if (modelIn != null)
                {
                    try
                    {
                        modelIn.close();
                    }
                    catch (IOException ex)
                    {
                    }
                }
            }
        }

        private void InitializePOSTagger()
        {
            InputStream modelIn = null;
            try
            {
                modelIn = new FileInputStream(POSModel);
                POSModel model = new POSModel(modelIn);
                tagger = new POSTaggerME(model);
            }
            catch (IOException ex)
            {
                tagger = null;
            }
            finally
            {
                if (modelIn != null)
                {
                    try
                    {
                        modelIn.close();
                    }
                    catch (IOException ex)
                    {
                    }
                }
            }
        }

        private void InitializeChunker()
        {
            InputStream modelIn = null;
            try
            {
                modelIn = new FileInputStream(ChunkerModel);
                ChunkerModel model = new ChunkerModel(modelIn);
                chunker = new ChunkerME(model);
            }
            catch (IOException ex)
            {
                chunker = null;
            }
            finally
            {
                if (modelIn != null)
                {
                    try
                    {
                        modelIn.close();
                    }
                    catch (IOException ex)
                    {
                    }
                }
            }
        }

        public ChunkedText Process(String sentence)
        {
            ChunkedText result = null;

            if (tokenizer == null)
            {
                InitializeTokenizer();
            }
            if (tagger == null)
            {
                InitializePOSTagger();
            }
            if (chunker == null)
            {
                InitializeChunker();
            }

            if (tokenizer != null && tagger != null & chunker != null)
            {
                result = new ChunkedText();

                result.Text = sentence; //.Replace("'s", "");  //To prevent the chunker categorizing 's as a term
                result.TokenArray = tokenizer.tokenize(result.Text);
                result.POSTagArray = tagger.tag(result.TokenArray);
                result.ChunkTagArray = chunker.chunk(result.TokenArray, result.POSTagArray);
            }

            return result;
        }
    }
}
