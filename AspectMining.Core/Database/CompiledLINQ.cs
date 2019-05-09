using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectMining.Core.Database
{
    public static class CompiledLINQ
    {
        public static Func<DataContext, string, short, IEnumerable<Term>> FindTerm = CompiledQuery.Compile(
            (DataContext db, string searchTerm, short posTypeId) =>
                from u in db.GetTable<Term>()
                where u.PreprocessedText == searchTerm && u.POSId == posTypeId
                select u);

        public static Func<DataContext, int, IEnumerable<Phrase>> FindPhrase = CompiledQuery.Compile(
            (DataContext db, int phraseId) =>
                from u in db.GetTable<Phrase>()
                where u.Id == phraseId
                select u
            );

        public static Func<DataContext, int, int, bool, IEnumerable<PhraseProductMapping>> FindPhraseProductMapping = CompiledQuery.Compile(
            (DataContext db, int phraseId, int productId, bool isManual) =>
                from u in db.GetTable<PhraseProductMapping>()
                where u.Phrase.Id == phraseId && u.ProductId == productId && u.IsManual == isManual
                select u
            );
    }
}
